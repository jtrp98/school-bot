using ServiceSchoolBot.Infrastructure.Repositories.Databases;
using ServiceSchoolBot.Application.Common.Model.Assessment;
using ServiceSchoolBot.Application.Common.Model.Licensing;
using ServiceSchoolBot.Application.Databases.Repository.Interfaces;
using ServiceSchoolBot.Application.Services.Implementations.Assessment;
using ServiceSchoolBot.Application.Services.Implementations.Curriculum;
using ServiceSchoolBot.Application.Services.Implementations.Licensing;
using ServiceSchoolBot.Application.Services.Interfaces.Curriculum;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using ServiceSchoolBot.Domain.Assessment;
using ServiceSchoolBot.Domain.Licensing;
using ServiceSchoolBot.Infrastructure.Repositories.Storage;
using static ServiceSchoolBot.Tests.ApiV2LicenseDeviceTests;

namespace ServiceSchoolBot.Tests;

internal sealed class CourseRunFixture(bool file) : WebApplicationFactory<Program>, IAssessmentAuthority
{
    private readonly string directory = Path.Combine(
        AppContext.BaseDirectory, "CourseRunSelfCheck-" + Guid.NewGuid().ToString("N"));
    private readonly MemorySchoolBotBackingStore memory = new();
    private CurriculumService[] curricula = [];
    private AssessmentReadService[] progress = [];
    private int curriculumRequests;
    private int progressRequests;
    internal FakeClock Clock { get; } = new(CourseRunSeed.Now);
    internal SchoolProfileRepository School { get; } = new();
    internal SchoolBotUnitOfWork[] Adapters { get; private set; } = [];
    internal int ClassroomGrade => 4;
    internal ISchoolBotBackingStore Store() => file ? new FileSchoolBotBackingStore(directory) : memory;
    internal SchoolBotUnitOfWork Reader() => new(Store(), Clock);
    internal Task<CurriculumStoreSnapshot> Snapshot() => ((ICurriculumBackingStore)Store()).LoadCurriculumUnitAsync();

    internal async Task Initialize()
    {
        using var signer = new TestLicenseTokenSigner();
        using var deviceA = CreateDeviceKey().Key;
        using var deviceB = CreateDeviceKey().Key;
        Assert.NotEqual(deviceA.ExportSubjectPublicKeyInfo(), deviceB.ExportSubjectPublicKeyInfo());
        foreach (var key in new[] { deviceA, deviceB })
        {
            var proof = CreateProof(key, "fixture:challenge", "GET", "/api/service-school-bot/progress", null, "{}");
            var publicKey = key.ExportParameters(false);
            var jwk = new DeviceJwk("EC", "P-256", TestLicenseTokenSigner.Base64UrlEncode(publicKey.Q.X!),
                TestLicenseTokenSigner.Base64UrlEncode(publicKey.Q.Y!));
            DeviceProofVerifier.VerifyDeviceProof(proof, jwk, "fixture:challenge", "GET", "/api/service-school-bot/progress", null,
                DeviceProofVerifier.ComputeBodyHashWithoutProof("{}"));
        }
        var signed = signer.Sign("fixture", new
        {
            school_id = 11,
            exp = Clock.GetUtcNow().AddDays(10).ToUnixTimeSeconds()
        });
        Assert.True(signer.Verify(signed, out var header, out var payload));
        header!.Dispose();
        payload!.Dispose();
        var admin = LicenseAdminPrincipalStub.CreateLicenseAdmin("P1", validFrom: Clock.GetUtcNow().AddDays(-1));
        Assert.Equal("staff_admin", admin.Channel);
        Assert.Contains(admin.Roles!, grant => grant.IsActive(Clock.GetUtcNow(), "license_admin"));
        var setup = Reader();
        var license = await setup.CreateLicenseAsync(new SchoolBotLicenseRecord(0, "S1", new string('a', 64), "hint",
            Clock.GetUtcNow().AddDays(10), null, null, 0, null, false, null, null, null, null, 11));
        var keyParameters = deviceA.ExportParameters(false);
        var deviceJwk = new DeviceJwk("EC", "P-256", TestLicenseTokenSigner.Base64UrlEncode(keyParameters.Q.X!),
            TestLicenseTokenSigner.Base64UrlEncode(keyParameters.Q.Y!));
        var jkt = DeviceProofVerifier.ComputeDeviceJkt(deviceJwk);
        await setup.CommitLicenseActivationAsync(new(license.SchoolBotLicenseId, null, 0, jkt,
            JsonSerializer.Serialize(deviceJwk), Clock.GetUtcNow().AddMinutes(15)));
        var seed = CourseRunSeed.Build(jkt);
        await ((ICurriculumBackingStore)Store()).MutateCurriculumUnitAsync((_, _) =>
            Task.FromResult(new CurriculumStoreMutation<bool>(seed, true)));
        School.SeedStudent(new(301, 11, "student", true));
        Adapters = [Reader(), Reader()];
        curricula = Adapters.Select(adapter => new CurriculumService(adapter, Clock)).ToArray();
        progress = Adapters.Select(adapter => new AssessmentReadService(adapter, this, Clock, adapter)).ToArray();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder) => builder.UseSchoolBotMemoryStorage().ConfigureServices(services =>
    {
        services.AddSingleton<TimeProvider>(Clock);
        services.AddSingleton<ISchoolBotBackingStore>(Store());
        services.AddSchoolBotRepositories(Adapters[0]);
        services.AddSingleton<ISchoolProfileRepository>(School);
        services.AddSingleton<IAssessmentAuthority>(this);
        services.AddTrustedGateway();
        services.AddScoped<ICurriculumService>(_ => curricula[Interlocked.Increment(ref curriculumRequests) % 2]);
        services.AddScoped(_ => progress[Interlocked.Increment(ref progressRequests) % 2]);
    });

    // The seeded student 301 on the desktop bound to school 11's license.
    internal Task<GatewayCaller> Student() => GatewayCaller.Desktop(this, Reader(), 11, "student", 301);

    public Task<bool> CanReadRunAsync(AssessmentPrincipalContext principal, SchoolBotCourseRunRecord run,
        CancellationToken ct = default) => Task.FromResult(
            principal.ProfileKind == "student" && principal.LearnerId == 301 && principal.Session.SchoolId == 11 &&
            run.SchoolId == 11 && run.ClassroomId == 401 && run.NYear == 101 && run.NTerm == "term-1" &&
            run.PlanCourseIds.SequenceEqual([501]));

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (!disposing || !file || !Directory.Exists(directory)) return;
        var resolved = Path.GetFullPath(directory);
        var root = Path.TrimEndingDirectorySeparator(Path.GetFullPath(AppContext.BaseDirectory))
            + Path.DirectorySeparatorChar;
        if (!resolved.StartsWith(root, StringComparison.OrdinalIgnoreCase) ||
            (File.GetAttributes(resolved) & FileAttributes.ReparsePoint) != 0 ||
            Directory.EnumerateFileSystemEntries(resolved, "*", SearchOption.AllDirectories)
                .Any(path => (File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0))
            throw new InvalidOperationException("Unsafe test directory.");
        Directory.Delete(resolved, true);
    }
}
