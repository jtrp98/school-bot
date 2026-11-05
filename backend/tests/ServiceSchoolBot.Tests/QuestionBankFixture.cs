using ServiceSchoolBot.Infrastructure.Repositories.Databases;
using ServiceSchoolBot.Application.Common.Model.Assessment;
using ServiceSchoolBot.Application.Common.Model.Curriculum;
using ServiceSchoolBot.Application.Common.Model.Licensing;
using ServiceSchoolBot.Application.Databases.Repository.Interfaces;
using ServiceSchoolBot.Application.Services.Implementations.Assessment;
using ServiceSchoolBot.Application.Services.Implementations.Licensing;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using ServiceSchoolBot.API.GrpcServices;
using Grpc.Core;
using ServiceSchoolBot.Domain.Assessment;
using ServiceSchoolBot.Domain.Licensing;
using ServiceSchoolBot.Infrastructure.Repositories.Storage;
using static ServiceSchoolBot.Tests.ApiV2LicenseDeviceTests;

namespace ServiceSchoolBot.Tests;

internal sealed class QuestionBankFixture : WebApplicationFactory<Program>, IAssessmentAuthResolver, IAssessmentAuthority
{
    private readonly string? directory;
    private readonly ISchoolBotBackingStore storeA;
    private readonly ISchoolBotBackingStore storeB;
    private int bankRequests;
    private int readRequests;
    private QuestionBankService[] banks = [];
    private AssessmentReadService[] readers = [];
    public FakeClock Clock { get; } = new(DateTimeOffset.Parse("2026-11-03T00:00:00Z"));
    public SchoolBotUnitOfWork AdapterA { get; private set; } = null!;
    public SchoolBotUnitOfWork AdapterB { get; private set; } = null!;
    public CurriculumStoreSnapshot Seed { get; } = QuestionBankSeed.Build();
    public string SeedHash => Convert.ToHexStringLower(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(Seed)));

    public QuestionBankFixture(bool file)
    {
        directory = file ? Path.Combine(Path.GetTempPath(), "QuestionBankSelfCheck-" + Guid.NewGuid().ToString("N")) : null;
        storeA = directory is null ? new MemorySchoolBotBackingStore() : new FileSchoolBotBackingStore(directory);
        storeB = directory is null ? storeA : new FileSchoolBotBackingStore(directory);
    }

    public async Task Initialize()
    {
        await ((ICurriculumBackingStore)storeA).MutateCurriculumUnitAsync((_, _) => Task.FromResult(
            new CurriculumStoreMutation<bool>(Seed, true)));
        AdapterA = new(storeA, Clock);
        AdapterB = new(storeB, Clock);
        banks = [new(AdapterA, Clock), new(AdapterB, Clock)];
        readers = [new(AdapterA, this, Clock, AdapterA), new(AdapterB, this, Clock, AdapterB)];
        var t = Clock.GetUtcNow();
        var admin = LicenseAdminPrincipalStub.CreateLicenseAdmin("P1", validFrom: t.AddDays(-1));
        Assert.Equal("staff_admin", admin.Channel);
        Assert.Contains(admin.Roles!, grant => grant.IsActive(t, "license_admin"));
        await AdapterA.CreateLicenseAsync(new SchoolBotLicenseRecord(0, "S1", new string('a', 64), "hint",
            t.AddDays(10), null, null, 0, null, false, null, t, null, t, 11));
        using var signer = new TestLicenseTokenSigner();
        using var deviceA = CreateDeviceKey().Key;
        using var deviceB = CreateDeviceKey().Key;
        foreach (var device in new[] { deviceA, deviceB })
        {
            var proof = CreateProof(device, "fixture:challenge", "GET", "/api/service-school-bot/attempts", null, "{}");
            var publicKey = device.ExportParameters(false);
            var jwk = new DeviceJwk("EC", "P-256", TestLicenseTokenSigner.Base64UrlEncode(publicKey.Q.X!),
                TestLicenseTokenSigner.Base64UrlEncode(publicKey.Q.Y!));
            DeviceProofVerifier.VerifyDeviceProof(proof, jwk, "fixture:challenge", "GET", "/api/service-school-bot/attempts", null,
                DeviceProofVerifier.ComputeBodyHashWithoutProof("{}"));
        }
        var signed = signer.Sign("fixture", new { school_id = 11, exp = t.AddDays(10).ToUnixTimeSeconds() });
        Assert.True(signer.Verify(signed, out var header, out var payload));
        header!.Dispose();
        payload!.Dispose();
        Clock.Advance(TimeSpan.FromHours(2));
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder) => builder.UseSchoolBotMemoryStorage().ConfigureServices(services =>
    {
        services.AddSingleton<TimeProvider>(Clock);
        services.AddSingleton<ISchoolBotBackingStore>(storeA);
        services.AddSchoolBotRepositories(AdapterA);
        services.AddSingleton<IAssessmentAuthResolver>(this);
        services.AddTrustedGateway();
        services.AddSingleton<IAssessmentAuthority>(this);
        services.AddScoped(_ => banks[Interlocked.Increment(ref bankRequests) % 2]);
        services.AddScoped(_ => readers[Interlocked.Increment(ref readRequests) % 2]);
    });

    public static AssessmentPrincipalContext Principal(string actor)
    {
        var from = DateTimeOffset.Parse("2026-01-01T00:00:00Z");
        return actor switch
        {
            "author" => new(CurriculumPrincipalStub.CreateStaffAuthor("1001", validFrom: from), 101),
            "approver" => new(CurriculumPrincipalStub.CreateStaffApprover("1003", validFrom: from), 103),
            "teacher" => new(CurriculumPrincipalStub.CreateSchoolTeacher(11, "201"), 201, "employee"),
            "student" => new(CurriculumPrincipalStub.CreateDesktopStudent("301") with { SchoolId = 11 },
                null, "student", 301),
            _ => new(new CurriculumPrincipalContext(true, null, null, false, null))
        };
    }

    public Task<AssessmentPrincipalContext> ResolveAsync(GatewayIdentity? identity, CancellationToken ct = default) =>
        Task.FromResult(Principal(StubPrincipals.Find(identity)?.Token ?? ""));

    // The caller named by actor ("author", "approver", "teacher", "student"; anything else is anonymous).
    public static Metadata As(string actor) => GrpcTest.As(actor);

    public Task<bool> CanReadRunAsync(AssessmentPrincipalContext principal, SchoolBotCourseRunRecord run,
        CancellationToken ct = default) => Task.FromResult(run.SchoolId == 11 && run.ClassroomId == 401 &&
            run.NYear == 2569 && run.NTerm == "1" &&
            ((principal.Session.Channel == "desktop" && principal.ProfileKind == "student" && principal.LearnerId == 301) ||
                (principal.Session.Channel == "school_web" && principal.EmployeeId == 201)));

    public void ReopenServices()
    {
        AdapterA = Reader();
        AdapterB = Reader();
        banks = [new(AdapterA, Clock), new(AdapterB, Clock)];
        readers = [new(AdapterA, this, Clock, AdapterA), new(AdapterB, this, Clock, AdapterB)];
    }

    public SchoolBotUnitOfWork Reader() => new(directory is null ? storeA : new FileSchoolBotBackingStore(directory), Clock);
    public QuestionBankService ReopenedBanks() => new(Reader(), Clock);
    public AssessmentReadService ReopenedReader()
    {
        var reader = Reader();
        return new(reader, this, Clock, reader);
    }
    public Task<CurriculumStoreSnapshot> Snapshot() =>
        ((ICurriculumBackingStore)(directory is null ? storeA : new FileSchoolBotBackingStore(directory)))
            .LoadCurriculumUnitAsync();

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (!disposing || directory is null || !Directory.Exists(directory)) return;
        var resolved = Path.GetFullPath(directory);
        var root = Path.TrimEndingDirectorySeparator(Path.GetFullPath(Path.GetTempPath())) + Path.DirectorySeparatorChar;
        if (!resolved.StartsWith(root, StringComparison.OrdinalIgnoreCase) ||
            (File.GetAttributes(resolved) & FileAttributes.ReparsePoint) != 0 ||
            Directory.EnumerateFileSystemEntries(resolved, "*", SearchOption.AllDirectories)
                .Any(path => (File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0))
            throw new InvalidOperationException("Unsafe test directory.");
        Directory.Delete(resolved, true);
    }
}
