using ServiceSchoolBot.Application.Common.Model.Curriculum;
using ServiceSchoolBot.Application.Services.Implementations.Assessment;
using ServiceSchoolBot.Domain;
using ServiceSchoolBot.Domain.Concurrency;
using ServiceSchoolBot.Infrastructure.Repositories.Storage;

namespace ServiceSchoolBot.Tests;

public sealed class CriteriaApprovalTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task IndependentApproverStampsCriteriaAtomicallyAndReplaysAfterRestart(bool file)
    {
        using var fixture = new CourseRunFixture(file);
        await fixture.Initialize();
        var now = fixture.Clock.GetUtcNow();
        await ((ICurriculumBackingStore)fixture.Store()).MutateCurriculumUnitAsync((snapshot, _) => Task.FromResult(new CurriculumStoreMutation<bool>(snapshot with
        {
            Curricula = snapshot.Curricula.Select(c => c with { Lifecycle = "draft", PublishedAt = null }).ToArray(),
            Criteria = snapshot.Criteria.Select(c => c with { ApprovedBy = null, ApprovedAt = null }).ToArray(),
            AuditEvents = [new(1, "admin:author", "Curriculum.Created", "SchoolBotCurriculum", 1, Guid.NewGuid(), "Draft", false, null, now, null, now, null)]
        }, true)));
        var author = CurriculumPrincipalStub.CreateStaffApprover("admin:author", validFrom: now.AddDays(-1));
        var approver = CurriculumPrincipalStub.CreateStaffApprover("admin:reviewer", validFrom: now.AddDays(-1));
        var etag = OpaqueConcurrencyToken.ToETag((await fixture.Reader().GetCurriculumByIdAsync(1))!.UpdateDate);
        Assert.Equal(ErrorCodes.FourEyesRequired, (await Assert.ThrowsAsync<DomainRuleException>(() => new CriteriaApprovalService(fixture.Reader())
            .ApproveAsync(author, 1, "Review-1", etag, Guid.NewGuid()))).Code);
        var key = Guid.NewGuid();
        var saved = await new CriteriaApprovalService(fixture.Reader()).ApproveAsync(approver, 1, "Review-1", etag, key);
        var replay = await new CriteriaApprovalService(fixture.Reader()).ApproveAsync(approver, 1, "Review-1", etag, key);
        Assert.Equal(saved.UpdateDate, replay.UpdateDate);
        var snapshot = await fixture.Snapshot();
        Assert.All(snapshot.Criteria, c => { Assert.Equal("admin:reviewer", c.ApprovedBy); Assert.NotNull(c.ApprovedAt); });
        Assert.Single(snapshot.AuditEvents, a => a.Event == "Criteria.Approved");
    }

    [Fact]
    public async Task AtomicFilePublishRetriesOnlyTemporaryWindowsSharingLocks()
    {
        if (!OperatingSystem.IsWindows()) return;
        var directory = Path.Combine(AppContext.BaseDirectory, "file-sharing-" + Guid.NewGuid().ToString("N"));
        try
        {
            var store = new FileSchoolBotBackingStore(directory);
            await store.MutateCurriculumUnitAsync((snapshot, _) => Task.FromResult(new CurriculumStoreMutation<bool>(snapshot, true)));
            using var heldReader = File.OpenRead(Path.Combine(directory, "curricula_snapshot.json"));
            var pending = store.MutateCurriculumUnitAsync((snapshot, _) => Task.FromResult(new CurriculumStoreMutation<bool>(snapshot, true)));
            await Task.Delay(100);
            heldReader.Dispose();
            Assert.True(await pending);
            Assert.Equal(1, (await store.LoadCurriculumUnitAsync()).Format);
        }
        finally
        {
            var resolved = Path.GetFullPath(directory);
            var root = Path.TrimEndingDirectorySeparator(Path.GetFullPath(AppContext.BaseDirectory)) + Path.DirectorySeparatorChar;
            if (resolved.StartsWith(root, StringComparison.OrdinalIgnoreCase) && Directory.Exists(resolved)) Directory.Delete(resolved, true);
        }
    }
}
