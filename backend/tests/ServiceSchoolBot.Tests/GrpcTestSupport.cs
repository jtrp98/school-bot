using System.Collections.Concurrent;
using System.Globalization;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using ServiceSchoolBot.API;
using Rpc = SchoolBot.Infrastructure.Protos;

namespace ServiceSchoolBot.Tests;

// The in-memory test server stands in for the Gateway, the only peer SchoolBot trusts.
public sealed class TrustingGatewayPeerVerifier : ITrustedGatewayPeerVerifier
{
    public bool IsTrusted(ServerCallContext context) => true;
}

// Identity metadata the Gateway forwards after it authenticated the caller.
public sealed record GatewayCaller(
    int SchoolId,
    int UserId,
    string? UserType = null,
    string? AdminId = null,
    bool IsSchoolAdmin = false,
    int? EmployeeId = null,
    string? DeviceEntitlement = null)
{
    public static GatewayCaller CentralAdmin(string adminId = "1") => new(1, 1, AdminId: adminId);

    // A desktop user whose entitlement is what ActivateLicense issues for the school's current binding.
    public static async Task<GatewayCaller> Desktop(WebApplicationFactory<Program> factory,
        ServiceSchoolBot.Application.Databases.Repository.Interfaces.ILicenseRepository licenses, int schoolId, string userType,
        int userId, int? employeeId = null)
    {
        var license = (await licenses.ListLicensesAsync(schoolId)).Single(row => row.CurrentBindingId is not null);
        var binding = (await licenses.GetBindingByIdAsync(license.CurrentBindingId!.Value))!;
        var signer = factory.Services.GetRequiredService<
            ServiceSchoolBot.Application.Services.Interfaces.Licensing.ILicenseTokenSigner>();
        var entitlement = signer.Sign("schoolbot-entitlement+jwt", new Dictionary<string, object>
        {
            ["aud"] = "schoolbot-desktop",
            ["exp"] = license.ExpiresAt.ToUnixTimeSeconds(),
            ["school_id"] = schoolId,
            ["license_id"] = license.SchoolBotLicenseId,
            ["binding_id"] = binding.SchoolBotLicenseBindingId,
            ["binding_epoch"] = license.BindingEpoch,
            ["cnf"] = new Dictionary<string, string> { ["jkt"] = binding.DeviceJkt }
        });
        return new GatewayCaller(schoolId, userId, userType, EmployeeId: employeeId, DeviceEntitlement: entitlement);
    }

    internal void AddTo(Metadata headers)
    {
        headers.Add("x-school-id", SchoolId.ToString(CultureInfo.InvariantCulture));
        headers.Add("x-user-id", UserId.ToString(CultureInfo.InvariantCulture));
        if (UserType is not null) headers.Add("x-user-type", UserType);
        if (AdminId is not null) headers.Add("x-admin-id", AdminId);
        if (IsSchoolAdmin) headers.Add("x-is-admin", "true");
        if (EmployeeId is { } employee) headers.Add("x-employee-id", employee.ToString(CultureInfo.InvariantCulture));
        if (DeviceEntitlement is not null) headers.Add("x-device-entitlement", DeviceEntitlement);
    }
}

// A failed RPC with the SchoolBot error trailer decoded.
public sealed record GrpcError(StatusCode Status, Rpc.SchoolBotError Error)
{
    public int HttpStatus => Error.HttpStatus;
    public string Code => Error.Code;
    public string Message => Error.Message;
    public Rpc.ErrorDetails? Details => Error.Details;
}

// Either the response or the error of one RPC, for tests that compare outcomes of racing calls.
public sealed record GrpcOutcome<T>(T? Response, GrpcError? Error)
{
    public int HttpStatus => Error?.HttpStatus ?? 200;
    public bool Succeeded => Error is null;
}

internal static class GrpcTest
{
    public static IServiceCollection AddTrustedGateway(this IServiceCollection services) =>
        services.AddSingleton<ITrustedGatewayPeerVerifier, TrustingGatewayPeerVerifier>();

    public static GrpcChannel Channel(WebApplicationFactory<Program> factory) =>
        GrpcChannel.ForAddress(factory.Server.BaseAddress, new GrpcChannelOptions
        {
            HttpHandler = factory.Server.CreateHandler(),
            MaxReceiveMessageSize = null,
            MaxSendMessageSize = null
        });

