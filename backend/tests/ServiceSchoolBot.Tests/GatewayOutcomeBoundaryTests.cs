using System.Globalization;
using System.Text.Json.Nodes;
using ServiceSchoolBot.Infrastructure.Authentication;
using static ServiceSchoolBot.Tests.GatewayOutcomeFixtures;

namespace ServiceSchoolBot.Tests;

public sealed class GatewayOutcomeBoundaryTests
{
    [Theory]
    [InlineData("token")]
    [InlineData("changeToken")]
    [InlineData("trustedDeviceToken")]
    public void SecretLimitMeasuresUtf8AfterUnescaping(string field)
    {
        var kind = field == "changeToken" ? GatewayOutcomeKind.PasswordChange : GatewayOutcomeKind.TerminalCandidate;
        foreach (var value in new[] { new string('s', 16_384), new string('é', 8_192) })
        {
            var results = Results(kind);
            results[field] = value;
            using var exact = Decode(results, kind);
            Assert.Equal(kind, exact.Kind);
            Secret(field == "token" ? exact.Token : field == "changeToken" ?
                exact.ChangeToken : exact.TrustedDeviceToken, value);
            results[field] = value + "s";
            using var oversized = Decode(results, kind);
            Rejected(oversized);
        }
    }

    [Theory]
    [InlineData(200, true)]
    [InlineData(201, false)]
    public void ChallengeLimitMeasuresUtf16CodeUnits(int units, bool accepted)
    {
        foreach (var value in new[] { new string('c', units),
            string.Concat(Enumerable.Repeat("😀", units / 2)) + (units % 2 == 0 ? "" : "c") })
        {
            var results = Results(GatewayOutcomeKind.TwoFactor);
            results["challengeToken"] = value;
            using var outcome = Decode(results, GatewayOutcomeKind.TwoFactor);
            if (accepted)
            {
                Assert.Equal(GatewayOutcomeKind.TwoFactor, outcome.Kind);
                Secret(outcome.ChallengeToken, value);
            }
            else Rejected(outcome);
        }
    }

    [Theory]
    [InlineData("2032-02-29T12:34:56Z", "2032-02-29T12:34:56Z")]
    [InlineData("2000-02-29T00:00:00Z", "2000-02-29T00:00:00Z")]
    [InlineData("2000-01-01T00:00:00Z", "2000-01-01T00:00:00Z")]
    [InlineData("2030-01-02T10:04:05+07:00", "2030-01-02T03:04:05Z")]
    [InlineData("2030-01-01T13:04:05-14:00", "2030-01-02T03:04:05Z")]
    [InlineData("2030-01-02T17:04:05+14:00", "2030-01-02T03:04:05Z")]
    [InlineData("2030-01-02T03:04:05.1234567Z", "2030-01-02T03:04:05.1234567Z")]
    [InlineData("2030-01-02T03:04:05.1Z", "2030-01-02T03:04:05.1Z")]
    [InlineData("0001-01-01T00:00:00Z", "0001-01-01T00:00:00Z")]
    [InlineData("9999-12-31T23:59:59.9999999Z", "9999-12-31T23:59:59.9999999Z")]
    public void ValidGregorianDatesNormalizeToUtcWithoutAuthenticatingOrRejectingThePast(
        string input, string normalized)
    {
        foreach (var kind in new[] { GatewayOutcomeKind.TwoFactor, GatewayOutcomeKind.Contact,
            GatewayOutcomeKind.TerminalCandidate, GatewayOutcomeKind.Sent })
        {
            var results = Results(kind);
            results["expiresAt"] = input;
            using var outcome = Decode(results, kind);
            Assert.Equal(kind, outcome.Kind);
            Assert.Equal(DateTimeOffset.Parse(normalized, CultureInfo.InvariantCulture), outcome.ExpiresAt);
            Assert.Equal(TimeSpan.Zero, outcome.ExpiresAt!.Value.Offset);
        }
    }

    [Theory]
    [InlineData("2030-02-29T00:00:00Z")]
    [InlineData("1900-02-29T00:00:00Z")]
    [InlineData("0000-01-01T00:00:00Z")]
    [InlineData("10000-01-01T00:00:00Z")]
    [InlineData("2030-13-01T00:00:00Z")]
    [InlineData("2030-04-31T00:00:00Z")]
    [InlineData("2030-01-02T03:04:05")]
    [InlineData("2030-01-02T03:04:05+14:01")]
    [InlineData("2030-01-02T03:04:05-14:01")]
    [InlineData("2030-01-02T03:04:05+15:00")]
    [InlineData("2030-01-02T03:04:60Z")]
    [InlineData("2030-01-02T24:00:00Z")]
    [InlineData("2030-01-02T03:60:00Z")]
    [InlineData("2030-01-02T03:04:05.12345678Z")]
    [InlineData("2030-01-02T03:04:05.Z")]
    [InlineData("2030-01-02 03:04:05Z")]
    [InlineData("2030-01-02t03:04:05z")]
    [InlineData("2030-01-02T03:04:05+0700")]
    [InlineData(" 2030-01-02T03:04:05Z")]
    [InlineData("1893553445")]
    public void InvalidDatesAndEpochStringsFailClosed(string input)
    {
        var results = Results(GatewayOutcomeKind.TerminalCandidate);
        results["expiresAt"] = input;
        using var outcome = Decode(results, GatewayOutcomeKind.TerminalCandidate);
        Rejected(outcome);
    }

    [Theory]
    [InlineData("0", 0)]
    [InlineData("2147483647", int.MaxValue)]
    public void ResendAcceptsInclusiveIntegerBounds(string value, int expected)
    {
        var results = Results(GatewayOutcomeKind.Sent);
        results["resendAfterSeconds"] = JsonNode.Parse(value);
        using var outcome = Decode(results, GatewayOutcomeKind.Sent);
        Assert.Equal(GatewayOutcomeKind.Sent, outcome.Kind);
        Assert.Equal(expected, outcome.ResendAfterSeconds);
    }

    [Theory]
    [InlineData("-1")]
    [InlineData("-0")]
    [InlineData("2147483648")]
    [InlineData("1.0")]
    [InlineData("1e0")]
    [InlineData("\"1\"")]
    [InlineData("null")]
    [InlineData("true")]
    public void ResendRejectsNonLexicalIntegersAndOverflow(string value)
    {
        var results = Results(GatewayOutcomeKind.Sent).ToJsonString();
        using var outcome = DecodeRaw("{\"code\":200,\"results\":" +
            results.Replace(":60", ":" + value, StringComparison.Ordinal) + "}",
            GatewayOperation.TwoFactorSend, GatewayStage.TwoFactor);
        Rejected(outcome);
    }
}
