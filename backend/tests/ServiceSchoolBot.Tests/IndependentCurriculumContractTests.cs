using ServiceSchoolBot.Infrastructure.Repositories.Databases;
using ServiceSchoolBot.Application.Common.Model.Curriculum;
using ServiceSchoolBot.Application.Databases.Repository.Interfaces;
using ServiceSchoolBot.Application.Services.Implementations.Curriculum;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ServiceSchoolBot.Domain;
using ServiceSchoolBot.Infrastructure.Repositories.Storage;
using static ServiceSchoolBot.Tests.IndependentCurriculumFixture;

namespace ServiceSchoolBot.Tests;

public sealed class IndependentCurriculumContractTests
{
    public static IEnumerable<object[]> Operations => from file in new[] { false, true }
        from operation in new[] { "create", "metadata", "content", "publish" }
        select new object[] { file, operation };

    [Theory]
    [MemberData(nameof(Operations))]
    public async Task EncodingFailureRollsBackEveryPartAndSameKeyCanRecover(bool file, string operation)
    {
        var fixture = new IndependentCurriculumFixture(file);
        var mutation = await fixture.Prepare(operation);
        var baseline = await fixture.Snapshot();
        var before = Json(baseline);
        var proxy = DispatchProxy.Create<ISchoolBotRepositoryScope, IndependentCurriculumFaultProxy>();
        var fault = (IndependentCurriculumFaultProxy)(object)proxy;
        fault.Target = fixture.Adapter();
        fault.FailEncoding = true;
        var key = Guid.NewGuid();
        var error = await Record.ExceptionAsync(() => mutation.Run(new CurriculumService(proxy, fixture.Clock), key));
        Assert.NotNull(error);
        Assert.Contains("Independent result encoding failure", error.ToString());
        Assert.True(fault.MutationFinished);
        Assert.Equal(before, Json(await fixture.Snapshot()));
        var success = await mutation.Run(fixture.Service(), key);
        Assert.Equal(success, await mutation.Run(fixture.Service(), key));
        var after = await fixture.Snapshot();
        Assert.Single(after.Idempotency!, entry => entry.RequestKey == key);
        Assert.Equal(baseline.AuditEvents.Count + 1, after.AuditEvents.Count);
        Assert.Equal(Event(operation), after.AuditEvents.Last().Event);
    }

    [Theory]
    [MemberData(nameof(Operations))]
    public async Task CompleteCandidateDoesNotLeakWhenStoreFailsBeforeSwap(bool file, string operation)
    {
        var fixture = new IndependentCurriculumFixture(file);
        var mutation = await fixture.Prepare(operation);
        var baseline = await fixture.Snapshot();
        var before = Json(baseline);
        var proxy = DispatchProxy.Create<ISchoolBotBackingStore, IndependentCurriculumStoreFaultProxy>();
        var fault = (IndependentCurriculumStoreFaultProxy)(object)proxy;
        fault.Target = fixture.Store();
        fault.FailBeforeSwap = true;
        var service = new CurriculumService(new SchoolBotUnitOfWork(proxy, fixture.Clock), fixture.Clock);
        var key = Guid.NewGuid();
        await Assert.ThrowsAsync<IOException>(() => mutation.Run(service, key));
        Assert.True(fault.CandidateComplete);
        Assert.Equal(before, Json(await fixture.Snapshot()));
        fault.FailBeforeSwap = false;
        var success = await mutation.Run(service, key);
        Assert.Equal(success, await mutation.Run(fixture.Service(), key));
        Assert.Single((await fixture.Snapshot()).Idempotency!, entry => entry.RequestKey == key);
    }