    public static Rpc.ProjectService.ProjectServiceClient Projects(WebApplicationFactory<Program> factory) =>
        new(Channel(factory));

    public static Rpc.LicenseService.LicenseServiceClient Licenses(WebApplicationFactory<Program> factory) =>
        new(Channel(factory));

    public static Rpc.CurriculumService.CurriculumServiceClient Curricula(WebApplicationFactory<Program> factory) =>
        new(Channel(factory));

    public static Rpc.AssessmentService.AssessmentServiceClient Assessments(WebApplicationFactory<Program> factory) =>
        new(Channel(factory));

    public static Rpc.ReleaseService.ReleaseServiceClient Releases(WebApplicationFactory<Program> factory) =>
        new(Channel(factory));

    // Required call metadata plus the caller identity; null means anonymous.
    public static Metadata Headers(GatewayCaller? caller = null)
    {
        var headers = new Metadata
        {
            { "x-caller", "api-gateway" },
            { "x-request-id", Guid.NewGuid().ToString("D") }
        };
        caller?.AddTo(headers);
        return headers;
    }

    public static Metadata As(string? stubToken, params (string Name, string Value)[] testHeaders) =>
        Headers(stubToken is null && testHeaders.Length == 0 ? null : StubPrincipals.Caller(stubToken, testHeaders));

    public static GrpcError Decode(RpcException exception)
    {
        var bytes = exception.Trailers.GetValueBytes("schoolbot-error-bin");
        Assert.True(bytes is not null, $"RPC failed without a SchoolBot error trailer: {exception.Status}");
        return new GrpcError(exception.StatusCode, Rpc.SchoolBotError.Parser.ParseFrom(bytes));
    }

    public static async Task<GrpcError> Fails<T>(AsyncUnaryCall<T> call)
    {
        var exception = await Assert.ThrowsAsync<RpcException>(() => call.ResponseAsync);
        return Decode(exception);
    }

    public static async Task<GrpcError> Fails(Func<Task> call) =>
        Decode(await Assert.ThrowsAsync<RpcException>(call));

    public static async Task<GrpcError> AssertError<T>(AsyncUnaryCall<T> call, int httpStatus, string code)
    {
        var error = await Fails(call);
        AssertError(error, httpStatus, code);
        return error;
    }

    public static void AssertError(GrpcError error, int httpStatus, string code)
    {
        Assert.Equal(httpStatus, error.HttpStatus);
        Assert.Equal(code, error.Code);
        Assert.False(string.IsNullOrWhiteSpace(error.Message));
    }

    public static async Task<GrpcOutcome<T>> Try<T>(AsyncUnaryCall<T> call)
    {
        try
        {
            return new GrpcOutcome<T>(await call.ResponseAsync, null);
        }
        catch (RpcException exception)
        {
            return new GrpcOutcome<T>(default, Decode(exception));
        }
    }
}

// Test principals: the old stub bearer tokens and X-Test-* headers, carried as a Gateway user id that
// the stub resolvers map back to the same stub principal objects.
public sealed record StubCredential(string? Token, IReadOnlyDictionary<string, string> Headers)
{
    public bool Has(string header) => Headers.ContainsKey(header);
    public string? Header(string header) => Headers.TryGetValue(header, out var value) ? value : null;
}

public static class StubPrincipals
{
    public const int SchoolId = 1;
    private static readonly ConcurrentDictionary<string, int> Ids = new(StringComparer.Ordinal);
    private static readonly ConcurrentDictionary<int, StubCredential> Credentials = new();
    private static int next = 900_000;

    public static GatewayCaller Caller(string? token, params (string Name, string Value)[] testHeaders)
    {
        var headers = testHeaders.ToDictionary(header => header.Name, header => header.Value,
            StringComparer.OrdinalIgnoreCase);
        var key = token + "\n" + string.Join("\n", headers.OrderBy(pair => pair.Key, StringComparer.Ordinal)
            .Select(pair => pair.Key + "=" + pair.Value));
        var id = Ids.GetOrAdd(key, _ =>
        {
            var assigned = Interlocked.Increment(ref next);
            Credentials[assigned] = new StubCredential(token, headers);
            return assigned;
        });
        return new GatewayCaller(SchoolId, id);
    }

    public static StubCredential? Find(ServiceSchoolBot.API.GrpcServices.GatewayIdentity? identity) =>
        identity is not null && Credentials.TryGetValue(identity.UserId, out var credential) ? credential : null;
}
