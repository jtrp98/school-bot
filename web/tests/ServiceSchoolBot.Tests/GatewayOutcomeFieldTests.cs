using System.Text.Json.Nodes;
using ServiceSchoolBot.Infrastructure.Authentication;
using static ServiceSchoolBot.Tests.GatewayOutcomeFixtures;

namespace ServiceSchoolBot.Tests;

public sealed class GatewayOutcomeFieldTests
{
    public static IEnumerable<object[]> InvalidRequiredFields()
    {
        foreach (var kind in Kinds.Where(kind => kind != GatewayOutcomeKind.Rejected))
        foreach (var field in Results(kind))
        {
            yield return [kind, field.Key, "absent"];
            foreach (var invalid in new[] { "null", "{}", "[]", "true", "false", "0", "\"\"" })
            {
                if (field.Key == "requirePasswordChange" &&
                    invalid == (kind == GatewayOutcomeKind.PasswordChange ? "true" : "false")) continue;
                if (field.Key == "resendAfterSeconds" && invalid == "0") continue;
                yield return [kind, field.Key, invalid];
            }
        }
    }

    [Theory]
    [MemberData(nameof(InvalidRequiredFields))]
    public void MissingNullEmptyOrWronglyTypedRequiredFieldRejects(
        GatewayOutcomeKind kind, string field, string mutation)
    {
        var results = Results(kind);
        if (mutation == "absent") results.Remove(field);
        else results[field] = JsonNode.Parse(mutation);
        using var outcome = Decode(results, kind);
        Rejected(outcome);
    }

    public static IEnumerable<object[]> ForbiddenFields()
    {
        var forbidden = new Dictionary<GatewayOutcomeKind, string[]>
        {
            [GatewayOutcomeKind.PasswordChange] =
                ["token", "next", "challengeToken", "expiresAt", "trustedDeviceToken", "ref", "masked",
                    "resendAfterSeconds"],
            [GatewayOutcomeKind.TwoFactor] =
                ["token", "changeToken", "trustedDeviceToken", "ref", "masked", "resendAfterSeconds"],
            [GatewayOutcomeKind.Contact] =
                ["token", "changeToken", "trustedDeviceToken", "ref", "masked", "resendAfterSeconds"],
            [GatewayOutcomeKind.TerminalCandidate] =
                ["next", "changeToken", "challengeToken", "ref", "masked", "resendAfterSeconds"],
            [GatewayOutcomeKind.Sent] =
                ["token", "changeToken", "challengeToken", "next", "requirePasswordChange", "trustedDeviceToken"]
        };
        foreach (var row in forbidden)
        foreach (var field in row.Value)
        foreach (var value in new[] { "null", "\"sentinel-secret\"", "true", "{}" })
            yield return [row.Key, field, value];
    }

    [Theory]
    [MemberData(nameof(ForbiddenFields))]
    public void ForbiddenFieldsRejectByPresenceEvenWhenNull(GatewayOutcomeKind kind, string field, string value)
    {
        var results = Results(kind);
        results[field] = JsonNode.Parse(value);
        using var outcome = Decode(results, kind);
        Rejected(outcome);
    }

    [Theory]
    [InlineData("two-factor")]
    [InlineData("contact")]
    public void ChallengeAcceptsAnExplicitFalsePasswordMarker(string next)
    {
        var kind = next == "contact" ? GatewayOutcomeKind.Contact : GatewayOutcomeKind.TwoFactor;
        var results = Results(kind);
        results["requirePasswordChange"] = false;
        using var outcome = Decode(results, kind);
        Exact(outcome, kind, GatewayStage.Initial);
    }

    [Theory]
    [InlineData("pending_device")]
    [InlineData("password_reset")]
    [InlineData("Two-Factor")]
    [InlineData("two-factor ")]
    [InlineData("unknown")]
    public void UnknownNextNeverFallsBackToTerminal(string next)
    {
        var results = Results(GatewayOutcomeKind.TwoFactor);
        results["next"] = next;
        using var outcome = Decode(results, GatewayOutcomeKind.TwoFactor);
        Rejected(outcome);
    }

    [Theory]
    [InlineData("null")]
    [InlineData("\"test-device\"")]
    public void TerminalAcceptsOnlyTheDocumentedOptionalTrustedDeviceToken(string value)
    {
        var results = Results(GatewayOutcomeKind.TerminalCandidate);
        results["trustedDeviceToken"] = JsonNode.Parse(value);
        using var outcome = Decode(results, GatewayOutcomeKind.TerminalCandidate);
        Assert.Equal(GatewayOutcomeKind.TerminalCandidate, outcome.Kind);
        Secret(outcome.Token, "test-token");
        Secret(outcome.TrustedDeviceToken, value == "null" ? null : "test-device");
        Assert.Null(outcome.NextStage);
        Assert.Null(outcome.Failure);
    }

    public static IEnumerable<object[]> InvalidSecrets()
    {
        foreach (var field in new[] { "token", "changeToken", "challengeToken", "trustedDeviceToken" })
        foreach (var value in new[]
        {
            "", " prefix", "suffix ", "a\tb", "a\nb", "a\u0000b", "a\u007fb", "a\u0085b",
            "a\u00a0b", "a\u2003b", "a\u2028b", "a\u3000b", new string('s', 16_385)
        })
            yield return [field, value];
    }

    [Theory]
    [MemberData(nameof(InvalidSecrets))]
    public void SecretsRejectWhitespaceControlEmptyAndOversizedValues(string field, string value)
    {
        var kind = field switch
        {
            "changeToken" => GatewayOutcomeKind.PasswordChange,
            "challengeToken" => GatewayOutcomeKind.TwoFactor,
            _ => GatewayOutcomeKind.TerminalCandidate
        };
        var results = Results(kind);
        results[field] = value;
        using var outcome = Decode(results, kind);
        Rejected(outcome);
    }

    [Theory]
    [InlineData("true")]
    [InlineData("0")]
    [InlineData("{}")]
    [InlineData("[]")]
    public void TrustedDeviceTokenRejectsNonStringValues(string value)
    {
        var results = Results(GatewayOutcomeKind.TerminalCandidate);
        results["trustedDeviceToken"] = JsonNode.Parse(value);
        using var outcome = Decode(results, GatewayOutcomeKind.TerminalCandidate);
        Rejected(outcome);
    }

    [Theory]
    [InlineData("ref")]
    [InlineData("masked")]
    public void PresentationUsesUtf16LengthAndRejectsControlsWithoutTrimming(string field)
    {
        foreach (var value in new[] { new string('r', 512), string.Concat(Enumerable.Repeat("😀", 256)), " m " })
        {
            var results = Results(GatewayOutcomeKind.Sent);
            results[field] = value;
            using var outcome = Decode(results, GatewayOutcomeKind.Sent);
            Assert.Equal(GatewayOutcomeKind.Sent, outcome.Kind);
            Assert.Equal(value, field == "ref" ? outcome.Ref : outcome.Masked);
        }
        foreach (var value in new[] { new string('r', 513), string.Concat(Enumerable.Repeat("😀", 257)), "x\ny" })
        {
            var results = Results(GatewayOutcomeKind.Sent);
            results[field] = value;
            using var outcome = Decode(results, GatewayOutcomeKind.Sent);
            Rejected(outcome);
        }
    }
}
