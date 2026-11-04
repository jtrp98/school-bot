using Grpc.Core;
using ServiceSchoolBot.Application.Common.Model.Projects;
using ServiceSchoolBot.Domain;
using ServiceSchoolBot.Domain.Concurrency;
using Rpc = SchoolBot.Infrastructure.Protos;

namespace ServiceSchoolBot.API.GrpcServices;

// A rejection decided before or outside a service call (auth check, missing If-Match) that keeps its HTTP status.
internal sealed class SchoolBotRpcFailure(
    int httpStatus,
    string code,
    string message,
    string? currentEtag = null,
    Rpc.ErrorDetails? details = null) : Exception(message)
{
    public int HttpStatus { get; } = httpStatus;
    public string Code { get; } = code;
    public string? CurrentEtag { get; } = currentEtag;

    public Rpc.ErrorDetails? Details { get; } = details;

    public static SchoolBotRpcFailure From(DomainRuleException error) =>
        new(ErrorStatus.For(error.Code), error.Code, error.Message);
}

internal sealed class SchoolBotRpcCall(Guid requestId, ServerCallContext context)
{
    public Guid RequestId { get; } = requestId;
    public CancellationToken CancellationToken => context.CancellationToken;

    public GatewayIdentity? Identity { get; } = GatewayIdentity.From(context.RequestHeaders);
}

public sealed class SchoolBotGrpcRunner(ITrustedGatewayPeerVerifier peers, SchoolBotGrpcOptions options)
{
    internal async Task<T> RunAsync<T>(ServerCallContext context, Func<SchoolBotRpcCall, Task<T>> body)
    {
        if (!peers.IsTrusted(context))
            throw new RpcException(new Status(StatusCode.Unauthenticated, "Untrusted service peer"));

        var requestId = SchoolBotGrpcMetadata.Validate(context, options);
        try
        {
            return await body(new SchoolBotRpcCall(requestId, context));
        }
        catch (SchoolBotRpcFailure failure)
        {
            throw SchoolBotGrpcMetadata.Error(StatusFor(failure.HttpStatus), failure.Code, failure.Message,
                failure.Message, requestId, currentEtag: failure.CurrentEtag, httpStatus: failure.HttpStatus,
                details: failure.Details);
        }
        catch (VersionConflictException conflict)
        {
            throw Fail(StatusCodes.Status412PreconditionFailed, ErrorCodes.VersionConflict, conflict.Message, requestId,
                OpaqueConcurrencyToken.ToETag(conflict.CurrentUpdateDate));
        }
        catch (DomainRuleException error)
        {
            throw Fail(ErrorStatus.For(error.Code), error.Code, error.Message, requestId);
        }
        catch (RpcException)
        {
            throw;
        }
        catch (OperationCanceledException) when (context.CancellationToken.IsCancellationRequested)
        {
            throw context.Deadline <= DateTime.UtcNow
                ? new RpcException(new Status(StatusCode.DeadlineExceeded, "Deadline exceeded"))
                : new RpcException(new Status(StatusCode.Cancelled, "Call cancelled"));
        }
        catch (Exception)
        {
            throw SchoolBotGrpcMetadata.Error(StatusCode.Internal, "INTERNAL_ERROR", "The request failed.",
                "คำขอล้มเหลว", requestId, httpStatus: StatusCodes.Status500InternalServerError);
        }
    }

    private static RpcException Fail(int httpStatus, string code, string message, Guid requestId, string? currentEtag = null) =>
        SchoolBotGrpcMetadata.Error(StatusFor(httpStatus), code, message, message, requestId,
            currentEtag: currentEtag, httpStatus: httpStatus);

    private static StatusCode StatusFor(int httpStatus) => httpStatus switch
    {
        StatusCodes.Status400BadRequest or StatusCodes.Status415UnsupportedMediaType => StatusCode.InvalidArgument,
        StatusCodes.Status401Unauthorized => StatusCode.Unauthenticated,
        StatusCodes.Status403Forbidden => StatusCode.PermissionDenied,
        StatusCodes.Status404NotFound => StatusCode.NotFound,
        StatusCodes.Status409Conflict => StatusCode.Aborted,
        StatusCodes.Status412PreconditionFailed or StatusCodes.Status428PreconditionRequired => StatusCode.FailedPrecondition,
        StatusCodes.Status413PayloadTooLarge or StatusCodes.Status429TooManyRequests => StatusCode.ResourceExhausted,
        StatusCodes.Status503ServiceUnavailable => StatusCode.Unavailable,
        >= 500 => StatusCode.Internal,
        _ => StatusCode.FailedPrecondition
    };
}
