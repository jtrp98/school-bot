using ServiceSchoolBot.Infrastructure.Repositories.Databases;
using ServiceSchoolBot.Application.Common.Model.Curriculum;
using ServiceSchoolBot.Application.Common.Model.Licensing;
using ServiceSchoolBot.Application.Databases.Repository.Interfaces;
using ServiceSchoolBot.Application.Services.Implementations.Curriculum;
using ServiceSchoolBot.Application.Services.Implementations.Licensing;
using ServiceSchoolBot.Application.Services.Interfaces.Curriculum;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using ServiceSchoolBot.API.GrpcServices;
using Grpc.Core;
using Rpc = SchoolBot.Infrastructure.Protos;
using ServiceSchoolBot.Domain.Curriculum;
using ServiceSchoolBot.Domain.Licensing;
using ServiceSchoolBot.Infrastructure.Repositories.Storage;
using static ServiceSchoolBot.Tests.ApiV2LicenseDeviceTests;

namespace ServiceSchoolBot.Tests;

internal sealed class CurriculumImportFixture(bool file) : WebApplicationFactory<Program>, ICurriculumAuthResolver
{
    private readonly string directory = Path.Combine(AppContext.BaseDirectory,
        "CurriculumImportSelfCheck-" + Guid.NewGuid().ToString("N"));
    private readonly MemorySchoolBotBackingStore memory = new();
    private CurriculumService[] services = [];
    private SchoolBotUnitOfWork[] adapters = [];
    private int requests;
    internal SelfCheckClock Clock { get; } = new() { Now = DateTimeOffset.Parse("2026-11-03T00:00:00Z") };
    internal CurriculumPrincipalContext Author => CurriculumPrincipalStub.CreateStaffAuthor("1001",
        validFrom: DateTimeOffset.Parse("2026-01-01T00:00:00Z"));
    internal ISchoolBotBackingStore Store() => file ? new FileSchoolBotBackingStore(directory) : memory;
    internal SchoolBotUnitOfWork Reader() => new(Store(), Clock);
    internal Task<CurriculumStoreSnapshot> Snapshot() => ((ICurriculumBackingStore)Store()).LoadCurriculumUnitAsync();
    internal static Guid Key(int row) => Guid.Parse($"00000000-0000-4000-8000-{row:D12}");
    internal SchoolBotCurriculumRecord Seed => new(1, Key(1), 1, 1, "หลักสูตรทดสอบ", ["P1"], [],
        "fixture:course-plan-096", "draft", null, false, null, Clock.GetUtcNow(), null, Clock.GetUtcNow());

    internal async Task Initialize()
    {
        Directory.CreateDirectory(directory);
        await ((ICurriculumBackingStore)Store()).MutateCurriculumUnitAsync((_, _) => Task.FromResult(
            new CurriculumStoreMutation<bool>(CurriculumStoreSnapshot.Empty with { Curricula = [Seed] }, true)));
        Reopen();
        var now = Clock.GetUtcNow();
        var admin = LicenseAdminPrincipalStub.CreateLicenseAdmin("P1", validFrom: now.AddDays(-1));
        Assert.Contains(admin.Roles!, grant => grant.IsActive(now, "license_admin"));
        await Reader().CreateLicenseAsync(new SchoolBotLicenseRecord(0, "S1", new string('a', 64), "hint",
            now.AddDays(10), null, null, 0, null, false, null, now, null, now, 11));
        using var signer = new TestLicenseTokenSigner();
        using var deviceA = CreateDeviceKey().Key;
        using var deviceB = CreateDeviceKey().Key;
        foreach (var device in new[] { deviceA, deviceB })
        {
            var proof = CreateProof(device, "fixture:challenge", "POST", "/api/service-school-bot/admin/curricula/1/import", null, "{}");
            var parameters = device.ExportParameters(false);
            var jwk = new DeviceJwk("EC", "P-256", TestLicenseTokenSigner.Base64UrlEncode(parameters.Q.X!),
                TestLicenseTokenSigner.Base64UrlEncode(parameters.Q.Y!));
            DeviceProofVerifier.VerifyDeviceProof(proof, jwk, "fixture:challenge", "POST",
                "/api/service-school-bot/admin/curricula/1/import", null, DeviceProofVerifier.ComputeBodyHashWithoutProof("{}"));
        }
        var signed = signer.Sign("fixture", new { school_id = 11, exp = now.AddDays(10).ToUnixTimeSeconds() });
        Assert.True(signer.Verify(signed, out var header, out var payload));
        header!.Dispose();
        payload!.Dispose();
        var rows = BaseRows();
        await WriteRows("base", rows);
        await WriteRows("o", rows.Select((row, index) => row with
        {
            ColumnO = new[] { "999", "-1", "ข้อความ" }[index]
        }).ToArray());
        await WriteRows("reorder", new[] { rows[2], rows[0], rows[1] }
            .Select((row, index) => row with { SourcePosition = index + 3 }).ToArray());
        await WriteRows("invalid", new[]
        {
            rows[0] with { JLink = "not-a-url" },
            rows[1] with { SourceRowKey = Guid.Empty },
            rows[2]
        });
        Clock.Now = now.AddMinutes(1);
    }

