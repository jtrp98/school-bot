using System.Security.Cryptography;
using System.Text.Json;
using ServiceSchoolBot.Application.Common.Model.Licensing;
using ServiceSchoolBot.Application.Services.Implementations.Licensing;
using ServiceSchoolBot.Application.Services.Interfaces.Licensing;
using ServiceSchoolBot.Domain;
using ServiceSchoolBot.Domain.Concurrency;
using ServiceSchoolBot.Infrastructure.Repositories.Storage;
using ServiceSchoolBot.Infrastructure.Repositories.Storage.Memory;

var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };
var clock = new FixtureClock();
var memory = new MemorySchoolBotBackingStore();
ISchoolBotBackingStore Store() => args[0] == "memory" ? memory : new FileSchoolBotBackingStore(args[1]);
var adapterA = new SchoolBotUnitOfWork(Store(), clock).License;
var adapterB = new SchoolBotUnitOfWork(Store(), clock).License;
using var signingKey = ECDsa.Create(ECCurve.NamedCurves.nistP256);
using var signer = new TestLicenseTokenSigner(signingKey);
var devices = new[] { new LicenseDeviceService(adapterA, signer, clock), new LicenseDeviceService(adapterB, signer, clock) };
var adminA = new LicenseAdminService(adapterA, clock);
var adminB = new LicenseAdminService(adapterB, clock);
var admin = LicenseAdminPrincipalStub.CreateLicenseAdmin(principal: "P1", validFrom: clock.GetUtcNow().AddDays(-1));
var created = await adminA.CreateLicenseAsync(admin, new CreateLicenseV2Request(100, "S1", clock.GetUtcNow().AddDays(10), 1), Guid.NewGuid());
await adapterA.ListLicensesAsync();
await adapterB.ListLicensesAsync();
var pub = signingKey.ExportParameters(false);
Console.WriteLine(JsonSerializer.Serialize(new { key = created.Key, kid = signer.KeyId, x = Convert.ToBase64String(pub.Q.X!), y = Convert.ToBase64String(pub.Q.Y!) }, options));
string? line;
while ((line = Console.ReadLine()) is not null)
{
    using var input = JsonDocument.Parse(line);
    var root = input.RootElement;
    var path = root.GetProperty("path").GetString();
    object? result = null;
    var status = 200;
    try
    {
        var device = devices[root.GetProperty("actor").GetInt32()];
        var body = root.GetProperty("body").GetRawText();
        switch (path)
        {
            case "/fixture/time":
                clock.Current = DateTimeOffset.FromUnixTimeSeconds(root.GetProperty("body").GetInt64());
                break;
            case "/fixture/snapshot":
                var reader = new SchoolBotUnitOfWork(Store(), clock).License;
                await reader.ListLicensesAsync();
                result = await Store().LoadLicenseUnitAsync();
                break;
            case "/fixture/unbind":
            case "/fixture/unbind-without-role":
            case "/fixture/revoke":
                var license = (await new SchoolBotUnitOfWork(Store(), clock).License.GetLicenseByIdAsync(created.License.Id))!;
                var etag = OpaqueConcurrencyToken.ToETag(license.UpdateDate);
                var principal = path.EndsWith("without-role")
                    ? LicenseAdminPrincipalStub.CreateStaffNoRole(principal: "P2")
                    : admin;
                var nextJkt = root.GetProperty("body").ValueKind == JsonValueKind.String
                    ? root.GetProperty("body").GetString()
                    : null;
                result = path != "/fixture/revoke"
                    ? await adminB.UnbindLicenseAsync(principal, created.License.Id, etag, nextDeviceJkt: nextJkt, reason: "fixture:move")
                    : await adminB.RevokeLicenseAsync(admin, created.License.Id, etag, reason: "fixture:revoke");
                break;
            case "/api/service-school-bot/device-challenges":
                result = await device.CreateChallengeAsync(JsonSerializer.Deserialize<CreateChallengeRequest>(body, options)!);
                break;
            case "/api/service-school-bot/licenses/activate":
                result = await device.ActivateAsync(JsonSerializer.Deserialize<ActivateLicenseRequest>(body, options)!, body, root.GetProperty("request_key").GetString());
                break;
            case "/api/service-school-bot/licenses/validate":
                result = await device.ValidateAsync(JsonSerializer.Deserialize<ValidateLicenseRequest>(body, options)!, body);
                break;
            default:
                throw new InvalidOperationException("Unknown fixture operation");
        }
    }
    catch (DomainRuleException error)
    {
        status = error.Code == "DEVICE_LIMIT" ? 409 : 403;
        result = new { error = new { code = error.Code } };
    }
    Console.WriteLine(JsonSerializer.Serialize(new { status, body = result }, options));
}

sealed class FixtureClock : TimeProvider
{
    public DateTimeOffset Current { get; set; } = DateTimeOffset.Parse("2026-11-03T00:00:00Z");
    public override DateTimeOffset GetUtcNow() => Current;
}
