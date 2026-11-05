using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ServiceSchoolBot.Infrastructure.Authentication;
using static ServiceSchoolBot.Tests.GatewayOutcomeFixtures;

namespace ServiceSchoolBot.Tests;

public sealed class GatewaySecretTests
{
    [Fact]
    public void CopyToReturnsExactUnescapedBytesAndLeavesExtraSpaceUntouched()
    {
        using var outcome = DecodeRaw("{\"code\":200,\"results\":{\"token\":\"synthetic-\\u0e01\\uD83D\\uDE00\"," +
            "\"requirePasswordChange\":false,\"expiresAt\":\"2030-01-02T03:04:05Z\"}}");
        var secret = Assert.IsType<GatewaySecret>(outcome.Token);
        var expected = Encoding.UTF8.GetBytes("synthetic-ก😀");
        var destination = Enumerable.Repeat((byte)0xa5, expected.Length + 4).ToArray();
        try
        {
            Assert.Equal(expected.Length, secret.ByteLength);
            Assert.Equal(expected.Length, secret.CopyTo(destination));
            Assert.Equal(expected, destination[..expected.Length]);
            Assert.All(destination[expected.Length..], value => Assert.Equal((byte)0xa5, value));
        }
        finally
        {
            CryptographicOperations.ZeroMemory(expected);
            CryptographicOperations.ZeroMemory(destination);
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(9)]
    public void UndersizedDestinationIsUnchangedAndExceptionsDoNotContainTheSecret(int length)
    {
        using var outcome = Decode(Results(GatewayOutcomeKind.TerminalCandidate), GatewayOutcomeKind.TerminalCandidate);
        var secret = Assert.IsType<GatewaySecret>(outcome.Token);
        var destination = Enumerable.Repeat((byte)0xa5, length).ToArray();
        var error = Assert.Throws<ArgumentException>(() => secret.CopyTo(destination));
        Assert.All(destination, value => Assert.Equal((byte)0xa5, value));
        Assert.DoesNotContain("test-token", error.ToString());
    }

    [Theory]
    [InlineData(GatewayOutcomeKind.PasswordChange)]
    [InlineData(GatewayOutcomeKind.TwoFactor)]
    [InlineData(GatewayOutcomeKind.Contact)]
    [InlineData(GatewayOutcomeKind.TerminalCandidate)]
    public void OutcomeDisposesEveryOwnedSecretIdempotentlyAndZerosItsOwnedBuffer(GatewayOutcomeKind kind)
    {
        var results = Results(kind);
        if (kind == GatewayOutcomeKind.TerminalCandidate) results["trustedDeviceToken"] = "synthetic-device";
        using var outcome = Decode(results, kind);
        var secrets = new[] { outcome.Token, outcome.ChangeToken, outcome.ChallengeToken, outcome.TrustedDeviceToken }
            .OfType<GatewaySecret>().ToArray();
        Assert.NotEmpty(secrets);
        var buffers = secrets.Select(secret => Assert.IsType<byte[]>(typeof(GatewaySecret)
            .GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
            .Single(field => field.FieldType == typeof(byte[])).GetValue(secret))).ToArray();
        outcome.Dispose();
        outcome.Dispose();
        foreach (var buffer in buffers) Assert.All(buffer, value => Assert.Equal((byte)0, value));
        foreach (var secret in secrets)
        {
            secret.Dispose();
            var lengthError = Assert.Throws<ObjectDisposedException>(() => secret.ByteLength);
            var copyError = Assert.Throws<ObjectDisposedException>(() => secret.CopyTo(new byte[64]));
            Assert.DoesNotContain("synthetic-device", lengthError.ToString() + copyError);
            Assert.DoesNotContain("test-", lengthError.ToString() + copyError);
        }
    }

    [Fact]
    public void SecretCanBeDisposedBeforeItsOutcome()
    {
        using var outcome = Decode(Results(GatewayOutcomeKind.TerminalCandidate), GatewayOutcomeKind.TerminalCandidate);
        var secret = Assert.IsType<GatewaySecret>(outcome.Token);
        secret.Dispose();
        outcome.Dispose();
        Assert.Throws<ObjectDisposedException>(() => secret.CopyTo(Array.Empty<byte>()));
    }

    [Fact]
    public void DiagnosticAndSerializationPathsCannotExposeSecretsOrRawPresentation()
    {
        const string sentinel = "synthetic-secret-private@example.invalid";
        var results = Results(GatewayOutcomeKind.TerminalCandidate);
        results["token"] = sentinel;
        results["trustedDeviceToken"] = sentinel;
        results["user"] = new System.Text.Json.Nodes.JsonObject { ["pii"] = sentinel };
        using var outcome = Decode(results, GatewayOutcomeKind.TerminalCandidate);
        Assert.Equal("TerminalCandidate", outcome.ToString());
        foreach (var secret in new[] { outcome.Token!, outcome.TrustedDeviceToken! })
        {
            Assert.Equal("[REDACTED]", secret.ToString());
            Assert.DoesNotContain(sentinel, Assert.Throws<NotSupportedException>(
                () => JsonSerializer.Serialize(secret)).ToString());
            Assert.DoesNotContain(sentinel, Assert.Throws<NotSupportedException>(
                () => JsonSerializer.Serialize<object>(secret)).ToString());
        }
        Assert.DoesNotContain(sentinel, Assert.Throws<NotSupportedException>(
            () => JsonSerializer.Serialize(outcome)).ToString());
        Assert.DoesNotContain(sentinel, Assert.Throws<NotSupportedException>(
            () => JsonSerializer.Serialize<object>(outcome)).ToString());
        Assert.DoesNotContain(sentinel, Assert.Throws<NotSupportedException>(
            () => JsonSerializer.Deserialize<GatewayOutcome>("{\"token\":\"" + sentinel + "\"}")).ToString());
        Assert.DoesNotContain(sentinel, Assert.Throws<NotSupportedException>(
            () => JsonSerializer.Deserialize<GatewaySecret>("\"" + sentinel + "\"")).ToString());
    }

    [Fact]
    public void PublicSurfaceCannotBeUsedToReadClaimsOrConstructAnAuthenticatedSession()
    {
        Assert.Empty(typeof(GatewayOutcome).GetConstructors());
        Assert.Empty(typeof(GatewaySecret).GetConstructors());
        Assert.Empty(typeof(GatewayFailure).GetConstructors());
        Assert.Equal(new[] { "ByteLength" }, typeof(GatewaySecret).GetProperties().Select(property => property.Name));
        Assert.Equal(new[] { "Code", "Status" }, typeof(GatewayFailure).GetProperties()
            .Select(property => property.Name).Order());
        var expected = new[] { "ChallengeToken", "ChangeToken", "ExpiresAt", "Failure", "Kind", "Masked",
            "NextStage", "Ref", "ResendAfterSeconds", "SourceKind", "Token", "TrustedDeviceToken" };
        Assert.Equal(expected, typeof(GatewayOutcome).GetProperties().Select(property => property.Name).Order());
        Assert.All(typeof(GatewayOutcome).GetProperties(), property => Assert.Null(property.SetMethod));
        Assert.DoesNotContain(typeof(GatewaySecret).GetMethods(BindingFlags.Public | BindingFlags.Instance |
            BindingFlags.Static | BindingFlags.DeclaredOnly), method => method.ReturnType == typeof(byte[]) ||
            method.Name.StartsWith("op_", StringComparison.Ordinal));
        Assert.All(typeof(GatewayOutcomeDecoder).Assembly.GetReferencedAssemblies(), reference =>
            Assert.NotEqual("ServiceSchoolBot.API", reference.Name));
    }

    [Fact]
    public void SerializationConvertersAlsoRejectNullInsteadOfBecomingAnAlternateWireFormat()
    {
        Assert.Throws<NotSupportedException>(() => JsonSerializer.Serialize<GatewayOutcome?>(null));
        Assert.Throws<NotSupportedException>(() => JsonSerializer.Serialize<GatewaySecret?>(null));
        Assert.Throws<NotSupportedException>(() => JsonSerializer.Deserialize<GatewayOutcome>("null"));
        Assert.Throws<NotSupportedException>(() => JsonSerializer.Deserialize<GatewaySecret>("null"));
    }
}
