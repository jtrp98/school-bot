using System.Globalization;
using System.Text.Json;
using Grpc.Core;
using ServiceSchoolBot.Application.Common.Model.Licensing;
using ServiceSchoolBot.Application.Services.Implementations.Licensing;
using ServiceSchoolBot.Application.Services.Interfaces.Licensing;
using ServiceSchoolBot.Domain;
using ServiceSchoolBot.Domain.Concurrency;
using Rpc = SchoolBot.Infrastructure.Protos;

namespace ServiceSchoolBot.API.GrpcServices;

public sealed class LicenseGrpcService(
    SchoolBotGrpcRunner runner,
    ILicenseDeviceService devices,
    ILicenseAdminService admin,
    ILicenseAdminAuthResolver authResolver,
    TimeProvider clock) : Rpc.LicenseService.LicenseServiceBase
{
    // Device bodies keep their own lenient binding (no web defaults), as the HTTP endpoints did.
    private static readonly JsonSerializerOptions DeviceJson = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    public override Task<Rpc.DeviceChallengeResponse> CreateDeviceChallenge(
        Rpc.DeviceBodyRequest request, ServerCallContext context) =>
        runner.RunAsync(context, async call =>
        {
            var body = ReadDeviceBody<CreateChallengeRequest>(request.RawBody);
            if (body.DeviceJwk is null)
                throw new SchoolBotRpcFailure(StatusCodes.Status400BadRequest, ErrorCodes.InvalidInput,
                    "Device JWK is required.");
            var challenge = await devices.CreateChallengeAsync(body, call.CancellationToken);
            return new Rpc.DeviceChallengeResponse
            {
                Challenge = challenge.Challenge,
                ServerTime = Time(challenge.ServerTime),
                ExpiresAt = Time(challenge.ExpiresAt)
            };
        });

    public override Task<Rpc.EntitlementResponse> ActivateLicense(
        Rpc.DeviceBodyRequest request, ServerCallContext context) =>
        runner.RunAsync(context, async call =>
        {
            var body = ReadDeviceBody<ActivateLicenseRequest>(request.RawBody, "Request body is required.");
            var key = request.HasIdempotencyKey && !string.IsNullOrWhiteSpace(request.IdempotencyKey)
                ? request.IdempotencyKey
                : null;
            return ToProto(await devices.ActivateAsync(body, request.RawBody, key, call.CancellationToken));
        });

    public override Task<Rpc.EntitlementResponse> ValidateLicense(
        Rpc.DeviceBodyRequest request, ServerCallContext context) =>
        runner.RunAsync(context, async call =>
        {
            var body = ReadDeviceBody<ValidateLicenseRequest>(request.RawBody, "Request body is required.");
            return ToProto(await devices.ValidateAsync(body, request.RawBody, call.CancellationToken));
        });

    public override Task<Rpc.ListLicensesResponse> ListLicenses(
        Rpc.ListLicensesRequest request, ServerCallContext context) =>
        runner.RunAsync(context, async call =>
        {
            var auth = Authorize(call);
            var list = await admin.ListLicensesAsync(auth, request.HasSchoolId ? request.SchoolId : null,
                call.CancellationToken);
            var response = new Rpc.ListLicensesResponse();
            response.Licenses.AddRange(list.Select(ToProto));
            return response;
        });

    public override Task<Rpc.CreatedLicenseResponse> CreateLicense(
        Rpc.CreateLicenseRequest request, ServerCallContext context) =>
        runner.RunAsync(context, async call =>
        {
            var auth = Authorize(call);
            Guid? requestKey = request.HasIdempotencyKey && !string.IsNullOrWhiteSpace(request.IdempotencyKey) &&
                Guid.TryParse(request.IdempotencyKey, out var parsed) ? parsed : null;
            var created = await admin.CreateLicenseAsync(auth,
                new CreateLicenseV2Request(request.SchoolId, request.SchoolName, ParseTime(request.ExpiresAt),
                    request.HasDeviceLimit ? request.DeviceLimit : null),
                requestKey, call.CancellationToken);
            return new Rpc.CreatedLicenseResponse { License = ToProto(created.License), Key = created.Key };
        });

    public override Task<Rpc.LicenseViewResponse> RevokeLicense(
        Rpc.RevokeLicenseRequest request, ServerCallContext context) =>
        runner.RunAsync(context, async call =>
        {
            var auth = Authorize(call);
            var view = await admin.RevokeLicenseAsync(auth, request.Id, IfMatch(request.HasIfMatch ? request.IfMatch : null),
                request.HasReason ? request.Reason : null, call.CancellationToken);
            return ToViewResponse(view);
        });

    public override Task<Rpc.LicenseViewResponse> UnbindLicense(
        Rpc.UnbindLicenseRequest request, ServerCallContext context) =>
        runner.RunAsync(context, async call =>
        {
            var auth = Authorize(call);
            var view = await admin.UnbindLicenseAsync(auth, request.Id, IfMatch(request.HasIfMatch ? request.IfMatch : null),
                request.HasNextDeviceJkt ? request.NextDeviceJkt : null, request.HasReason ? request.Reason : null,
                call.CancellationToken);
            return ToViewResponse(view);
        });

    private LicenseAdminAuthContext Authorize(SchoolBotRpcCall call)
    {
        var auth = authResolver.Resolve(call.Identity);
        var decision = LicenseAdminDecisionService.EvaluateAuthorization(auth, clock.GetUtcNow());
        if (!decision.IsAllowed)
            throw new SchoolBotRpcFailure(decision.StatusCode, decision.ErrorCode!, decision.Message!);
        return auth;
    }

    private static T ReadDeviceBody<T>(string rawBody, string missingMessage = "Device JWK is required.") where T : class
    {
        T? body;
        try
        {
            body = JsonSerializer.Deserialize<T>(rawBody, DeviceJson);
        }
        catch (JsonException)
        {
            throw new SchoolBotRpcFailure(StatusCodes.Status400BadRequest, ErrorCodes.InvalidInput,
                "Malformed JSON request.");
        }

        return body ?? throw new SchoolBotRpcFailure(StatusCodes.Status400BadRequest, ErrorCodes.InvalidInput,
            missingMessage);
    }

    private static string? IfMatch(string? value) => string.IsNullOrWhiteSpace(value) ? null : value;

    private static string Time(DateTimeOffset value) => value.ToString("O", CultureInfo.InvariantCulture);

    private static DateTimeOffset ParseTime(string value) =>
        DateTimeOffset.ParseExact(value, "O", CultureInfo.InvariantCulture);

    private static Rpc.EntitlementResponse ToProto(EntitlementResult result) => new()
    {
        Valid = result.Valid,
        LicenseId = result.LicenseId,
        BindingId = result.BindingId,
        Entitlement = result.Entitlement,
        CheckedAt = Time(result.CheckedAt),
        ExpiresAt = Time(result.ExpiresAt)
    };

    private static Rpc.LicenseItem ToProto(LicenseViewV2 view)
    {
        var result = new Rpc.LicenseItem
        {
            Id = view.Id,
            SchoolId = view.SchoolId,
            SchoolName = view.SchoolName,
            KeyHint = view.KeyHint,
            ExpiresAt = Time(view.ExpiresAt),
            Revoked = view.Revoked,
            ActivatedDevices = view.ActivatedDevices
        };
        if (view.UpdateDate is { } updated) result.UpdateDate = Time(updated);
        return result;
    }

    private static Rpc.LicenseViewResponse ToViewResponse(LicenseViewV2 view)
    {
        var response = new Rpc.LicenseViewResponse { License = ToProto(view) };
        if (view.UpdateDate.HasValue) response.Etag = OpaqueConcurrencyToken.ToETag(view.UpdateDate);
        return response;
    }
}
