using System.Text.Json.Nodes;
using ServiceSchoolBot.Infrastructure.Authentication;
using static ServiceSchoolBot.Tests.GatewayOutcomeFixtures;

namespace ServiceSchoolBot.Tests;

public sealed class GatewayOutcomeHttpTests
{
    public static IEnumerable<object[]> HttpErrors()
    {
        for (var status = 100; status <= 599; status++)
        {
            if (status == 200) continue;
            foreach (var body in new[] { "", " \t\r\n", "{}", "{\"results\":null}",
                $"{{\"code\":{status},\"results\":{{}}}}", $"{{\"code\":\"{status}\"}}" })
                yield return [status, body];
        }
    }

    [Theory]
    [MemberData(nameof(HttpErrors))]
    public void NonSuccessNeverReturnsAPayloadAndMapsOnlyTheDeclaredErrors(int status, string body)
    {
        var (expectedStatus, code) = status switch
        {
            400 => (400, "INVALID_INPUT"),
            401 => (401, "UNAUTHORIZED"),
            403 => (403, "FORBIDDEN"),
            429 => (429, "RATE_LIMITED"),
            _ => (503, "AUTH_UNAVAILABLE")
        };
        using var outcome = DecodeRaw(body, status: status);
        Rejected(outcome, expectedStatus, code);
    }

    public static IEnumerable<object[]> MalformedErrors()
    {
        foreach (var status in new[] { 400, 401, 403, 429, 502, 503 })
        {
            foreach (var body in new[]
            {
                "not-json sentinel-person@example.invalid", "[]", "null", "{", "{\"results\":[]}",
                "{\"results\":false}", "{\"code\":200}", "{\"code\":null}", "{\"code\":true}",
                "{\"unknown\":{\"x\":1,\"x\":2}}"
            }) yield return [status, body];
            foreach (var marker in new[] { "token", "changeToken", "challengeToken", "next", "requirePasswordChange" })
            foreach (var value in new[] { "null", "\"sentinel-secret\"" })
                yield return [status, $"{{\"code\":{status},\"results\":{{\"{marker}\":{value}}}}}"];
        }
    }

    [Theory]
    [MemberData(nameof(MalformedErrors))]
    public void MalformedOrConflictingErrorBodiesFailClosed(int status, string body)
    {
        using var outcome = DecodeRaw(body, status: status);
        Rejected(outcome);
    }

    [Theory]
    [InlineData("null")]
    [InlineData("true")]
    [InlineData("200.0")]
    [InlineData("2e2")]
    [InlineData("\" 200\"")]
    [InlineData("\"200 \"")]
    [InlineData("\"0200\"")]
    [InlineData("99")]
    [InlineData("600")]
    [InlineData("401")]
    [InlineData("[]")]
    [InlineData("{}")]
    public void SuccessRequiresAnExactMatchingCode(string code)
    {
        var results = Results(GatewayOutcomeKind.TerminalCandidate).ToJsonString();
        using var outcome = DecodeRaw($"{{\"code\":{code},\"results\":{results}}}");
        Rejected(outcome);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" \r\n\t")]
    [InlineData("{}")]
    [InlineData("{\"code\":200}")]
    [InlineData("{\"code\":200,\"results\":null}")]
    [InlineData("{\"code\":200,\"results\":[]}")]
    [InlineData("{\"code\":200,\"results\":1}")]
    [InlineData("{\"results\":{\"token\":\"test-token\"}}")]
    public void SuccessCannotOmitItsEnvelope(string body)
    {
        using var outcome = DecodeRaw(body);
        Rejected(outcome);
    }

    [Theory]
    [InlineData("200")]
    [InlineData("\"200\"")]
    public void BothDeclaredCodeRepresentationsPreserveTheExactPayload(string code)
    {
        using var outcome = DecodeRaw($"{{\"code\":{code},\"results\":" +
            Results(GatewayOutcomeKind.TerminalCandidate).ToJsonString() + "}");
        Exact(outcome, GatewayOutcomeKind.TerminalCandidate, GatewayStage.Initial);
    }

    [Theory]
    [InlineData("null")]
    [InlineData("42")]
    [InlineData("true")]
    [InlineData("\"sentinel-secret@example.invalid\"")]
    [InlineData("{\"role\":\"admin\",\"token\":\"sentinel-secret\"}")]
    [InlineData("[null,{\"unknown\":false}]")]
    public void PresentationValuesAreDiscardedRatherThanUsedAsAuthority(string presentation)
    {
        foreach (var kind in Kinds.Where(kind => kind != GatewayOutcomeKind.Rejected))
        {
            var results = Results(kind);
            foreach (var key in new[] { "channels", "missing", "user", "retryAfterSeconds", "unknown" })
                results[key] = JsonNode.Parse(presentation);
            var root = new JsonObject { ["code"] = 200, ["results"] = results };
            foreach (var key in new[] { "message", "messageTh", "requestId", "unknown" })
                root[key] = JsonNode.Parse(presentation);
            using var outcome = DecodeRaw(root.ToJsonString(),
                kind == GatewayOutcomeKind.Sent ? GatewayOperation.ContactSend : GatewayOperation.Login,
                kind == GatewayOutcomeKind.Sent ? GatewayStage.Contact : GatewayStage.Initial);
            Exact(outcome, kind, kind == GatewayOutcomeKind.Sent ? GatewayStage.Contact : GatewayStage.Initial);
        }
    }
}
