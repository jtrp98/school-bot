using ServiceSchoolBot.Domain;
using ServiceSchoolBot.Infrastructure.Repositories.Storage;
using static ServiceSchoolBot.Tests.IndependentCurriculumFixture;

namespace ServiceSchoolBot.Tests;

public sealed class IndependentCurriculumReplayMatrixTests
{
    public static IEnumerable<object[]> Cases => from row in IndependentCurriculumContractTests.Operations
        from concurrent in new[] { false, true }
        from changed in new[] { false, true }
        select row.Concat(new object[] { concurrent, changed }).ToArray();

    [Theory]
    [MemberData(nameof(Cases))]
    public async Task SameKeyCommitsOnceAndReplayPrecedesStalePreconditions(
        bool file, string operation, bool concurrent, bool changed)
    {
        var fixture = new IndependentCurriculumFixture(file);
        var mutation = await fixture.Prepare(operation);
        var baseline = await fixture.Snapshot();
        var (firstService, firstGate) = fixture.Gated();
        var (secondService, secondGate) = fixture.Gated();
        firstGate.ObserveOnly = !concurrent;
        secondGate.ObserveOnly = true;
        var key = Guid.NewGuid();
        var first = mutation.Run(firstService, key);
        try
        {
            if (concurrent) await firstGate.Entered.Task.WaitAsync(TimeSpan.FromSeconds(10));
            else await first.WaitAsync(TimeSpan.FromSeconds(10));
            var second = mutation.Run(secondService, key, different: changed);
            await secondGate.CommitEntered.Task.WaitAsync(TimeSpan.FromSeconds(10));
            if (concurrent) Assert.False(second.IsCompleted);
            firstGate.Release.TrySetResult();
            var success = await first.WaitAsync(TimeSpan.FromSeconds(10));
            if (changed)
                Assert.Equal(ErrorCodes.IdempotencyConflict, (await Assert.ThrowsAsync<DomainRuleException>(
                    () => second.WaitAsync(TimeSpan.FromSeconds(10)))).Code);
            else Assert.Equal(success, await second.WaitAsync(TimeSpan.FromSeconds(10)));
            var after = await fixture.Snapshot();
            Assert.Equal(baseline.Curricula.Count + (operation == "create" ? 1 : 0), after.Curricula.Count);
            Assert.Equal(baseline.AuditEvents.Count + 1, after.AuditEvents.Count);
            Assert.Equal((baseline.Idempotency?.Count ?? 0) + 1, after.Idempotency!.Count);
            var entry = Assert.Single(after.Idempotency, item => item.RequestKey == key);
            Assert.Equal(operation == "create" ? 201 : 200, entry.HttpStatus);
            Assert.Equal(success.Id, entry.ResourceId);
            Assert.Null(entry.SchoolId);
            Assert.Equal("staff_admin:" + mutation.Actor.PrincipalId, entry.Principal);
            Assert.Equal(fixture.Clock.Now, entry.CompletedAt);
            Assert.Equal(entry.CompletedAt.AddHours(24), entry.ReplayUntil);
            if (operation == "content")
            {
                Assert.Equal(2, after.Lessons.Count);
                Assert.Equal(6, after.Missions.Count);
                Assert.Equal(6, after.Criteria.Count);
                Assert.Equal(6, after.Activities.Count);
                foreach (var expected in mutation.ContentInput.Lessons)
                {
                    var lesson = Assert.Single(after.Lessons, item => item.LessonKey == expected.LessonKey);
                    Assert.Equal(expected.Objective, lesson.Objective);
                    Assert.Equal(3, after.Missions.Count(item => item.LessonId == lesson.SchoolBotLessonId));
                }
            }
            if (operation == "publish") Assert.Equal("approval:first", after.AuditEvents.Last().Reason);
            Assert.Equal(success, await mutation.Run(fixture.Service(), key));
            Assert.Equal(Json(after), Json(await fixture.Snapshot()));
        }
        finally { firstGate.Release.TrySetResult(); }
    }
}