    [Theory]
    [MemberData(nameof(Operations))]
    public async Task LostResponsePreservesHashCipherCommitTimeAndOriginalResult(bool file, string operation)
    {
        var fixture = new IndependentCurriculumFixture(file);
        var mutation = await fixture.Prepare(operation);
        var before = await fixture.Snapshot();
        var proxy = DispatchProxy.Create<ISchoolBotRepositoryScope, IndependentCurriculumFaultProxy>();
        var fault = (IndependentCurriculumFaultProxy)(object)proxy;
        fault.Target = fixture.Adapter();
        fault.LoseResponse = true;
        var key = Guid.NewGuid();
        await Assert.ThrowsAsync<IOException>(() => mutation.Run(new CurriculumService(proxy, fixture.Clock), key));
        var committed = await fixture.Snapshot();
        var entry = Assert.Single(committed.Idempotency!, item => item.RequestKey == key);
        var hashInput = operation switch
        {
            "create" => JsonSerializer.Serialize(mutation.Input),
            "metadata" => mutation.Created.Id + "|" + JsonSerializer.Serialize(
                new CurriculumMetadataInput(1, "updated", "source:updated", ["P1"], [])),
            "content" => mutation.Created.Id + "|" + JsonSerializer.Serialize(mutation.ContentInput),
            "publish" => mutation.Created.Id + "|approval:first",
            _ => throw new ArgumentOutOfRangeException(nameof(operation))
        };
        Assert.Equal(Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(hashInput))), entry.RequestHash);
        Assert.Null(entry.SchoolId);
        Assert.Equal("staff_admin:" + mutation.Actor.PrincipalId, entry.Principal);
        Assert.Equal(entry.CompletedAt.AddHours(24), entry.ReplayUntil);
        Assert.NotEmpty(entry.ResultCipher!);
        Assert.NotEmpty(entry.ResultKeyId!);
        fixture.Clock.Now = fixture.Clock.Now.AddMinutes(10);
        var recovered = await mutation.Run(fixture.Service(), key);
        Assert.Equal(entry.ResourceId, recovered.Id);
        Assert.Equal(before.Curricula.Count + (operation == "create" ? 1 : 0), committed.Curricula.Count);
        Assert.Equal(before.AuditEvents.Count + 1, committed.AuditEvents.Count);
        Assert.Equal((before.Idempotency?.Count ?? 0) + 1, committed.Idempotency!.Count);
        Assert.Equal(Json(committed), Json(await fixture.Snapshot()));
        Assert.Equal(recovered, await mutation.Run(fixture.Service(), key));
        Assert.Equal(Json(committed), Json(await fixture.Snapshot()));
    }

    [Theory]
    [MemberData(nameof(Operations))]
    public async Task EveryRejectedReplayUsesFreshFixtureAndLeavesAllStoredStateUnchanged(bool file, string operation)
    {
        for (var actorCase = 0; actorCase < 5; actorCase++)
        {
            var fixture = new IndependentCurriculumFixture(file);
            var mutation = await fixture.Prepare(operation);
            var key = Guid.NewGuid();
            await mutation.Run(fixture.Service(), key);
            var baseline = await fixture.Snapshot();
        var before = Json(baseline);
            var actor = actorCase switch
            {
                0 => CurriculumPrincipalStub.CreateStaffAuthor("without-role") with { Roles = [] },
                1 => CurriculumPrincipalStub.CreateSchoolAdmin(101, Author.PrincipalId!),
                2 => CurriculumPrincipalStub.CreateSchoolTeacher(102),
                3 => mutation.Actor with { Roles = [] },
                4 => mutation.Actor with { IsSessionExpired = true },
                _ => throw new ArgumentOutOfRangeException()
            };
            var error = await Assert.ThrowsAsync<DomainRuleException>(() => mutation.Run(fixture.Service(), key, actor));
            Assert.Equal(actorCase switch
            {
                1 or 2 => ErrorCodes.ChannelNotAllowed,
                4 => ErrorCodes.SessionExpired,
                _ => ErrorCodes.StaffRoleRequired
            }, error.Code);
            Assert.Equal(before, Json(await fixture.Snapshot()));
        }
    }

    private static string Event(string operation) => operation switch
    {
        "create" => "Curriculum.Created",
        "metadata" => "Curriculum.MetadataUpdated",
        "content" => "Curriculum.ContentUpdated",
        "publish" => "Curriculum.Published",
        _ => throw new ArgumentOutOfRangeException(nameof(operation))
    };
}