using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;
using ServiceSchoolBot.Infrastructure.Authentication;

namespace ServiceSchoolBot.Tests;

internal static class GatewayOutcomeFixtures
{
    internal const string Timestamp = "2030-01-02T03:04:05Z";
    internal static readonly DateTimeOffset Instant = new(2030, 1, 2, 3, 4, 5, TimeSpan.Zero);
    internal static readonly GatewayOutcomeKind[] Kinds = Enum.GetValues<GatewayOutcomeKind>();

    internal static JsonObject Results(GatewayOutcomeKind kind) => kind switch
    {
        GatewayOutcomeKind.PasswordChange => new()
        {
            ["requirePasswordChange"] = true,
            ["changeToken"] = "test-change"
        },
        GatewayOutcomeKind.TwoFactor or GatewayOutcomeKind.Contact => new()
        {
            ["next"] = kind == GatewayOutcomeKind.TwoFactor ? "two-factor" : "contact",
            ["challengeToken"] = "test-challenge",
            ["expiresAt"] = Timestamp
        },
        GatewayOutcomeKind.TerminalCandidate => new()
        {
            ["token"] = "test-token",
            ["requirePasswordChange"] = false,
            ["expiresAt"] = Timestamp
        },
        GatewayOutcomeKind.Sent => new()
        {
            ["ref"] = "r",
            ["masked"] = "m",
            ["expiresAt"] = Timestamp,
            ["resendAfterSeconds"] = 60
        },
        _ => new()
    };

    internal static GatewayOutcome Decode(JsonObject results, GatewayOutcomeKind kind) => DecodeRaw(
        new JsonObject { ["code"] = 200, ["results"] = results.DeepClone() }.ToJsonString(),
        kind == GatewayOutcomeKind.Sent ? GatewayOperation.TwoFactorSend : GatewayOperation.Login,
        kind == GatewayOutcomeKind.Sent ? GatewayStage.TwoFactor : GatewayStage.Initial);

    internal static GatewayOutcome DecodeRaw(string json,
        GatewayOperation operation = GatewayOperation.Login, GatewayStage stage = GatewayStage.Initial,
        int status = 200) => GatewayOutcomeDecoder.Decode(
            GatewaySourceKind.Web, operation, stage, status, Encoding.UTF8.GetBytes(json));

    internal static void Rejected(GatewayOutcome outcome, int status = 503, string code = "AUTH_UNAVAILABLE",
        GatewaySourceKind source = GatewaySourceKind.Web)
    {
        Assert.Equal(GatewayOutcomeKind.Rejected, outcome.Kind);
        Assert.Equal(source, outcome.SourceKind);
        Assert.NotNull(outcome.Failure);
        Assert.Equal(status, outcome.Failure.Status);
        Assert.Equal(code, outcome.Failure.Code);
        Assert.Null(outcome.NextStage);
        Assert.Null(outcome.Token);
        Assert.Null(outcome.ChangeToken);
        Assert.Null(outcome.ChallengeToken);
        Assert.Null(outcome.TrustedDeviceToken);
        Assert.Null(outcome.ExpiresAt);
        Assert.Null(outcome.Ref);
        Assert.Null(outcome.Masked);
        Assert.Null(outcome.ResendAfterSeconds);
        Assert.Equal("Rejected", outcome.ToString());
    }

    internal static void Exact(GatewayOutcome outcome, GatewayOutcomeKind kind, GatewayStage inputStage)
    {
        Assert.Equal(kind, outcome.Kind);
        Assert.Equal(GatewaySourceKind.Web, outcome.SourceKind);
        Assert.Null(outcome.Failure);
        GatewayStage? next = kind switch
        {
            GatewayOutcomeKind.PasswordChange => GatewayStage.PasswordChange,
            GatewayOutcomeKind.TwoFactor => GatewayStage.TwoFactor,
            GatewayOutcomeKind.Contact => GatewayStage.Contact,
            GatewayOutcomeKind.Sent => inputStage,
            _ => null
        };
        Assert.Equal(next, outcome.NextStage);
        Secret(outcome.Token, kind == GatewayOutcomeKind.TerminalCandidate ? "test-token" : null);
        Secret(outcome.ChangeToken, kind == GatewayOutcomeKind.PasswordChange ? "test-change" : null);
        Secret(outcome.ChallengeToken,
            kind is GatewayOutcomeKind.TwoFactor or GatewayOutcomeKind.Contact ? "test-challenge" : null);
        Assert.Null(outcome.TrustedDeviceToken);
        Assert.Equal(kind == GatewayOutcomeKind.PasswordChange ? null : (DateTimeOffset?)Instant, outcome.ExpiresAt);
        Assert.Equal(kind == GatewayOutcomeKind.Sent ? "r" : null, outcome.Ref);
        Assert.Equal(kind == GatewayOutcomeKind.Sent ? "m" : null, outcome.Masked);
        Assert.Equal(kind == GatewayOutcomeKind.Sent ? 60 : (int?)null, outcome.ResendAfterSeconds);
        Assert.Equal(kind.ToString(), outcome.ToString());
    }

    internal static void Secret(GatewaySecret? secret, string? expected)
    {
        if (expected is null)
        {
            Assert.Null(secret);
            return;
        }
        Assert.NotNull(secret);
        var bytes = Encoding.UTF8.GetBytes(expected);
        var destination = new byte[bytes.Length];
        try
        {
            Assert.Equal(bytes.Length, secret.ByteLength);
            Assert.Equal(bytes.Length, secret.CopyTo(destination));
            Assert.Equal(bytes, destination);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(bytes);
            CryptographicOperations.ZeroMemory(destination);
        }
    }
}
