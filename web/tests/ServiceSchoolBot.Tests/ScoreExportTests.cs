using System.Text.Json;
using ServiceSchoolBot.Application.Common.Model.Assessment;
using ServiceSchoolBot.Application.Common.Model.Curriculum;
using ServiceSchoolBot.Application.Services.Implementations.Assessment;
using ServiceSchoolBot.Domain;
using ServiceSchoolBot.Domain.Assessment;
using ServiceSchoolBot.Domain.Concurrency;
using ServiceSchoolBot.Infrastructure.Repositories.Storage;

namespace ServiceSchoolBot.Tests;

public sealed class ScoreExportTests
{
    private static AssessmentPrincipalContext Student => new(CurriculumPrincipalStub.CreateDesktopStudent("301") with { SchoolId = 11 }, null, "student", 301);
    private static AssessmentPrincipalContext Teacher => new(Student.Session with { Channel = "school_web", PrincipalId = "teacher:77", IsSchoolAdmin = true }, 77, "employee");
    private static async Task<Authority> Prepare(CourseRunFixture fixture, bool graded = true)
    {
        await fixture.Initialize();
        await ((ICurriculumBackingStore)fixture.Store()).MutateCurriculumUnitAsync((s, _) => Task.FromResult(new CurriculumStoreMutation<bool>(s with
        {
            CourseRuns = s.CourseRuns!.Select(r => r with { CurrentPolicyId = 1 }).ToArray(),
            ScoringPolicies = [new(1, 11, 1, 1, new(false, [new(4, 0), new(5, 0), new(6, 0), new(7, 0), new(8, 10000)], "weighted-normalized-v1"), 77, fixture.Clock.GetUtcNow())]
        }, true)));
        var authority = new Authority(fixture);
        if (graded)
        {
            var repo = fixture.Reader();
            var exams = new AssessmentReadService(repo, authority, fixture.Clock, repo);
            var attempt = await exams.StartAttemptAsync(Student, 1, 8, Guid.NewGuid());
            await exams.SaveAttemptAsync(Student, attempt.Id, [new(QuestionBankSeed.Key(201), [QuestionBankSeed.Key(211)])],
                true, OpaqueConcurrencyToken.ToETag(attempt.UpdateDate), Guid.NewGuid());
        }
        return authority;
    }
    private static async Task Map(ScoreExportService service)
    {
        var view = JsonSerializer.SerializeToElement(await service.GetMappingAsync(Teacher, 1));
        var key = view.GetProperty("candidates")[0].GetProperty("candidate_key").GetString()!;
        await service.SetMappingAsync(Teacher, 1, key, 8, "Dedicated SchoolBot score cell", Guid.NewGuid());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task PreviewConfirmApplyAndReplayPersistWithoutDuplicateGradeWrites(bool file)
    {
        using var fixture = new CourseRunFixture(file);
        var authority = await Prepare(fixture);
        var destination = new Destination();
        var service = new ScoreExportService(fixture.Reader(), authority, destination, fixture.Clock);
        await Map(service);
        var preview = await service.PreviewAsync(Teacher, 1, Guid.NewGuid());
        Assert.Equal(4m, preview.Learners.Single().Cells.Single().IntegerTarget);
        var key = Guid.NewGuid();
        var export = await service.ConfirmAsync(Teacher, preview.Id, preview.SnapshotHash, OpaqueConcurrencyToken.ToETag(preview.UpdateDate), key);
        Assert.Equal(export.Id, (await service.ConfirmAsync(Teacher, preview.Id, preview.SnapshotHash, OpaqueConcurrencyToken.ToETag(preview.UpdateDate), key)).Id);
        var reopened = new ScoreExportService(fixture.Reader(), authority, destination, fixture.Clock);
        Assert.Equal(1, await reopened.ProcessAsync(Teacher, export.Id));
        Assert.Equal(0, await reopened.ProcessAsync(Teacher, export.Id));
        Assert.Equal(1, destination.Writes);
        Assert.Equal("applied", (await fixture.Reader().GetScoreExportAsync(export.Id))!.State);
        Assert.NotNull((await fixture.Reader().ListScoreOutboxAsync(export.Id)).Single().ReceiptHash);
        await Assert.ThrowsAsync<DomainRuleException>(() => Map(reopened));
    }

    [Fact]
    public async Task MissingSourceNeverBecomesAnExportedZeroAndGatePreventsOutboxCreation()
    {
        using var fixture = new CourseRunFixture(false);
        var authority = await Prepare(fixture, false);
        var destination = new Destination();
        var service = new ScoreExportService(fixture.Reader(), authority, destination, fixture.Clock);
        await Map(service);
        var preview = await service.PreviewAsync(Teacher, 1, Guid.NewGuid());
        Assert.Equal("ready", preview.Learners.Single().State);
        Assert.Empty(preview.Learners.Single().Cells);
        Assert.Equal("SOURCE_RESULT_MISSING", preview.Learners.Single().SkippedCells!.Single().ReasonCode);
        destination.Verified = false;
        Assert.Equal("WRITER_NOT_VERIFIED", (await Assert.ThrowsAsync<DomainRuleException>(() => service.ConfirmAsync(Teacher,
            preview.Id, preview.SnapshotHash, OpaqueConcurrencyToken.ToETag(preview.UpdateDate), Guid.NewGuid()))).Code);
        Assert.Empty(await fixture.Reader().ListScoreOutboxAsync(null));
        destination.Verified = true;
        var export = await service.ConfirmAsync(Teacher, preview.Id, preview.SnapshotHash, OpaqueConcurrencyToken.ToETag(preview.UpdateDate), Guid.NewGuid());
        Assert.Equal("completed_with_skips", export.State);
        Assert.Equal("skipped", (await fixture.Reader().ListScoreOutboxAsync(export.Id)).Single().State);
        Assert.Equal(0, await service.ProcessAsync(Teacher, export.Id));
        Assert.Equal(0, destination.Writes);
    }

    [Fact]
    public async Task TargetChangeExpiryAndRevokedAssignmentRejectConfirmation()
    {
        using var fixture = new CourseRunFixture(false);
        var authority = await Prepare(fixture);
        var destination = new Destination();
        var service = new ScoreExportService(fixture.Reader(), authority, destination, fixture.Clock);
        await Map(service);
        var preview = await service.PreviewAsync(Teacher, 1, Guid.NewGuid());
        destination.Xmin = "2";
        Assert.Equal("SNAPSHOT_STALE", (await Assert.ThrowsAsync<DomainRuleException>(() => service.ConfirmAsync(Teacher,
            preview.Id, preview.SnapshotHash, OpaqueConcurrencyToken.ToETag(preview.UpdateDate), Guid.NewGuid()))).Code);
        fixture.Clock.Advance(TimeSpan.FromMinutes(15));
        Assert.Equal("PREVIEW_EXPIRED", (await Assert.ThrowsAsync<DomainRuleException>(() => service.ConfirmAsync(Teacher,
            preview.Id, preview.SnapshotHash, OpaqueConcurrencyToken.ToETag(preview.UpdateDate), Guid.NewGuid()))).Code);
        authority.Allowed = false;
        await Assert.ThrowsAsync<DomainRuleException>(() => service.GetPreviewAsync(Teacher, preview.Id));
    }

    [Fact]
    public async Task CrashAfterDestinationCommitRecoversReceiptEvenAfterPreviewExpiry()
    {
        using var fixture = new CourseRunFixture(true);
        var authority = await Prepare(fixture);
        var destination = new Destination { CrashAfterCommit = true };
        var service = new ScoreExportService(fixture.Reader(), authority, destination, fixture.Clock);
        await Map(service);
        var preview = await service.PreviewAsync(Teacher, 1, Guid.NewGuid());
        var export = await service.ConfirmAsync(Teacher, preview.Id, preview.SnapshotHash, OpaqueConcurrencyToken.ToETag(preview.UpdateDate), Guid.NewGuid());
        await Assert.ThrowsAsync<IOException>(() => service.ProcessAsync(Teacher, export.Id));
        fixture.Clock.Advance(TimeSpan.FromMinutes(16));
        Assert.Equal(1, await new ScoreExportService(fixture.Reader(), authority, destination, fixture.Clock).ProcessAsync(Teacher, export.Id));
        Assert.Equal(1, destination.Writes);
    }

    [Theory]
    [InlineData(3, 4, 10, 8)]
    [InlineData(74, 100, 10, 7)]
    public void ConversionUsesDecimalHalfUpOnce(decimal earned, decimal max, decimal target, decimal expected) =>
        Assert.Equal(expected, GradeConversion.Convert(earned, max, target).Integer);

    private sealed class Authority(CourseRunFixture fixture) : IAssessmentAuthority
    {
        internal bool Allowed = true;
        public Task<bool> CanReadRunAsync(AssessmentPrincipalContext p, SchoolBotCourseRunRecord r, CancellationToken ct = default) =>
            p.EmployeeId == 77 ? Task.FromResult(Allowed && r.SchoolId == 11) : fixture.CanReadRunAsync(p, r, ct);
        public Task<bool> CanReviewLearnerAsync(AssessmentPrincipalContext p, SchoolBotCourseRunRecord r, int learnerId, CancellationToken ct = default) =>
            Task.FromResult(Allowed && p.EmployeeId == 77 && learnerId == 301 && r.SchoolId == 11);
        public Task<ClassroomLearningContext> GetClassroomContextAsync(AssessmentPrincipalContext p, SchoolBotCourseRunRecord r, CancellationToken ct = default) =>
            Task.FromResult(new ClassroomLearningContext([301], 102, "term-2", CourseRunSeed.Now.AddMonths(4), "roster-v1"));
    }

    // Test double only: production remains fail-closed until the existing PGGrade writer is verified.
    private sealed class Destination : IGradeDestination
    {
        internal bool Verified = true, CrashAfterCommit;
        internal string Xmin = "1";
        internal int Writes;
        private readonly Dictionary<Guid, (string Hash, GradeApplyReceipt Receipt)> receipts = [];
        public Task<IReadOnlyList<GradeSlotRecord>> DiscoverSlotsAsync(AssessmentPrincipalContext p, SchoolBotCourseRunRecord r, CancellationToken ct) =>
            Task.FromResult<IReadOnlyList<GradeSlotRecord>>([new(0, 11, 1, 501, 601, 701, 801, "existing-id", "test", "verified-fixture", "", null, CourseRunSeed.Now)]);
        public Task RecheckSlotAsync(AssessmentPrincipalContext p, SchoolBotCourseRunRecord r, GradeSlotRecord s, bool empty, CancellationToken ct) => Task.CompletedTask;
        public Task<IReadOnlyList<GradeTargetCell>> ReadCellsAsync(AssessmentPrincipalContext p, SchoolBotCourseRunRecord r, IReadOnlyList<GradeSlotRecord> slots, IReadOnlyList<int> learners, CancellationToken ct) =>
            Task.FromResult<IReadOnlyList<GradeTargetCell>>(slots.SelectMany(s => learners.Select(l => new GradeTargetCell(l, 901, Xmin, "before", s.Id, 10, null, "settings-v1", false))).ToArray());
        public Task RequireVerifiedWriterAsync(CancellationToken ct) => Verified ? Task.CompletedTask : Task.FromException(new DomainRuleException("WRITER_NOT_VERIFIED", "Test gate closed"));
        public Task<GradeApplyReceipt?> FindReceiptAsync(Guid key, string hash, CancellationToken ct)
        {
            if (!receipts.TryGetValue(key, out var receipt)) return Task.FromResult<GradeApplyReceipt?>(null);
            if (receipt.Hash != hash) throw new DomainRuleException(ErrorCodes.IdempotencyConflict, "Receipt hash changed.");
            return Task.FromResult<GradeApplyReceipt?>(receipt.Receipt);
        }
        public Task<GradeApplyReceipt> ApplyAsync(AssessmentPrincipalContext p, Guid key, string hash, IReadOnlyList<ScorePreviewCell> cells, CancellationToken ct)
        {
            if (receipts.TryGetValue(key, out var existing)) return Task.FromResult(existing.Receipt);
            if (cells.Any(c => c.Target.Xmin != Xmin)) throw new DomainRuleException("GRADE_CONFLICT", "Destination changed.");
            Writes++;
            var receipt = new GradeApplyReceipt(Writes, hash);
            receipts[key] = (hash, receipt);
            Xmin = "2";
            if (CrashAfterCommit) { CrashAfterCommit = false; throw new IOException("Simulated process crash after grade commit"); }
            return Task.FromResult(receipt);
        }
    }
}
