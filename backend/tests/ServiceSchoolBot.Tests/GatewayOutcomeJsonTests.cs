using System.Text;
using System.Text.Json.Nodes;
using ServiceSchoolBot.Infrastructure.Authentication;
using static ServiceSchoolBot.Tests.GatewayOutcomeFixtures;

namespace ServiceSchoolBot.Tests;

public sealed class GatewayOutcomeJsonTests
{
    private static string Envelope(string extra) => "{\"code\":200,\"results\":" +
        Results(GatewayOutcomeKind.TerminalCandidate).ToJsonString() + extra + "}";

    public static IEnumerable<object[]> Duplicates()
    {
        yield return [Envelope(",\"code\":200")];
        yield return [Envelope(",\"co\\u0064e\":200")];
        yield return [Envelope(",\"results\":{}")];
        yield return [Envelope(",\"unknown\":{\"x\":0,\"x\":1}")];
        yield return [Envelope(",\"unknown\":[{\"x\":0,\"\\u0078\":1}]")];
        yield return [Envelope(",\"unknown\":{\"a\":[{\"é\":0,\"\\u00e9\":1}]}")];
        foreach (var key in new[] { "token", "next", "channels", "missing", "user", "unknown" })
        {
            var results = Results(GatewayOutcomeKind.TerminalCandidate).ToJsonString();
            yield return ["{\"code\":200,\"results\":" + results[..^1] + $",\"{key}\":null,\"{key}\":null}}}}"];
            yield return ["{\"code\":200,\"results\":" + results[..^1] +
                $",\"{key}\":{{\"a\":[{{\"x\":0,\"x\":1}}]}}}}}}"];
        }
    }

    [Theory]
    [MemberData(nameof(Duplicates))]
    public void DuplicateNamesAreRejectedThroughoutDiscardedAndRetainedSubtrees(string json)
    {
        using var outcome = DecodeRaw(json);
        Rejected(outcome);
    }

    public static IEnumerable<object[]> WrongCase()
    {
        foreach (var rootKey in new[] { "code", "results", "message", "messageTh", "requestId" })
            yield return [true, rootKey];
        foreach (var resultKey in new[]
        {
            "requirePasswordChange", "changeToken", "next", "challengeToken", "expiresAt", "token",
            "trustedDeviceToken", "ref", "masked", "resendAfterSeconds", "channels", "missing", "user",
            "retryAfterSeconds", "temporaryToken", "isDeviceVerified", "forceResetPassword"
        }) yield return [false, resultKey];
    }

    [Theory]
    [MemberData(nameof(WrongCase))]
    public void KnownNamesRejectCaseAliasesAtBothAuthorityLevels(bool atRoot, string key)
    {
        var results = Results(GatewayOutcomeKind.TerminalCandidate);
        var root = new JsonObject { ["code"] = 200, ["results"] = results };
        (atRoot ? root : results)[key.ToUpperInvariant()] = null;
        using var outcome = DecodeRaw(root.ToJsonString());
        Rejected(outcome);
    }

    [Theory]
    [InlineData("null")]
    [InlineData("[]")]
    [InlineData("1")]
    [InlineData("{} {}")]
    [InlineData("{\"code\":200,}")]
    [InlineData("{/*comment*/\"code\":200}")]
    [InlineData("{\"unknown\":\"\\uD800\"}")]
    [InlineData("{\"unknown\":\"\\uDC00\"}")]
    [InlineData("{\"unknown\":\"\\uD800\\u0041\"}")]
    [InlineData("{\"\\uD800\":0}")]
    [InlineData("{\"unknown\":\"\\u12\"}")]
    [InlineData("{\"unknown\":\"\\uZZZZ\"}")]
    public void MalformedDocumentsAndInvalidEscapedUnicodeRejectWithoutThrowing(string json)
    {
        using var outcome = DecodeRaw(json);
        Rejected(outcome);
    }

    [Fact]
    public void TruncatedTrailingBomAndInvalidUtf8AreRejectedEvenInsideUnknownStrings()
    {
        var valid = Envelope(",\"unknown\":\"x\"");
        var bytes = Encoding.UTF8.GetBytes(valid);
        var unknownIndex = valid.LastIndexOf('x');
        var badUtf8 = new byte[][]
        {
            [0x80], [0xc0, 0xaf], [0xed, 0xa0, 0x80], [0xf4, 0x90, 0x80, 0x80], [0xe2, 0x82]
        };
        List<byte[]> documents = [bytes[..^1], [.. bytes, .. bytes], [0xef, 0xbb, 0xbf, .. bytes]];
        documents.AddRange(badUtf8.Select(invalid =>
            bytes[..unknownIndex].Concat(invalid).Concat(bytes[(unknownIndex + 1)..]).ToArray()));
        foreach (var json in documents)
        {
            using var outcome = GatewayOutcomeDecoder.Decode(
                GatewaySourceKind.Web, GatewayOperation.Login, GatewayStage.Initial, 200, json);
            Rejected(outcome);
        }
    }

    [Fact]
    public void EscapesAreDecodedExactlyAndUnknownNamesUseOrdinalComparison()
    {
        var json = Envelope(",\"unknown\":{\"X\":0,\"x\":1,\"é\":2,\"e\\u0301\":3}")
            .Replace("test-token", "test-\\u0E01\\uD83D\\uDE00", StringComparison.Ordinal);
        using var outcome = DecodeRaw(json);
        Assert.Equal(GatewayOutcomeKind.TerminalCandidate, outcome.Kind);
        Secret(outcome.Token, "test-ก😀");
    }

    [Fact]
    public void KnownNamesAreComparedAfterUnescaping()
    {
        var valid = Envelope("").Replace("\"token\"", "\"to\\u006ben\"", StringComparison.Ordinal);
        using var accepted = DecodeRaw(valid);
        Exact(accepted, GatewayOutcomeKind.TerminalCandidate, GatewayStage.Initial);
        using var wrongCase = DecodeRaw(valid.Replace("to\\u006ben", "\\u0054oken", StringComparison.Ordinal));
        Rejected(wrongCase);
        using var wrongRootCase = DecodeRaw(valid.Replace("\"code\"", "\"\\u0043ode\"", StringComparison.Ordinal));
        Rejected(wrongRootCase);
    }

    [Theory]
    [InlineData(2_097_152, true)]
    [InlineData(2_097_153, false)]
    public void BodyLimitCountsAllInputBytesBeforeParsing(int size, bool accepted)
    {
        var prefix = Envelope(",\"filler\":\"");
        var json = prefix[..^1] + new string('p', size - Encoding.UTF8.GetByteCount(prefix) - 1) + "\"}";
        Assert.Equal(size, Encoding.UTF8.GetByteCount(json));
        using var outcome = DecodeRaw(json);
        if (accepted) Exact(outcome, GatewayOutcomeKind.TerminalCandidate, GatewayStage.Initial);
        else Rejected(outcome);
    }

    [Theory]
    [InlineData(32, true)]
    [InlineData(33, false)]
    public void DepthCountsTheRootContainerAndValidatesUnknownArrays(int depth, bool accepted)
    {
        var json = Envelope(",\"unknown\":" + new string('[', depth - 1) + "0" + new string(']', depth - 1));
        using var outcome = DecodeRaw(json);
        if (accepted) Exact(outcome, GatewayOutcomeKind.TerminalCandidate, GatewayStage.Initial);
        else Rejected(outcome);
    }
}
