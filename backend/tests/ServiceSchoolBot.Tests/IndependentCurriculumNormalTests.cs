using ServiceSchoolBot.Application.Common.Model.Curriculum;
using ServiceSchoolBot.Application.Databases.Repository.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using ServiceSchoolBot.Domain.Licensing;
using ServiceSchoolBot.Infrastructure.Repositories.Databases;
using ServiceSchoolBot.Infrastructure.Repositories.Storage;

namespace ServiceSchoolBot.Tests;

public sealed class IndependentCurriculumNormalTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task StaffWritesAndPublishes_SchoolReadsButCannotModifyCentralRevision(bool file)
    {
        var directory = Path.Combine(Path.GetTempPath(), "IndependentCurriculumNormal-" + Guid.NewGuid().ToString("N"));
        ISchoolBotBackingStore store = file ? new FileSchoolBotBackingStore(directory) : new MemorySchoolBotBackingStore();
        try
        {
            using var factory = new ApiFactory(stubPrincipals: false);
            using var host = factory.WithWebHostBuilder(builder =>
                builder.ConfigureServices(services => services.AddSingleton(store)));
            var now = factory.Clock.GetUtcNow();
            await host.Services.GetRequiredService<ILicenseRepository>().CreateLicenseAsync(new SchoolBotLicenseRecord(0,
                "school", new string('a', 64), "hint", now.AddDays(10), null, null, 0, null, false, null, null, null,
                null, 1));
            var school = (SchoolProfileRepository)host.Services.GetRequiredService<ISchoolProfileRepository>();
            school.SeedEmployee(new(13, 1, "employee-13", true, true));
            var client = GrpcTest.Curricula(host);
            var author = GrpcTest.Headers(GatewayCaller.CentralAdmin("source-author"));
            var approver = GrpcTest.Headers(GatewayCaller.CentralAdmin("source-approver"));
            var schoolUser = new GatewayCaller(1, 13, "employee", IsSchoolAdmin: true);

            var metadata = new CreateCurriculumRequest(Guid.NewGuid(), 1, 1,
                "หลักสูตรกลาง", "source:normal", ["ป.1"], []);
            var create = await client.CreateCurriculumAsync(CurriculumRpc.Create(metadata, Guid.NewGuid()), author);
            var id = create.Id;
            var update = await client.UpdateCurriculumMetadataAsync(CurriculumRpc.Metadata(id,
                new CurriculumMetadataInput(1, "หลักสูตรที่แก้แล้ว", "source:normal", ["ป.1"], []), create.Etag,
                Guid.NewGuid()), author);
            var content = await client.UpdateCurriculumContentAsync(CurriculumRpc.Content(id,
                IndependentCurriculumFixture.Content(), update.Etag, Guid.NewGuid()), author);
            var publish = await client.PublishCurriculumAsync(CurriculumRpc.Publish(id, "approval:normal", content.Etag,
                Guid.NewGuid()), approver);
            var baseline = await ((ICurriculumBackingStore)store).LoadCurriculumUnitAsync();
            Assert.Equal("published", Assert.Single(baseline.Curricula).Lifecycle);
            Assert.Equal(4, baseline.AuditEvents.Count(row => row.EntityType == "SchoolBotCurriculum"));
            Assert.Equal(4, baseline.Idempotency!.Count);
            Assert.All(baseline.Idempotency, entry => Assert.Null(entry.SchoolId));
            var read = await client.GetPublishedCurriculumAsync(new() { Id = id }, GrpcTest.Headers(schoolUser));
            Assert.Equal("หลักสูตรที่แก้แล้ว", read.Summary.Title);
            await GrpcTest.AssertError(client.UpdateCurriculumContentAsync(CurriculumRpc.Content(id,
                    IndependentCurriculumFixture.Content(), publish.Etag, Guid.NewGuid()), GrpcTest.Headers(schoolUser)),
                403, "CHANNEL_NOT_ALLOWED");
            var after = await ((ICurriculumBackingStore)store).LoadCurriculumUnitAsync();
            Assert.Equal(IndependentCurriculumFixture.Json(baseline), IndependentCurriculumFixture.Json(after));
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory, true);
        }
    }
}
