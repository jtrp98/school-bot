using System.Text.Json;
using static ServiceSchoolBot.Infrastructure.Authentication.GatewayFieldValidation;

namespace ServiceSchoolBot.Infrastructure.Authentication;

public static class GatewayOutcomeDecoder
{
    public static GatewayOutcome Decode(
        GatewaySourceKind sourceKind,
        GatewayOperation operation,
        GatewayStage stage,
        int httpStatus,
        ReadOnlySpan<byte> utf8Json)
    {
        if (!Enum.IsDefined(sourceKind) || !Enum.IsDefined(operation) || !Enum.IsDefined(stage) ||
            httpStatus is < 100 or > 599 || sourceKind != GatewaySourceKind.Web || !MatchesStage(operation, stage))
            return Reject(sourceKind, 400, "INVALID_INPUT");
        if (utf8Json.Length > 2_097_152) return Unavailable(sourceKind);
        if (GatewayJsonValidation.IsWhitespace(utf8Json))
            return httpStatus == 200 ? Unavailable(sourceKind) : RejectHttpStatus(sourceKind, httpStatus);
        if (!GatewayJsonValidation.TryParse(utf8Json, out var document)) return Unavailable(sourceKind);

        using (document)
        {
            var root = document.RootElement;
            if (!HasMatchingCode(root, httpStatus)) return Unavailable(sourceKind);
            var hasResults = root.TryGetProperty("results", out var results);
            if (hasResults && results.ValueKind is not (JsonValueKind.Object or JsonValueKind.Null))
                return Unavailable(sourceKind);
            if (httpStatus != 200)
            {
                if (results.ValueKind == JsonValueKind.Object &&
                    HasAny(results, "token", "changeToken", "challengeToken", "next", "requirePasswordChange"))
                    return Unavailable(sourceKind);
                return RejectHttpStatus(sourceKind, httpStatus);
            }
            if (!hasResults || results.ValueKind != JsonValueKind.Object) return Unavailable(sourceKind);
            return DecodeSuccess(sourceKind, operation, stage, results);
        }
    }

    private static GatewayOutcome DecodeSuccess(
        GatewaySourceKind sourceKind,
        GatewayOperation operation,
        GatewayStage stage,
        JsonElement results)
    {
        if (HasAny(results, "temporaryToken", "isDeviceVerified", "forceResetPassword")) return Unavailable(sourceKind);
        if (results.TryGetProperty("requirePasswordChange", out var passwordChange) &&
            passwordChange.ValueKind is not (JsonValueKind.True or JsonValueKind.False))
            return Unavailable(sourceKind);

        if (passwordChange.ValueKind == JsonValueKind.True)
        {
            if (!Allows(operation, GatewayOutcomeKind.PasswordChange) ||
                HasAny(results, "token", "next", "challengeToken", "expiresAt", "trustedDeviceToken",
                    "ref", "masked", "resendAfterSeconds") || !TrySecret(results, "changeToken", out var changeToken))
                return Unavailable(sourceKind);
            return new GatewayOutcome(GatewayOutcomeKind.PasswordChange, sourceKind,
                nextStage: GatewayStage.PasswordChange, changeToken: new GatewaySecret(changeToken));
        }

        if (results.TryGetProperty("next", out var next))
            return DecodeChallenge(sourceKind, operation, results, next);
        if (results.TryGetProperty("token", out _))
            return DecodeTerminal(sourceKind, operation, results, passwordChange);
        return DecodeSent(sourceKind, operation, stage, results);
    }

    private static GatewayOutcome DecodeChallenge(
        GatewaySourceKind sourceKind,
        GatewayOperation operation,
        JsonElement results,
        JsonElement next)
    {
        if (next.ValueKind != JsonValueKind.String) return Unavailable(sourceKind);
        var kind = next.GetString() switch
        {
            "two-factor" => GatewayOutcomeKind.TwoFactor,
            "contact" => GatewayOutcomeKind.Contact,
            _ => GatewayOutcomeKind.Rejected
        };
        if (!Allows(operation, kind) ||
            HasAny(results, "token", "changeToken", "trustedDeviceToken", "ref", "masked", "resendAfterSeconds") ||
            !TrySecret(results, "challengeToken", out var challengeToken, challenge: true) ||
            !TryExpiresAt(results, out var expiresAt))
            return Unavailable(sourceKind);
        var nextStage = kind == GatewayOutcomeKind.TwoFactor ? GatewayStage.TwoFactor : GatewayStage.Contact;
        return new GatewayOutcome(kind, sourceKind, nextStage: nextStage,
            challengeToken: new GatewaySecret(challengeToken), expiresAt: expiresAt);
    }

