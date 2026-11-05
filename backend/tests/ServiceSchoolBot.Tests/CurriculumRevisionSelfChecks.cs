using ServiceSchoolBot.Application.Common.Model.Curriculum;
using ServiceSchoolBot.Domain;
using static ServiceSchoolBot.Tests.CurriculumCommitSelfCheckFixture;

namespace ServiceSchoolBot.Tests;

public sealed class CurriculumRevisionSelfChecks
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public async Task CriteriaApprovalsAndParentLinksSurviveDiskReload(int missing)
    {
        var fixture = new CurriculumCommitSelfCheckFixture(true);
        var content = Content();
        if (missing < 4)
        {
            var lessonIndex = missing / 2;
            var lesson = content.Lessons[lessonIndex];
            var mission = lesson.Missions[0];
            var criteria = missing % 2 == 0 ? mission.Criteria with { ApprovedBy = " " }
                : mission.Criteria with { ApprovedAt = null };
            content = content with { Lessons = content.Lessons.Select((item, index) => index != lessonIndex ? item
                : lesson with { Missions = [mission with { Criteria = criteria }, .. lesson.Missions.Skip(1)] }).ToArray() };
        }
        var service = fixture.Service();
        var created = await service.CreateCurriculumAsync(Author, Metadata());
        var edited = await service.UpdateContentAsync(Author, created.Id, content, Tag(created));
        var baseline = await fixture.Snapshot();
        if (missing < 4)
        {
            Assert.Equal(ErrorCodes.CriteriaNotApproved, (await Assert.ThrowsAsync<DomainRuleException>(() =>
                service.PublishCurriculumAsync(Approver, created.Id, new("approval"), Tag(edited),
                    idempotencyKey: Guid.NewGuid()))).Code);
            Assert.Equal(Json(baseline), Json(await fixture.Snapshot()));
            Assert.Empty((await service.ListPublishedCurriculaAsync(CurriculumPrincipalStub.CreateSchoolAdmin())).Items);
        }
        else
        {
            var published = await service.PublishCurriculumAsync(Approver, created.Id, new("approval"), Tag(edited),
                idempotencyKey: Guid.NewGuid());
            Assert.NotNull(published.PublishedAt);
            var after = await fixture.Snapshot();
            Assert.Equal(baseline.AuditEvents.Count + 1, after.AuditEvents.Count);
            Assert.Equal(baseline.Idempotency!.Count + 1, after.Idempotency!.Count);
            var view = await fixture.Service().GetPublishedViewAsync(CurriculumPrincipalStub.CreateSchoolAdmin(), created.Id);
            Assert.Equal(Approver.PrincipalId, view.Approval!.Actor);
            foreach (var lesson in after.Lessons)
            {
                Assert.Equal(created.Id, lesson.CurriculumId);
                var missions = after.Missions.Where(m => m.LessonId == lesson.SchoolBotLessonId).ToArray();
                Assert.Equal(3, missions.Length);
                Assert.All(missions, mission => Assert.Single(after.Criteria, c => c.MissionId == mission.SchoolBotMissionId));
                Assert.Equal(3, after.Activities.Count(activity => activity.LessonId == lesson.SchoolBotLessonId));
            }
        }
    }

    [Fact]
    public async Task StaffMutatesAndSchoolOrUnprivilegedActorsLeaveDiskUnchanged()
    {
        var fixture = new CurriculumCommitSelfCheckFixture(true);
        var service = fixture.Service();
        var created = await service.CreateCurriculumAsync(Author, Metadata(), idempotencyKey: Guid.NewGuid());
        var metadata = await service.UpdateMetadataAsync(Author, created.Id, new(1, "edited", "source", [], []),
            Tag(created), idempotencyKey: Guid.NewGuid());
        var content = await service.UpdateContentAsync(Author, created.Id, Content(), Tag(metadata), idempotencyKey: Guid.NewGuid());
        await service.PublishCurriculumAsync(Approver, created.Id, new("approval"), Tag(content), idempotencyKey: Guid.NewGuid());
        var baseline = await fixture.Snapshot();
        Assert.Equal(4, baseline.AuditEvents.Count);
        Assert.Equal(4, baseline.Idempotency!.Count);
        foreach (var actor in new[] { Author with { Roles = [] },
            CurriculumPrincipalStub.CreateSchoolAdmin(101, Author.PrincipalId!), CurriculumPrincipalStub.CreateSchoolTeacher(102) })
        {
            await Assert.ThrowsAsync<DomainRuleException>(() => service.CreateCurriculumAsync(actor, Metadata()));
            await Assert.ThrowsAsync<DomainRuleException>(() => service.UpdateMetadataAsync(actor, created.Id,
                new(1, "edited", "source", [], []), Tag(content)));
            await Assert.ThrowsAsync<DomainRuleException>(() => service.UpdateContentAsync(actor, created.Id, Content(), Tag(content)));
            await Assert.ThrowsAsync<DomainRuleException>(() => service.PublishCurriculumAsync(actor, created.Id, new("approval"), Tag(content)));
            Assert.Equal(Json(baseline), Json(await fixture.Snapshot()));
        }
    }

    [Theory]
    [InlineData("publish")]
    [InlineData("revision")]
    [InlineData("content-first")]
    [InlineData("publish-first")]
    public async Task CompetingInstancesReloadFreshStateBeforeCommitting(string race)
    {
        var fixture = new CurriculumCommitSelfCheckFixture(true);
        var mutation = await fixture.Prepare("publish");
        var (firstService, gate) = fixture.Gated();
        var secondService = fixture.Service();
        gate.PauseBefore = true;
        var first = race == "publish-first"
            ? firstService.UpdateContentAsync(Author, mutation.Created.Id, Content(), Tag(mutation.Created), idempotencyKey: Guid.NewGuid())
            : mutation.Run(firstService, Guid.NewGuid());
        try
        {
            await gate.Entered.Task.WaitAsync(TimeSpan.FromSeconds(10));
            fixture.Clock.Now = fixture.Clock.Now.AddTicks(1);
            if (race == "revision")
                await secondService.CreateCurriculumAsync(Editor, mutation.Input with { Revision = 2 }, idempotencyKey: Guid.NewGuid());
            else if (race == "content-first")
                await secondService.UpdateContentAsync(Author, mutation.Created.Id, Content(), Tag(mutation.Created), idempotencyKey: Guid.NewGuid());
            else
                await mutation.Run(secondService, Guid.NewGuid(), SecondApprover, different: true);
            gate.Release.TrySetResult();
            if (race == "revision") await first;
            else Assert.Contains((await Assert.ThrowsAsync<DomainRuleException>(async () => await first)).Code,
                new[] { ErrorCodes.VersionConflict, ErrorCodes.StateConflict });
            var snapshot = await fixture.Snapshot();
            Assert.Equal(race == "revision" ? 2 : 1, snapshot.Curricula.Count);
            Assert.Equal(race == "revision" ? 4 : 3, snapshot.AuditEvents.Count);
            Assert.Equal(race == "content-first" ? "draft" : "published", snapshot.Curricula[0].Lifecycle);
        }
        finally { gate.Release.TrySetResult(); }
    }

    [Fact]
    public async Task ReorderingAndNewRevisionPreservePublishedRevisionAndRejectedWrites()
    {
        var fixture = new CurriculumCommitSelfCheckFixture(true);
        var service = fixture.Service();
        var created = await service.CreateCurriculumAsync(Author, Metadata());
        var content = Content();
        var edited = await service.UpdateContentAsync(Author, created.Id, content, Tag(created));
        fixture.Clock.Now = fixture.Clock.Now.AddTicks(1);
        var reordered = content with { Lessons = [content.Lessons[1] with { Ordinal = 1, Objective = "new" },
            content.Lessons[0] with { Ordinal = 2 }] };
        var ordered = await service.UpdateContentAsync(Author, created.Id, reordered, Tag(edited));
        var view = await fixture.Service().GetDraftViewAsync(Author, created.Id);
        Assert.Equal(content.Lessons[1].LessonKey, view.Lessons[0].LessonKey);
        Assert.Equal("new", view.Lessons[0].Objective);
        var baseline = Json(await fixture.Snapshot());
        await Assert.ThrowsAsync<DomainRuleException>(() => service.UpdateContentAsync(Author, created.Id, content, null));
        await Assert.ThrowsAsync<DomainRuleException>(() => service.UpdateContentAsync(Author, created.Id, content, Tag(created)));
        foreach (var approval in new[] { "", " ", new string('x', 1001) })
            await Assert.ThrowsAsync<DomainRuleException>(() => service.PublishCurriculumAsync(Approver, created.Id, new(approval), Tag(ordered)));
        var self = Author with { Roles = [.. Author.Roles!, .. Approver.Roles!] };
        Assert.Equal(ErrorCodes.FourEyesRequired, (await Assert.ThrowsAsync<DomainRuleException>(() =>
            service.PublishCurriculumAsync(self, created.Id, new("approval"), Tag(ordered)))).Code);
        Assert.Equal(baseline, Json(await fixture.Snapshot()));
        var published = await service.PublishCurriculumAsync(Approver, created.Id, new(new string('x', 1000)), Tag(ordered));
        var oldView = await fixture.Service().GetPublishedViewAsync(CurriculumPrincipalStub.CreateSchoolAdmin(), created.Id);
        var old = await fixture.Snapshot();
        var revision2 = await service.CreateCurriculumAsync(Editor,
            Metadata() with { CurriculumKey = oldView.CurriculumKey, Revision = 2 });
        var revision2Content = await service.UpdateContentAsync(Editor, revision2.Id, reordered, Tag(revision2));
        await service.PublishCurriculumAsync(SecondApprover, revision2.Id, new("approval:revision2"), Tag(revision2Content));
        await Assert.ThrowsAsync<DomainRuleException>(() => service.UpdateContentAsync(Author, created.Id, content, Tag(published)));
        var latest = await fixture.Snapshot();
        Assert.Equal(System.Text.Json.JsonSerializer.Serialize(old.Curricula[0]),
            System.Text.Json.JsonSerializer.Serialize(latest.Curricula[0]));
        var oldAfter = await fixture.Service().GetPublishedViewAsync(CurriculumPrincipalStub.CreateSchoolAdmin(), created.Id);
        Assert.Equal(System.Text.Json.JsonSerializer.Serialize(oldView), System.Text.Json.JsonSerializer.Serialize(oldAfter));
    }
}
