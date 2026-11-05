using System.Text.Json;
using System.Text.Json.Serialization;

namespace ServiceSchoolBot.Infrastructure.Authentication;

public enum GatewaySourceKind { Web, Mobile }
public enum GatewayOperation { Login, FirstPassword, TwoFactorSend, TwoFactorVerify, ContactSend, ContactVerify }
public enum GatewayStage { Initial, PasswordChange, TwoFactor, Contact }
public enum GatewayOutcomeKind { Rejected, PasswordChange, TwoFactor, Contact, Sent, TerminalCandidate }

public sealed class GatewayFailure
{
    internal GatewayFailure(int status, string code)
    {
        Status = status;
        Code = code;
    }

    public int Status { get; }
    public string Code { get; }
}

[JsonConverter(typeof(GatewayOutcome.SerializationConverter))]
public sealed class GatewayOutcome : IDisposable
{
    internal GatewayOutcome(
        GatewayOutcomeKind kind,
        GatewaySourceKind sourceKind,
        GatewayStage? nextStage = null,
        GatewaySecret? token = null,
        GatewaySecret? changeToken = null,
        GatewaySecret? challengeToken = null,
        GatewaySecret? trustedDeviceToken = null,
        DateTimeOffset? expiresAt = null,
        string? reference = null,
        string? masked = null,
        int? resendAfterSeconds = null,
        GatewayFailure? failure = null)
    {
        Kind = kind;
        SourceKind = sourceKind;
        NextStage = nextStage;
        Token = token;
        ChangeToken = changeToken;
        ChallengeToken = challengeToken;
        TrustedDeviceToken = trustedDeviceToken;
        ExpiresAt = expiresAt;
        Ref = reference;
        Masked = masked;
        ResendAfterSeconds = resendAfterSeconds;
        Failure = failure;
    }

    public GatewayOutcomeKind Kind { get; }
    public GatewaySourceKind SourceKind { get; }
    public GatewayStage? NextStage { get; }
    public GatewaySecret? Token { get; }
    public GatewaySecret? ChangeToken { get; }
    public GatewaySecret? ChallengeToken { get; }
    public GatewaySecret? TrustedDeviceToken { get; }
    public DateTimeOffset? ExpiresAt { get; }
    public string? Ref { get; }
    public string? Masked { get; }
    public int? ResendAfterSeconds { get; }
    public GatewayFailure? Failure { get; }

    public void Dispose()
    {
        Token?.Dispose();
        ChangeToken?.Dispose();
        ChallengeToken?.Dispose();
        TrustedDeviceToken?.Dispose();
    }

    public override string ToString() => Kind.ToString();

    private sealed class SerializationConverter : JsonConverter<GatewayOutcome>
    {
        public SerializationConverter() { }

        public override bool HandleNull => true;

        public override GatewayOutcome Read(
            ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => throw new NotSupportedException("Gateway outcomes cannot be deserialized.");

        public override void Write(Utf8JsonWriter writer, GatewayOutcome value, JsonSerializerOptions options)
            => throw new NotSupportedException("Gateway outcomes cannot be serialized.");
    }
}