    private static GatewayOutcome DecodeTerminal(
        GatewaySourceKind sourceKind,
        GatewayOperation operation,
        JsonElement results,
        JsonElement passwordChange)
    {
        if (!Allows(operation, GatewayOutcomeKind.TerminalCandidate) ||
            passwordChange.ValueKind != JsonValueKind.False ||
            HasAny(results, "next", "changeToken", "challengeToken", "ref", "masked", "resendAfterSeconds") ||
            !TrySecret(results, "token", out var token) || !TryExpiresAt(results, out var expiresAt))
            return Unavailable(sourceKind);
        string? trustedDeviceToken = null;
        if (results.TryGetProperty("trustedDeviceToken", out var trustedDevice) &&
            trustedDevice.ValueKind != JsonValueKind.Null)
        {
            if (!TrySecret(results, "trustedDeviceToken", out var trustedToken)) return Unavailable(sourceKind);
            trustedDeviceToken = trustedToken;
        }
        return new GatewayOutcome(GatewayOutcomeKind.TerminalCandidate, sourceKind,
            token: new GatewaySecret(token), expiresAt: expiresAt,
            trustedDeviceToken: trustedDeviceToken is null ? null : new GatewaySecret(trustedDeviceToken));
    }

    private static GatewayOutcome DecodeSent(
        GatewaySourceKind sourceKind,
        GatewayOperation operation,
        GatewayStage stage,
        JsonElement results)
    {
        if (!Allows(operation, GatewayOutcomeKind.Sent) ||
            HasAny(results, "token", "changeToken", "challengeToken", "next",
                "requirePasswordChange", "trustedDeviceToken") ||
            !TryPresentation(results, "ref", out var reference) ||
            !TryPresentation(results, "masked", out var masked) ||
            !TryExpiresAt(results, out var expiresAt) || !TryResendAfterSeconds(results, out var resendAfterSeconds))
            return Unavailable(sourceKind);
        return new GatewayOutcome(GatewayOutcomeKind.Sent, sourceKind, nextStage: stage,
            expiresAt: expiresAt, reference: reference, masked: masked, resendAfterSeconds: resendAfterSeconds);
    }

    private static bool MatchesStage(GatewayOperation operation, GatewayStage stage) => operation switch
    {
        GatewayOperation.Login => stage == GatewayStage.Initial,
        GatewayOperation.FirstPassword => stage == GatewayStage.PasswordChange,
        GatewayOperation.TwoFactorSend or GatewayOperation.TwoFactorVerify => stage == GatewayStage.TwoFactor,
        GatewayOperation.ContactSend or GatewayOperation.ContactVerify => stage == GatewayStage.Contact,
        _ => false
    };

    private static bool Allows(GatewayOperation operation, GatewayOutcomeKind kind) => operation switch
    {
        GatewayOperation.Login => kind is GatewayOutcomeKind.PasswordChange or GatewayOutcomeKind.TwoFactor or
            GatewayOutcomeKind.Contact or GatewayOutcomeKind.TerminalCandidate,
        GatewayOperation.FirstPassword => kind is GatewayOutcomeKind.TwoFactor or GatewayOutcomeKind.Contact or
            GatewayOutcomeKind.TerminalCandidate,
        GatewayOperation.TwoFactorSend or GatewayOperation.ContactSend => kind == GatewayOutcomeKind.Sent,
        GatewayOperation.TwoFactorVerify or GatewayOperation.ContactVerify => kind is GatewayOutcomeKind.Contact or
            GatewayOutcomeKind.TerminalCandidate,
        _ => false
    };

    private static GatewayOutcome RejectHttpStatus(GatewaySourceKind sourceKind, int httpStatus) => httpStatus switch
    {
        400 => Reject(sourceKind, 400, "INVALID_INPUT"),
        401 => Reject(sourceKind, 401, "UNAUTHORIZED"),
        403 => Reject(sourceKind, 403, "FORBIDDEN"),
        429 => Reject(sourceKind, 429, "RATE_LIMITED"),
        _ => Unavailable(sourceKind)
    };

    private static GatewayOutcome Unavailable(GatewaySourceKind sourceKind)
        => Reject(sourceKind, 503, "AUTH_UNAVAILABLE");

    private static GatewayOutcome Reject(GatewaySourceKind sourceKind, int status, string code)
        => new(GatewayOutcomeKind.Rejected, sourceKind, failure: new GatewayFailure(status, code));
}
