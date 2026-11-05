using System.Text;
using System.Text.Json.Nodes;
using ServiceSchoolBot.Infrastructure.Authentication;
using static ServiceSchoolBot.Tests.GatewayOutcomeFixtures;

namespace ServiceSchoolBot.Tests;

public sealed class GatewayOutcomeMatrixTests
{
    private static readonly (GatewayOperation Operation, GatewayStage Stage, GatewayOutcomeKind[] Allowed)[] Rows =
    [
        (GatewayOperation.Login, GatewayStage.Initial,
            [GatewayOutcomeKind.PasswordChange, GatewayOutcomeKind.TwoFactor,
                GatewayOutcomeKind.Contact, GatewayOutcomeKind.TerminalCandidate]),
        (GatewayOperation.FirstPassword, GatewayStage.PasswordChange,
            [GatewayOutcomeKind.TwoFactor, GatewayOutcomeKind.Contact, GatewayOutcomeKind.TerminalCandidate]),
        (GatewayOperation.TwoFactorSend, GatewayStage.TwoFactor, [GatewayOutcomeKind.Sent]),
        (GatewayOperation.TwoFactorVerify, GatewayStage.TwoFactor,
            [GatewayOutcomeKind.Contact, GatewayOutcomeKind.TerminalCandidate]),
        (GatewayOperation.ContactSend, GatewayStage.Contact, [GatewayOutcomeKind.Sent]),
        (GatewayOperation.ContactVerify, GatewayStage.Contact,
            [GatewayOutcomeKind.Contact, GatewayOutcomeKind.TerminalCandidate])
    ];

    public static IEnumerable<object[]> Matrix()
    {
        foreach (var row in Rows)
        foreach (var stage in Enum.GetValues<GatewayStage>())
        foreach (var kind in Kinds)
        foreach (var source in Enum.GetValues<GatewaySourceKind>())
            yield return [row.Operation, stage, kind, source, stage == row.Stage, row.Allowed.Contains(kind)];
    }

    [Theory]
    [MemberData(nameof(Matrix))]
    public void OnlyTheDeclaredSourceStageAndOutcomeCombinationIsAccepted(
        GatewayOperation operation, GatewayStage stage, GatewayOutcomeKind kind,
        GatewaySourceKind source, bool correctStage, bool allowed)
    {
        var json = new JsonObject { ["code"] = 200, ["results"] = Results(kind) }.ToJsonString();
        using var outcome = GatewayOutcomeDecoder.Decode(source, operation, stage, 200, Encoding.UTF8.GetBytes(json));
        if (source == GatewaySourceKind.Mobile || !correctStage)
            Rejected(outcome, 400, "INVALID_INPUT", source);
        else if (!allowed)
            Rejected(outcome);
        else
            Exact(outcome, kind, stage);
    }

    public static IEnumerable<object[]> InvalidArguments()
    {
        foreach (var invalid in new[] { -1, 2, int.MaxValue })
            yield return [(GatewaySourceKind)invalid, GatewayOperation.Login, GatewayStage.Initial, 200];
        foreach (var invalid in new[] { -1, 6, int.MaxValue })
            yield return [GatewaySourceKind.Web, (GatewayOperation)invalid, GatewayStage.Initial, 200];
        foreach (var invalid in new[] { -1, 4, int.MaxValue })
            yield return [GatewaySourceKind.Web, GatewayOperation.Login, (GatewayStage)invalid, 200];
        foreach (var invalid in new[] { int.MinValue, 0, 99, 600, int.MaxValue })
            yield return [GatewaySourceKind.Web, GatewayOperation.Login, GatewayStage.Initial, invalid];
    }

    [Theory]
    [MemberData(nameof(InvalidArguments))]
    public void InvalidArgumentsRejectBeforeDecoding(
        GatewaySourceKind source, GatewayOperation operation, GatewayStage stage, int status)
    {
        using var outcome = GatewayOutcomeDecoder.Decode(source, operation, stage, status, [0xff]);
        Rejected(outcome, 400, "INVALID_INPUT", source);
    }

    [Theory]
    [InlineData("temporaryToken")]
    [InlineData("isDeviceVerified")]
    [InlineData("forceResetPassword")]
    public void MobileMarkersNeverBecomeAWebSession(string marker)
    {
        foreach (var value in new[] { "null", "true", "\"test-secret\"" })
        {
            var results = Results(GatewayOutcomeKind.TerminalCandidate);
            results[marker] = JsonNode.Parse(value);
            using var web = Decode(results, GatewayOutcomeKind.TerminalCandidate);
            Rejected(web);
            foreach (var stage in Enum.GetValues<GatewayStage>())
            foreach (var operation in Enum.GetValues<GatewayOperation>())
            {
                using var mobile = GatewayOutcomeDecoder.Decode(GatewaySourceKind.Mobile, operation, stage, 200,
                    Encoding.UTF8.GetBytes(new JsonObject { ["code"] = 200, ["results"] = results.DeepClone() }
                        .ToJsonString()));
                Rejected(mobile, 400, "INVALID_INPUT", GatewaySourceKind.Mobile);
            }
        }
    }
}
