using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ServiceSchoolBot.Application.Common.Model.Assessment;
using ServiceSchoolBot.Domain;
using ServiceSchoolBot.Domain.Assessment;
using ServiceSchoolBot.Infrastructure.Common.Databases;
using ServiceSchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

namespace ServiceSchoolBot.Infrastructure.Repositories.Databases;

public sealed class SchoolAssessmentAuthority(JabjaiSchoolSingleDBPostgreSQLDBContext db,
    IConfiguration configuration, TimeProvider clock) : IAssessmentAuthority
{
    public Task<bool> CanReadRunAsync(AssessmentPrincipalContext principal, SchoolBotCourseRunRecord run,
        CancellationToken ct = default) => Read(async () =>
    {
        if (!SameSchool(principal, run)) return false;
        await RequirePeriod(run, ct);
        if (principal.ProfileKind == "employee" && principal.Session.Channel == "school_web")
            return await Assigned(principal, run, ct);
        if (principal.ProfileKind != "student" || principal.Session.Channel != "desktop" || principal.LearnerId is not > 0)
            return false;
        return await LearnerInRun(run, principal.LearnerId.Value, ct);
    });

    public Task<bool> CanReviewLearnerAsync(AssessmentPrincipalContext principal, SchoolBotCourseRunRecord run,
        int learnerId, CancellationToken ct = default) => Read(async () =>
    {
        if (!SameSchool(principal, run) || learnerId <= 0 || !await Assigned(principal, run, ct)) return false;
        await RequirePeriod(run, ct);
        return await LearnerInRun(run, learnerId, ct);
    });

    public Task<IReadOnlyList<int>> GetLearnerIdsAsync(AssessmentPrincipalContext principal, SchoolBotCourseRunRecord run,
        CancellationToken ct = default) => Read<IReadOnlyList<int>>(async () =>
    {
        if (!SameSchool(principal, run) || !await Assigned(principal, run, ct))
            throw new DomainRuleException(ErrorCodes.ProfileNotAllowed, "Current subject teacher assignment is required.");
        var period = await RequirePeriod(run, ct);
        var candidates = await db.TstudentClassroomHistories.AsNoTracking().Where(h => h.SchoolId == run.SchoolId &&
            h.NTerm == run.NTerm && h.NTermSubLevel2 == run.ClassroomId && !h.CDel).Select(h => h.SId).Distinct().Take(1001).ToArrayAsync(ct);
        if (candidates.Length > 1000 || candidates.Any(id => id is null or <= 0)) throw Unconfigured("Classroom history is incomplete or exceeds the supported roster size.");
        var ids = candidates.Select(id => id!.Value).Order().ToArray();
        var histories = await db.TstudentClassroomHistories.AsNoTracking().Where(h => h.SchoolId == run.SchoolId &&
            h.NTerm == run.NTerm && !h.CDel && h.SId != null && ids.Contains(h.SId.Value)).Take(20001).ToArrayAsync(ct);
        if (histories.Length > 20000) throw Unconfigured("Classroom history must be reconciled before evaluating the whole class.");
        var active = await db.Tusers.AsNoTracking().Where(u => u.SchoolId == run.SchoolId && ids.Contains(u.SId) &&
            (u.CDel == null || u.CDel == '0')).Select(u => u.SId).ToArrayAsync(ct);
        var enrolled = await Enrollments(run, ids, ct);
        foreach (var id in ids)
        {
            // Excluding an unresolved student would silently change the whole-class average.
            if (!ResolveMembership(histories.Where(h => h.SId == id).ToArray(), run, period) || !active.Contains(id) ||
                run.PlanCourseIds.Any(course => !enrolled.Contains((id, course))))
                throw Unconfigured("The complete classroom roster cannot be resolved from unique active history and explicit course enrollments.");
        }
        return ids;
    });

    public Task<ClassroomLearningContext> GetClassroomContextAsync(AssessmentPrincipalContext principal, SchoolBotCourseRunRecord run,
        CancellationToken ct = default) => Read(async () =>
    {
        var ids = await GetLearnerIdsAsync(principal, run, ct);
        var current = await RequirePeriod(run, ct);
        var end = Utc(current.DEnd!.Value);
        if (clock.GetUtcNow() <= end) throw new DomainRuleException("TERM_NOT_ENDED", "The authoritative school term has not ended.");
        var periods = await (from t in db.Tterms.AsNoTracking()
            join y in db.Tyears.AsNoTracking() on new { t.SchoolId, NYear = t.NYear } equals new { y.SchoolId, NYear = (int?)y.NYear }
            where t.SchoolId == run.SchoolId && (t.CDel == null || t.CDel == '0') && !y.CDel && t.DStart > current.DEnd
            orderby t.DStart
            select t).Take(2).ToArrayAsync(ct);
        if (periods.Length == 0 || periods[0].NYear is not > 0 || periods[0].DStart is null || periods[0].DEnd is null ||
            periods[0].DEnd < periods[0].DStart || periods.Length > 1 && periods[0].DStart == periods[1].DStart)
            throw new DomainRuleException("NEXT_TERM_NOT_AVAILABLE", "The next registered school term must be unambiguous.");
        var next = periods[0];
        var effective = Utc(next.DStart!.Value);
        return new ClassroomLearningContext(ids, next.NYear!.Value, next.NTerm, effective,
            CanonicalJson.Hash(new { run.SchoolId, run.ClassroomId, run.NYear, run.NTerm, ids, end, effective, nextTerm = next.NTerm, nextYear = next.NYear }));
    });

    private static bool SameSchool(AssessmentPrincipalContext p, SchoolBotCourseRunRecord r) =>
        r.SchoolId > 0 && r.ClassroomId > 0 && !r.IsDelete && p.Session.SchoolId == r.SchoolId &&
        p.Session.IsAuthConfigured && !p.Session.IsSessionExpired && p.Session.HasActiveSchoolLicense &&
        !string.IsNullOrWhiteSpace(p.Session.PrincipalId);

    private async Task<Tterm> RequirePeriod(SchoolBotCourseRunRecord run, CancellationToken ct)
    {
        if (run.PlanCourseIds.Count == 0 || run.PlanCourseIds.Count > 128 || run.PlanCourseIds.Any(id => id <= 0) ||
            run.PlanCourseIds.Distinct().Count() != run.PlanCourseIds.Count) throw Unconfigured("Course run subjects are incomplete.");
        var term = await (from t in db.Tterms.AsNoTracking()
            join y in db.Tyears.AsNoTracking() on new { t.SchoolId, NYear = t.NYear } equals new { y.SchoolId, NYear = (int?)y.NYear }
            where t.SchoolId == run.SchoolId && t.NTerm == run.NTerm && t.NYear == run.NYear &&
                (t.CDel == null || t.CDel == '0') && !y.CDel
            select t).SingleOrDefaultAsync(ct);
        if (term?.DStart is null || term.DEnd is null || term.DEnd < term.DStart)
            throw Unconfigured("Registered term dates and year are required.");
        _ = Utc(term.DStart.Value);
        _ = Utc(term.DEnd.Value);
        return term;
    }

    private async Task<bool> Assigned(AssessmentPrincipalContext p, SchoolBotCourseRunRecord run, CancellationToken ct)
    {
        if (p.ProfileKind != "employee" || p.Session.Channel != "school_web" || p.EmployeeId is not > 0) return false;
        var plans = await (from teacher in db.TplanCourseTeachers.AsNoTracking()
            join course in db.TplanCourses.AsNoTracking() on new { teacher.SchoolId, teacher.PlanCourseId } equals new { course.SchoolId, course.PlanCourseId }
            join term in db.TplanCourseTerms.AsNoTracking() on new { course.SchoolId, course.PlanCourseId } equals new { term.SchoolId, term.PlanCourseId }
            join roomPlan in db.TplanTermSubLevel2s.AsNoTracking() on new { course.SchoolId, course.PlanId } equals new { roomPlan.SchoolId, roomPlan.PlanId }
            join room in db.TtermSubLevel2s.AsNoTracking() on new { roomPlan.SchoolId, roomPlan.NTermSubLevel2 } equals new { room.SchoolId, room.NTermSubLevel2 }
            join employee in db.Temployees.AsNoTracking() on new { teacher.SchoolId, teacher.SEmp } equals new { employee.SchoolId, employee.SEmp }
            where teacher.SchoolId == run.SchoolId && teacher.SEmp == p.EmployeeId && teacher.IsActive == true && !teacher.CDel &&
                course.IsActive == true && !course.CDel && course.CourseStatus == 1 && course.NYear == run.NYear &&
                term.NTerm == run.NTerm && term.IsActive == true && !term.CDel && roomPlan.IsActive == true && !roomPlan.CDel &&
                room.NTermSubLevel2 == run.ClassroomId && !room.CDel && room.NWorkingStatus == 1 &&
                (employee.CDel == null || employee.CDel == '0') && run.PlanCourseIds.Contains(course.PlanCourseId)
            select course.PlanCourseId).Distinct().ToArrayAsync(ct);
        // The current API exposes an entire run, so a partial subject assignment cannot authorize it.
        return run.PlanCourseIds.Count > 0 && plans.Length == run.PlanCourseIds.Count;
    }

    private async Task<HashSet<(int Student, int Course)>> Enrollments(SchoolBotCourseRunRecord run, IReadOnlyList<int> ids, CancellationToken ct) =>
        (await db.TplanCourseStudents.AsNoTracking().Where(e => e.SchoolId == run.SchoolId && e.NTerm == run.NTerm &&
            e.NTermSubLevel2 == run.ClassroomId && e.IsActive && !e.CDel && ids.Contains(e.SId) && run.PlanCourseIds.Contains(e.PlanCourseId))
            .Select(e => new { e.SId, e.PlanCourseId }).Distinct().ToArrayAsync(ct)).Select(e => (e.SId, e.PlanCourseId)).ToHashSet();

    private async Task<bool> LearnerInRun(SchoolBotCourseRunRecord run, int learnerId, CancellationToken ct)
    {
        var period = await RequirePeriod(run, ct);
        var histories = await db.TstudentClassroomHistories.AsNoTracking().Where(h => h.SchoolId == run.SchoolId &&
            h.SId == learnerId && h.NTerm == run.NTerm && !h.CDel).Take(3).ToArrayAsync(ct);
        if (!ResolveMembership(histories, run, period)) return false;
        if (!await db.Tusers.AsNoTracking().AnyAsync(u => u.SchoolId == run.SchoolId && u.SId == learnerId && (u.CDel == null || u.CDel == '0'), ct)) return false;
        var enrolled = await Enrollments(run, [learnerId], ct);
        return run.PlanCourseIds.All(id => enrolled.Contains((learnerId, id)));
    }

    private bool ResolveMembership(IReadOnlyList<TstudentClassroomHistory> histories, SchoolBotCourseRunRecord run, Tterm term)
    {
        if (histories.Count == 0) return false;
        if (histories.Count != 1) throw Unconfigured("Duplicate classroom histories require resolution; history IDs do not grant membership.");
        var h = histories[0];
        if (h.NTermSubLevel2 != run.ClassroomId) return false;
        if (h.IsActive != true || h.NStudentStatus != 0 || h.MoveOutDate is not null)
            throw Unconfigured("Moved or non-active student histories require the legacy roster resolver.");
        var asOf = clock.GetUtcNow();
        var end = Utc(term.DEnd!.Value);
        if (asOf > end) asOf = end;
        return h.MoveInDate is null || Utc(h.MoveInDate.Value) <= asOf;
    }

    private DateTimeOffset Utc(DateTime value)
    {
        var name = configuration["SchoolBot:LegacyAuthority:TimeZoneId"];
        if (string.IsNullOrWhiteSpace(name)) throw Unconfigured("The school database timestamp time zone must be configured explicitly.");
        TimeZoneInfo zone;
        try { zone = TimeZoneInfo.FindSystemTimeZoneById(name); }
        catch (Exception ex) when (ex is TimeZoneNotFoundException or InvalidTimeZoneException) { throw Unconfigured("School time zone is unavailable."); }
        var local = DateTime.SpecifyKind(value, DateTimeKind.Unspecified);
        if (zone.IsAmbiguousTime(local) || zone.IsInvalidTime(local)) throw Unconfigured("School term or movement time is ambiguous.");
        return new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(local, zone));
    }

    private static async Task<T> Read<T>(Func<Task<T>> read)
    {
        try { return await read(); }
        catch (DbException) { throw new DomainRuleException("AUTH_UNAVAILABLE", "School authority source is unavailable."); }
    }
    private static DomainRuleException Unconfigured(string reason) => new(ErrorCodes.AuthNotConfigured, reason);
}
