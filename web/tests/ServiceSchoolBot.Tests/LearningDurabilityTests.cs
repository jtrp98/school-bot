using System.Text.Json;
using ServiceSchoolBot.Application.Common.Model.Assessment;
using ServiceSchoolBot.Application.Common.Model.Curriculum;
using ServiceSchoolBot.Application.Common.Model.Projects;
using ServiceSchoolBot.Application.Services.Implementations.Assessment;
using ServiceSchoolBot.Application.Services.Implementations.Projects;
using ServiceSchoolBot.Domain;
using ServiceSchoolBot.Domain.Concurrency;
using ServiceSchoolBot.Infrastructure.Repositories.Storage;

namespace ServiceSchoolBot.Tests;

public sealed class LearningDurabilityTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task LostProjectResponseReplaysAcrossServiceRestartWithoutCreatingAnotherRevision(bool file)
    {
        using var fixture = new CourseRunFixture(file);
        await fixture.Initialize();
        var auth = new ProjectAuthContext(true, "gateway-token", false, "desktop", 11, "student", 301);
        var request = new CreateProjectRequest("code", "Homework", JsonSerializer.SerializeToElement(new { kind = "code", source = "hero.jump()" }));
        var key = Guid.NewGuid();
        var first = await new ProjectService(fixture.Reader()).CreateProjectAsync(auth, request, key);
        var restarted = new ProjectService(fixture.Reader());
        var responses = await Task.WhenAll(Enumerable.Range(0, 4).Select(_ => restarted.CreateProjectAsync(auth, request, key)));
        Assert.All(responses, result => Assert.Equal(first.ProjectRevisionId, result.ProjectRevisionId));
        Assert.Single(await fixture.Reader().ListProjectsByOwnerAsync(11, "student", 301));
        Assert.Equal(ErrorCodes.IdempotencyConflict, (await Assert.ThrowsAsync<DomainRuleException>(() => restarted.CreateProjectAsync(auth,
            request with { Name = "Different work" }, key))).Code);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task BackgroundExpiryGradesSavedAnswersWithoutOpeningResults(bool file)
    {
        using var fixture = new CourseRunFixture(file);
        await fixture.Initialize();
        await ((ICurriculumBackingStore)fixture.Store()).MutateCurriculumUnitAsync((s, _) => Task.FromResult(new CurriculumStoreMutation<bool>(s with
        { CourseRuns = s.CourseRuns!.Select(r => r with { CurrentPolicyId = 1 }).ToArray() }, true)));
        var principal = new AssessmentPrincipalContext(CurriculumPrincipalStub.CreateDesktopStudent("301") with { SchoolId = 11 }, null, "student", 301);
        var repo = fixture.Reader();
        var service = new AssessmentReadService(repo, fixture, fixture.Clock, repo);
        var started = await service.StartAttemptAsync(principal, 1, 8, Guid.NewGuid());
        await service.SaveAttemptAsync(principal, started.Id, [new(QuestionBankSeed.Key(201), [QuestionBankSeed.Key(211)])], false,
            OpaqueConcurrencyToken.ToETag(started.UpdateDate), Guid.NewGuid());
        fixture.Clock.Advance(TimeSpan.FromMinutes(2));
        Assert.Equal((1, 0), await new AttemptFinalizer(fixture.Reader()).SweepAsync());
        Assert.Equal((0, 0), await new AttemptFinalizer(fixture.Reader()).SweepAsync());
        var saved = await fixture.Reader().GetAttemptByIdAsync(started.Id);
        Assert.Equal("expired", saved!.State);
        Assert.Equal(2m, saved.EarnedScore);
    }

    [Fact]
    public void SnapshotHashIgnoresJsonPropertyOrderAndDatabaseDecimalScale()
    {
        using var first = JsonDocument.Parse("{\"score\":4.00,\"source\":{\"id\":1,\"max\":10.000000}}");
        using var second = JsonDocument.Parse("{\"source\":{\"max\":10,\"id\":1},\"score\":4}");
        Assert.Equal(CanonicalJson.Hash(first.RootElement), CanonicalJson.Hash(second.RootElement));
    }
}
