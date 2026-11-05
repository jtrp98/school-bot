using System.Text;
using Google.Protobuf;
using Grpc.Core;
using SchoolBot.Infrastructure.Protos;

namespace ServiceSchoolBot.API.GrpcServices;

// Metadata rules and error trailers shared by every SchoolBot RPC so each service stays a thin mapper.
internal static class SchoolBotGrpcMetadata
{
    private const int MaxMetadataBytes = 16 * 1024;

    private static readonly HashSet<string> AllowedMetadataKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        ":authority",
        "authority",
        ":method",
        ":path",
        ":scheme",
        "content-type",
        "te",
        "user-agent",
        "grpc-accept-encoding",
        "grpc-encoding",
        "grpc-timeout",
        "grpc-trace-bin",
        "traceparent",
        "x-request-id",
        "x-caller",
        "origin",
        "x-lang",
        "x-school-id",
        "x-user-id",
        "x-user-type",
        "x-admin-id",
        "x-is-admin",
        "x-employee-id",
        "x-device-entitlement"
    };

    public static Guid Validate(ServerCallContext context, SchoolBotGrpcOptions options)
    {
        Microsoft.AspNetCore.Http.HttpContext? httpContext = null;
        try { httpContext = context.GetHttpContext(); }
        catch (InvalidOperationException) { }

        Guid? rawCandidateRequestId = null;
        var rawMetadataValid = true;

        if (httpContext is not null && httpContext.Request.Headers.Count > 0)
        {
            var rawHeaders = httpContext.Request.Headers;
            var rawRequestIdCount = 0;

            foreach (var header in rawHeaders)
            {
                var count = header.Value.Count;
                if (count > 1) rawMetadataValid = false;

                if (header.Key.Equals("x-request-id", StringComparison.OrdinalIgnoreCase))
                {
                    rawRequestIdCount += count;
                    rawCandidateRequestId = (count == 1 && header.Value[0] is { Length: 36 } text &&
                        Guid.TryParseExact(text, "D", out var parsed) && parsed != Guid.Empty) ? parsed : null;
                }
                else if (header.Key.Equals("grpc-trace-bin", StringComparison.OrdinalIgnoreCase))
                {
                    if (count != 1 || string.IsNullOrEmpty(header.Value[0]))
                    {
                        rawMetadataValid = false;
                    }
                    else
                    {
                        try { _ = Convert.FromBase64String(header.Value[0]!); }
                        catch (FormatException) { rawMetadataValid = false; }
                    }
                }
                else if (header.Key.Equals("origin", StringComparison.OrdinalIgnoreCase))
                {
                    if (count != 1 || (header.Value[0] is { } originVal && originVal.Contains(',')))
                    {
                        rawMetadataValid = false;
                    }
                }
            }

            if (rawRequestIdCount != 1)
            {
                rawCandidateRequestId = null;
                rawMetadataValid = false;
            }
        }

        Metadata headers;
        try
        {
            headers = context.RequestHeaders;
        }
        catch (Exception)
        {
            var fallbackRequestId = rawCandidateRequestId ?? Guid.NewGuid();
            throw Error(StatusCode.Internal, "INTERNAL_ERROR",
                "The request failed.", "คำขอล้มเหลว", fallbackRequestId);
        }

        Guid? candidateRequestId = rawCandidateRequestId;
        var requestIdCount = 0;
        long totalMetadataBytes = 0;

        foreach (var entry in headers)
        {
            totalMetadataBytes += (long)entry.Key.Length +
                (entry.IsBinary ? entry.ValueBytes.Length : Encoding.UTF8.GetByteCount(entry.Value)) + 32;

            if (entry.Key.Equals("x-request-id", StringComparison.OrdinalIgnoreCase))
            {
                requestIdCount++;
                if (requestIdCount == 1 && entry.Value is { Length: 36 } text &&
                    Guid.TryParseExact(text, "D", out var parsed) && parsed != Guid.Empty)
                {
                    if (httpContext is null || httpContext.Request.Headers.Count == 0)
                    {
                        candidateRequestId = parsed;
                    }
                }
                else
                {
                    candidateRequestId = null;
                }
            }
        }

        var requestId = candidateRequestId ?? Guid.NewGuid();

        if (totalMetadataBytes > MaxMetadataBytes)
        {
            throw Error(StatusCode.Internal, "INTERNAL_ERROR",
                "The request failed.", "คำขอล้มเหลว", requestId);
        }

        var seenKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        string? caller = null;
        string? origin = null;
        var metadataValid = rawMetadataValid;

        foreach (var entry in headers)
        {
            var key = entry.Key;
            var canonicalKey = (key.Equals(":authority", StringComparison.OrdinalIgnoreCase) ||
                                key.Equals("authority", StringComparison.OrdinalIgnoreCase))
                ? ":authority"
                : key;

            if (!seenKeys.Add(canonicalKey))
            {
                metadataValid = false;
                break;
            }

            // Reject credentials, proxy headers and identity keys the Gateway never sends.
            if (key.Equals("cookie", StringComparison.OrdinalIgnoreCase) ||
                key.Equals("authorization", StringComparison.OrdinalIgnoreCase) ||
                key.Equals("x-forwarded-for", StringComparison.OrdinalIgnoreCase) ||
                key.StartsWith("jabjai-", StringComparison.OrdinalIgnoreCase) ||
                key.Equals("x-tenant-id", StringComparison.OrdinalIgnoreCase) ||
                key.Equals("x-account-id", StringComparison.OrdinalIgnoreCase) ||
                key.Equals("x-profile-id", StringComparison.OrdinalIgnoreCase) ||
                key.Equals("x-principal", StringComparison.OrdinalIgnoreCase) ||
                key.Equals("x-role", StringComparison.OrdinalIgnoreCase) ||
                key.Equals("x-identity", StringComparison.OrdinalIgnoreCase))
            {
                metadataValid = false;
                break;
            }

            if (!AllowedMetadataKeys.Contains(key))
            {
                metadataValid = false;
                break;
            }

            if (key.Equals("x-request-id", StringComparison.OrdinalIgnoreCase))
            {
                if (entry.Value is not { Length: 36 } text ||
                    !Guid.TryParseExact(text, "D", out var parsed) || parsed == Guid.Empty)
                {
                    metadataValid = false;
                    break;
                }
            }
            else if (key.Equals("x-caller", StringComparison.OrdinalIgnoreCase))
            {
                caller = entry.Value;
            }
            else if (key.Equals("origin", StringComparison.OrdinalIgnoreCase))
            {
                origin = entry.Value;
                if (origin.Contains(','))
                {
                    metadataValid = false;
                    break;
                }
            }
            else if (key.Equals("x-lang", StringComparison.OrdinalIgnoreCase))
            {
                if (entry.Value is not ("th" or "en"))
                {
                    metadataValid = false;
                    break;
                }
            }
            else if (key.Equals("traceparent", StringComparison.OrdinalIgnoreCase))
            {
                if (!IsValidTraceparent(entry.Value))
                {
                    metadataValid = false;
                    break;
                }
            }
        }

        if (!metadataValid ||
            string.IsNullOrEmpty(caller) || !string.Equals(caller, "api-gateway", StringComparison.Ordinal) ||
            !seenKeys.Contains("x-request-id"))
        {
            throw Error(StatusCode.Internal, "INTERNAL_ERROR",
                "The request failed.", "คำขอล้มเหลว", requestId);
        }

        if (origin is not null)
        {
            if (options.AllowedOrigins.Count == 0 || !options.AllowedOrigins.Contains(origin))
            {
                throw Error(StatusCode.PermissionDenied, "ORIGIN_NOT_ALLOWED",
                    "Origin is not allowed.", "ไม่อนุญาตที่มาของคำขอ", requestId);
            }
        }

        return requestId;
    }

    private static bool IsValidTraceparent(string? value)
    {
        if (string.IsNullOrEmpty(value) || value.Length < 55) return false;
        if (value[2] != '-' || value[35] != '-' || value[52] != '-') return false;

        var v0 = value[0];
        var v1 = value[1];
        if (!IsLowerHex(v0) || !IsLowerHex(v1)) return false;
        if (v0 == 'f' && v1 == 'f') return false;

        if (v0 == '0' && v1 == '0')
        {
            if (value.Length != 55) return false;
        }
        else
        {
            if (value.Length > 55 && value[55] != '-') return false;
        }

        var allZeroTrace = true;
        for (var i = 3; i < 35; i++)
        {
            var c = value[i];
            if (!IsLowerHex(c)) return false;
            if (c != '0') allZeroTrace = false;
        }
        if (allZeroTrace) return false;

        var allZeroParent = true;
        for (var i = 36; i < 52; i++)
        {
            var c = value[i];
            if (!IsLowerHex(c)) return false;
            if (c != '0') allZeroParent = false;
        }
        if (allZeroParent) return false;

        return IsLowerHex(value[53]) && IsLowerHex(value[54]);

        static bool IsLowerHex(char c) =>
            c is (>= '0' and <= '9') or (>= 'a' and <= 'f');
    }


    public static RpcException Error(
        StatusCode statusCode,
        string code,
        string message,
        string messageTh,
        Guid requestId,
        int? retryAfterSeconds = null,
        string? field = null,
        string? currentEtag = null,
        int? resourceId = null,
        int? httpStatus = null,
        ErrorDetails? details = null)
    {
        if (code.Length > 64) code = code[..64];
        if (message.Length > 512) message = message[..512];
        if (messageTh.Length > 512) message = messageTh[..512];
        if (field is { Length: > 128 }) field = field[..128];
        if (currentEtag is { Length: > 64 }) currentEtag = currentEtag[..64];
        if (retryAfterSeconds.HasValue && retryAfterSeconds.Value < 0) retryAfterSeconds = 0;
        if (resourceId.HasValue && resourceId.Value <= 0) resourceId = null;

        var errorDetail = new ErrorDetail
        {
            Code = code,
            Message = message,
            MessageTh = messageTh
        };
        if (!string.IsNullOrEmpty(field))
        {
            errorDetail.FieldViolations.Add(new FieldViolation
            {
                Field = field,
                Description = message,
                DescriptionTh = messageTh
            });
        }

        var schoolBotError = new SchoolBotError
        {
            Code = code,
            Message = message,
            RequestId = requestId.ToString("D")
        };
        if (httpStatus.HasValue) schoolBotError.HttpStatus = httpStatus.Value;
        if (details is not null)
        {
            schoolBotError.Details = details;
        }
        else if (field != null || currentEtag != null || retryAfterSeconds.HasValue || resourceId.HasValue)
        {
            var built = new ErrorDetails();
            if (field != null) built.Field = field;
            if (currentEtag != null) built.CurrentEtag = currentEtag;
            if (retryAfterSeconds.HasValue) built.RetryAfterSeconds = retryAfterSeconds.Value;
            if (resourceId.HasValue) built.ResourceId = resourceId.Value;
            schoolBotError.Details = built;
        }

        var detailBytes = errorDetail.ToByteArray();
        var sbErrorBytes = schoolBotError.ToByteArray();

        var trailers = new Metadata
        {
            { "sb-error-bin", detailBytes },
            { "schoolbot-error-bin", sbErrorBytes }
        };

        return new RpcException(new Status(statusCode, code), trailers);
    }
}