    internal void Reopen()
    {
        adapters = [Reader(), Reader()];
        services = adapters.Select(adapter => new CurriculumService(adapter, Clock)).ToArray();
    }

    private Task WriteRows(string variant, ImportFixtureRow[] rows) => File.WriteAllTextAsync(
        Path.Combine(directory, $"course-plan-096-{variant}.json"), JsonSerializer.Serialize(rows));

    internal async Task<CurriculumImportRequest> Normalize(string variant)
    {
        var rows = JsonSerializer.Deserialize<ImportFixtureRow[]>(await File.ReadAllTextAsync(
            Path.Combine(directory, $"course-plan-096-{variant}.json")))!;
        return new("fixture:course-plan-096", rows.Select((raw, index) =>
        {
            var supplied = new CoursePlanSuppliedRow(raw.SourcePosition, raw.SourceRowKey, raw.A, raw.B, raw.C,
                raw.D, raw.E, raw.F, raw.G, LessonPlan: raw.J, SlideSet1: raw.L, ExerciseSet1Link: raw.N,
                ColumnO: raw.ColumnO, TopicSet2: raw.P, DetailsSet2: raw.Q, SlideSet2: raw.T,
                ExerciseSet2Details: raw.W, ExerciseSet2Link: raw.X);
            var transformed = CoursePlanSuppliedRowTransformer.Transform(supplied);
            var media = new[] { raw.JLink, raw.L, raw.N, raw.T, raw.X }.Where(link => link is not null).ToArray();
            var activity = new ActivityInput(Key(raw.Identity + 100), 1, Text(raw.E), "instruction", null, "[]",
                JsonSerializer.Serialize(new { kind = "instruction-v1", instructions = new { th = raw.Q ?? raw.G },
                    media_refs = media }));
            return new CurriculumImportRow(raw.SourceRowKey, raw.SourcePosition, raw.D,
                new LessonInput(transformed.LessonKey, index + 1, Text(raw.E), Text(raw.B), Text(raw.G), [], [activity]));
        }).ToArray());
    }

    private static string Text(string text) => JsonSerializer.Serialize(new { th = text });

    private static ImportFixtureRow[] BaseRows() => Enumerable.Range(3, 3).Select(identity =>
        new ImportFixtureRow(identity, identity, Key(identity), "ป.1", "fixture",
            identity == 3 ? "6" : identity == 4 ? "13" : "2", identity == 3 ? true : identity == 4 ? false : null,
            "บทเรียนทดสอบ", 1, "รายละเอียดทดสอบ",
            identity == 3 ? "Unit 7: How AI Learns" : identity == 4 ? "Unit 6" : "Unit 2",
            identity == 3 ? "https://example.invalid/plan-1" : null,
            identity == 3 ? "https://example.invalid/slide-1" : null,
            identity == 3 ? "https://example.invalid/exercise-1" : null,
            identity == 3 ? "90" : identity == 4 ? "91" : null,
            identity == 3 ? "เรื่องชุดสอง" : null, identity == 3 ? "รายละเอียดชุดสอง" : null,
            identity == 3 ? "https://example.invalid/slide-2" : null,
            identity == 3 ? "แบบฝึกชุดสอง" : null,
            identity == 3 ? "https://example.invalid/exercise-2" : null)).ToArray();

    protected override void ConfigureWebHost(IWebHostBuilder builder) => builder.UseSchoolBotMemoryStorage().ConfigureServices(collection =>
    {
        collection.AddSingleton<TimeProvider>(Clock);
        collection.AddSingleton<ISchoolBotBackingStore>(Store());
        collection.AddSchoolBotRepositories(adapters[0]);
        collection.AddSingleton<ICurriculumAuthResolver>(this);
        collection.AddTrustedGateway();
        collection.AddHttpContextAccessor();
        collection.AddScoped<ICurriculumService>(provider =>
            IsRead(provider.GetRequiredService<IHttpContextAccessor>().HttpContext!.Request.Path)
                ? new CurriculumService(Reader(), Clock) : services[Interlocked.Increment(ref requests) % 2]);
    });

    // Reads (the former GET routes) use a fresh adapter; mutations alternate between the two open adapters.
    private static bool IsRead(PathString path) =>
        path.Value!.Contains("/Get", StringComparison.Ordinal) || path.Value.Contains("/List", StringComparison.Ordinal);

    public Task<CurriculumPrincipalContext> ResolveAsync(GatewayIdentity? identity, CancellationToken ct = default) =>
        Task.FromResult(Author);

    internal static AsyncUnaryCall<Rpc.CurriculumImportResponse> Import(
        Rpc.CurriculumService.CurriculumServiceClient client, CurriculumImportRequest request, Guid key, int id = 1) =>
        client.ImportCurriculumAsync(CurriculumRpc.Import(id, request, key), GrpcTest.Headers());

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (!disposing || !Directory.Exists(directory)) return;
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

    private sealed record ImportFixtureRow(int Identity, int SourcePosition, Guid SourceRowKey,
        string A, string B, string C, bool? D, string E, int F, string G, string J, string? JLink,
        string? L, string? N, string? ColumnO, string? P, string? Q, string? T, string? W, string? X);
}
