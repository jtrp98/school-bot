using ServiceSchoolBot.Infrastructure.Repositories.Databases;
using ServiceSchoolBot.Application.Databases.Repository.Interfaces;
using ServiceSchoolBot.Application.Services.Implementations.Curriculum;
using Google.Protobuf;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using ServiceSchoolBot.Domain;
using ServiceSchoolBot.Infrastructure.Repositories.Storage;
using static ServiceSchoolBot.Tests.IndependentCurriculumFixture;
using Rpc = SchoolBot.Infrastructure.Protos;

namespace ServiceSchoolBot.Tests;

public sealed class IndependentCurriculumRevisionBoundaryTests
{
    [Fact]
    public async Task NonStringApprovalWireIsRejectedBeforeChangingFileSnapshot()
    {
        var directory = Path.Combine(Path.GetTempPath(), "IndependentCurriculumBoundary-" + Guid.NewGuid().ToString("N"));
        var store = new FileSchoolBotBackingStore(directory);
        var adapter = new SchoolBotUnitOfWork(store);
        var service = new CurriculumService(adapter, TimeProvider.System);
        var created = await service.CreateCurriculumAsync(Author, Metadata());
        var content = await service.UpdateContentAsync(Author, created.Id, Content(), Tag(created));
        var before = await File.ReadAllBytesAsync(Path.Combine(directory, "curricula_snapshot.json"));
        using var root = new ApiFactory();
        using var host = root.WithWebHostBuilder(builder => builder.ConfigureServices(services =>
            services.AddSchoolBotRepositories(new SchoolBotUnitOfWork(new FileSchoolBotBackingStore(directory)))));
        var client = GrpcTest.Curricula(host);
        var validFields = CurriculumRpc.Publish(created.Id, null, Tag(content), Guid.NewGuid());
        // Field 5 is a string; a numeric wire value must never become an approval reference.
        var wire = validFields.ToByteArray().Concat(new byte[] { 0x28, 123 }).ToArray();
        var request = Rpc.PublishCurriculumRequest.Parser.ParseFrom(wire);
        Assert.False(request.HasApprovalRef);
        await GrpcTest.AssertError(client.PublishCurriculumAsync(request,
            GrpcTest.As("token-staff-approver-valid")), 400, ErrorCodes.InvalidInput);
        Assert.Equal(before, await File.ReadAllBytesAsync(Path.Combine(directory, "curricula_snapshot.json")));
    }

    [Fact]
    public async Task MissingContributorAuditCannotBeReplacedByGuessingApprover()
    {
        var fixture = new IndependentCurriculumFixture(true);
        var mutation = await fixture.Prepare("publish");
        await ((ICurriculumBackingStore)fixture.Store()).MutateCurriculumUnitAsync((snapshot, _) =>
            Task.FromResult(new CurriculumStoreMutation<bool>(snapshot with { AuditEvents = [] }, true)));
        var before = Json(await fixture.Snapshot());
        var error = await Assert.ThrowsAsync<DomainRuleException>(() => mutation.Run(fixture.Service(), Guid.NewGuid()));
        Assert.Equal(ErrorCodes.FourEyesRequired, error.Code);
        Assert.Equal(before, Json(await fixture.Snapshot()));
    }

    [Fact]
    public async Task PublishedMetadataAndContentRemainImmutableWithFreshETag()
    {
        var fixture = new IndependentCurriculumFixture(true);
        var mutation = await fixture.Prepare("publish");
        var published = await mutation.Run(fixture.Service(), Guid.NewGuid());
        var before = Json(await fixture.Snapshot());
        var service = fixture.Service();
        var metadata = await Assert.ThrowsAsync<DomainRuleException>(() => service.UpdateMetadataAsync(
            Editor, published.Id, new(1, "changed", "source", [], []), Tag(published), idempotencyKey: Guid.NewGuid()));
        Assert.Equal(ErrorCodes.StateConflict, metadata.Code);
        var content = await Assert.ThrowsAsync<DomainRuleException>(() => service.UpdateContentAsync(
            Editor, published.Id, Content(), Tag(published), idempotencyKey: Guid.NewGuid()));
        Assert.Equal(ErrorCodes.StateConflict, content.Code);
        Assert.Equal(before, Json(await fixture.Snapshot()));
    }
}
