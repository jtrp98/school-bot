using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using ServiceSchoolBot.Application.Databases.Repository.Interfaces;
using ServiceSchoolBot.Domain.Assessment;
using ServiceSchoolBot.Infrastructure.Common.Databases;
using ServiceSchoolBot.Infrastructure.Entities.SchoolBotDb;

namespace ServiceSchoolBot.Infrastructure.Repositories.Databases;

public sealed partial class AssessmentRepository
{
    private static SchoolBotQuestionBankRecord From(SchoolBotQuestionBank row) => new(
        row.SchoolBotQuestionBankId,
        row.BankKey,
        row.Revision,
        row.Title,
        JsonSerializer.Deserialize<IReadOnlyList<QuestionBankQuestion>>(row.Questions) ?? throw new InvalidOperationException("Invalid stored Questions."),
        row.SourceRef,
        row.Lifecycle,
        Offset(row.PublishedAt),
        row.IsDelete,
        row.CreateBy,
        Offset(row.CreateDate),
        row.UpdateBy,
        Offset(row.UpdateDate),
        row.SchoolId);

    private static SchoolBotQuestionBank To(SchoolBotQuestionBankRecord row) => new()
    {
        SchoolBotQuestionBankId = row.SchoolBotQuestionBankId,
        BankKey = row.BankKey,
        Revision = row.Revision,
        Title = row.Title,
        Questions = JsonSerializer.Serialize(row.Questions),
        SourceRef = row.SourceRef,
        Lifecycle = row.Lifecycle,
        PublishedAt = row.PublishedAt?.UtcDateTime,
        IsDelete = row.IsDelete,
        CreateBy = row.CreateBy,
        CreateDate = row.CreateDate?.UtcDateTime,
        UpdateBy = row.UpdateBy,
        UpdateDate = row.UpdateDate?.UtcDateTime,
        SchoolId = row.SchoolId
    };

    private static SchoolBotCourseRunRecord From(SchoolBotCourseRun row) => new(
        row.SchoolBotCourseRunId,
        row.CurriculumId,
        row.ClassroomId,
        row.Nyear,
        row.Nterm,
        row.PlanCourseIds.ToArray(),
        row.CurrentPolicyId,
        row.PreviousRunId,
        Offset(row.EffectiveAt),
        row.Lifecycle,
        row.IsDelete,
        row.CreateBy,
        Offset(row.CreateDate),
        row.UpdateBy,
        Offset(row.UpdateDate),
        row.SchoolId);

    private static SchoolBotCourseRun To(SchoolBotCourseRunRecord row) => new()
    {
        SchoolBotCourseRunId = row.SchoolBotCourseRunId,
        CurriculumId = row.CurriculumId,
        ClassroomId = row.ClassroomId,
        Nyear = row.NYear,
        Nterm = row.NTerm,
        PlanCourseIds = row.PlanCourseIds.ToList(),
        CurrentPolicyId = row.CurrentPolicyId,
        PreviousRunId = row.PreviousRunId,
        EffectiveAt = row.EffectiveAt.UtcDateTime,
        Lifecycle = row.Lifecycle,
        IsDelete = row.IsDelete,
        CreateBy = row.CreateBy,
        CreateDate = row.CreateDate?.UtcDateTime,
        UpdateBy = row.UpdateBy,
        UpdateDate = row.UpdateDate?.UtcDateTime,
        SchoolId = row.SchoolId
    };

    private static SchoolBotCourseRunActivityRecord From(SchoolBotCourseRunActivity row) => new(
        row.SchoolBotCourseRunActivityId,
        row.CourseRunId,
        row.ActivityId,
        Offset(row.OpensAt),
        Offset(row.ClosesAt),
        row.PolicyOverride,
        row.IsDelete,
        row.CreateBy,
        Offset(row.CreateDate),
        row.UpdateBy,
        Offset(row.UpdateDate),
        row.SchoolId);

    private static SchoolBotCourseRunActivity To(SchoolBotCourseRunActivityRecord row) => new()
    {
        SchoolBotCourseRunActivityId = row.SchoolBotCourseRunActivityId,
        CourseRunId = row.CourseRunId,
        ActivityId = row.ActivityId,
        OpensAt = row.OpensAt?.UtcDateTime,
        ClosesAt = row.ClosesAt?.UtcDateTime,
        PolicyOverride = row.PolicyOverride,
        IsDelete = row.IsDelete,
        CreateBy = row.CreateBy,
        CreateDate = row.CreateDate?.UtcDateTime,
        UpdateBy = row.UpdateBy,
        UpdateDate = row.UpdateDate?.UtcDateTime,
        SchoolId = row.SchoolId
    };

    private static SchoolBotAttemptRecord From(SchoolBotAttempt row) => new(
        row.SchoolBotAttemptId,
        row.CourseRunId,
        row.ActivityId,
        row.LearnerId,
        row.Sequence,
        row.State,
        Offset(row.StartedAt),
        Offset(row.DeadlineAt),
        Offset(row.SubmittedAt),
        row.Snapshot,
        row.Answers,
        row.EarnedScore,
        row.MaxScore,
        row.IsDelete,
        row.CreateBy,
        Offset(row.CreateDate),
        row.UpdateBy,
        Offset(row.UpdateDate),
        row.SchoolId);

    private static SchoolBotAttempt To(SchoolBotAttemptRecord row) => new()
    {
        SchoolBotAttemptId = row.SchoolBotAttemptId,
        CourseRunId = row.CourseRunId,
        ActivityId = row.ActivityId,
        LearnerId = row.LearnerId,
        Sequence = row.Sequence,
        State = row.State,
        StartedAt = row.StartedAt.UtcDateTime,
        DeadlineAt = row.DeadlineAt?.UtcDateTime,
        SubmittedAt = row.SubmittedAt?.UtcDateTime,
        Snapshot = row.Snapshot,
        Answers = row.Answers,
        EarnedScore = row.EarnedScore,
        MaxScore = row.MaxScore,
        IsDelete = row.IsDelete,
        CreateBy = row.CreateBy,
        CreateDate = row.CreateDate?.UtcDateTime,
        UpdateBy = row.UpdateBy,
        UpdateDate = row.UpdateDate?.UtcDateTime,
        SchoolId = row.SchoolId
    };

    private static DateTimeOffset Offset(DateTime value) => new(DateTime.SpecifyKind(value, DateTimeKind.Utc));
    private static DateTimeOffset? Offset(DateTime? value) => value is { } date ? Offset(date) : null;
}
