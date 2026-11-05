using ServiceSchoolBot.Application.Common.Model.Licensing;
using ServiceSchoolBot.Application.Services.Interfaces.Licensing;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ServiceSchoolBot.Domain;

namespace ServiceSchoolBot.Application.Services.Implementations.Licensing;

public static class DeviceProofVerifier
{
    public static void ValidateDeviceJwk(DeviceJwk jwk)
    {
        if (jwk is null)
            throw new DomainRuleException(ErrorCodes.InvalidInput, "Device JWK is required.");

        if (!string.Equals(jwk.Kty, "EC", StringComparison.Ordinal) ||
            !string.Equals(jwk.Crv, "P-256", StringComparison.Ordinal))
            throw new DomainRuleException(ErrorCodes.InvalidInput, "Device JWK must be EC P-256.");

        if (string.IsNullOrWhiteSpace(jwk.X) || string.IsNullOrWhiteSpace(jwk.Y))
            throw new DomainRuleException(ErrorCodes.InvalidInput, "Device JWK X and Y are required.");

        try
        {
            var xBytes = TestLicenseTokenSigner.Base64UrlDecode(jwk.X);
            var yBytes = TestLicenseTokenSigner.Base64UrlDecode(jwk.Y);
            if (xBytes.Length != 32 || yBytes.Length != 32)
                throw new DomainRuleException(ErrorCodes.InvalidInput, "Device JWK coordinates must be 32 bytes.");
        }
        catch (FormatException)
        {
            throw new DomainRuleException(ErrorCodes.InvalidInput, "Invalid base64url coordinates in Device JWK.");
        }
    }

    public static string ComputeDeviceJkt(DeviceJwk jwk)
    {
        ValidateDeviceJwk(jwk);
        // RFC 7638: lexicographical order of required members: crv, kty, x, y
        var canonical = $"{{\"crv\":\"{jwk.Crv}\",\"kty\":\"{jwk.Kty}\",\"x\":\"{jwk.X}\",\"y\":\"{jwk.Y}\"}}";
        return Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(canonical)));
    }

    public static ECDsa ImportPublicKey(DeviceJwk jwk)
    {
        ValidateDeviceJwk(jwk);
        var ecParams = new ECParameters
        {
            Curve = ECCurve.NamedCurves.nistP256,
            Q = new ECPoint
            {
                X = TestLicenseTokenSigner.Base64UrlDecode(jwk.X),
                Y = TestLicenseTokenSigner.Base64UrlDecode(jwk.Y)
            }
        };
        return ECDsa.Create(ecParams);
    }

    public static string Canonicalize(JsonElement element)
    {
        var sb = new StringBuilder();
        WriteCanonical(element, sb);
        return sb.ToString();
    }

    private static void WriteCanonical(JsonElement element, StringBuilder sb)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                sb.Append('{');
                var properties = element.EnumerateObject()
                    .OrderBy(p => p.Name, StringComparer.Ordinal)
                    .ToArray();
                for (var i = 0; i < properties.Length; i++)
                {
                    if (i > 0) sb.Append(',');
                    sb.Append(JsonSerializer.Serialize(properties[i].Name));
                    sb.Append(':');
                    WriteCanonical(properties[i].Value, sb);
                }
                sb.Append('}');
                break;
            case JsonValueKind.Array:
                sb.Append('[');
                var first = true;
                foreach (var item in element.EnumerateArray())
                {
                    if (!first) sb.Append(',');
                    first = false;
                    WriteCanonical(item, sb);
                }
                sb.Append(']');
                break;
            case JsonValueKind.String:
                sb.Append(JsonSerializer.Serialize(element.GetString()));
                break;
            case JsonValueKind.Number:
                sb.Append(element.GetRawText());
                break;
            case JsonValueKind.True:
                sb.Append("true");
                break;
            case JsonValueKind.False:
                sb.Append("false");
                break;
            case JsonValueKind.Null:
                sb.Append("null");
                break;
        }
    }

    public static string ComputeBodyHashWithoutProof(string rawJson)
    {
        using var doc = JsonDocument.Parse(rawJson);
        if (doc.RootElement.ValueKind != JsonValueKind.Object)
            throw new DomainRuleException(ErrorCodes.InvalidInput, "Request body must be a JSON object.");

        var sb = new StringBuilder();
        sb.Append('{');
        var properties = doc.RootElement.EnumerateObject()
            .Where(p => !string.Equals(p.Name, "proof", StringComparison.OrdinalIgnoreCase))
            .OrderBy(p => p.Name, StringComparer.Ordinal)
            .ToArray();

        for (var i = 0; i < properties.Length; i++)
        {
            if (i > 0) sb.Append(',');
            sb.Append(JsonSerializer.Serialize(properties[i].Name));
            sb.Append(':');
            WriteCanonical(properties[i].Value, sb);
        }
        sb.Append('}');

        return Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(sb.ToString())));
    }

    public static void VerifyDeviceProof(
        string proofToken,
        DeviceJwk jwk,
        string expectedChallenge,
        string expectedMethod,
        string expectedPath,
        string? expectedIdempotencyKey,
        string expectedBodyHash)
    {
        if (string.IsNullOrWhiteSpace(proofToken))
            throw new DomainRuleException(ErrorCodes.DeviceProofInvalid, "Device proof token is missing.");

        var parts = proofToken.Split('.');
        if (parts.Length != 3)
            throw new DomainRuleException(ErrorCodes.DeviceProofInvalid, "Malformed device proof token.");

        using var deviceEcdsa = ImportPublicKey(jwk);
        byte[] sigBytes;
        try
        {
            sigBytes = TestLicenseTokenSigner.Base64UrlDecode(parts[2]);
        }
        catch
        {
            throw new DomainRuleException(ErrorCodes.DeviceProofInvalid, "Invalid proof signature encoding.");
        }

        var dataToVerify = Encoding.UTF8.GetBytes($"{parts[0]}.{parts[1]}");
        if (!deviceEcdsa.VerifyData(dataToVerify, sigBytes, HashAlgorithmName.SHA256, DSASignatureFormat.IeeeP1363FixedFieldConcatenation))
            throw new DomainRuleException(ErrorCodes.DeviceProofInvalid, "Device proof signature verification failed.");

        JsonDocument payloadDoc;
        try
        {
            var payloadBytes = TestLicenseTokenSigner.Base64UrlDecode(parts[1]);
            payloadDoc = JsonDocument.Parse(payloadBytes);
        }
        catch
        {
            throw new DomainRuleException(ErrorCodes.DeviceProofInvalid, "Invalid proof payload JSON.");
        }

        using (payloadDoc)
        {
            var root = payloadDoc.RootElement;
            if (!root.TryGetProperty("challenge", out var chalElem) ||
                !string.Equals(chalElem.GetString(), expectedChallenge, StringComparison.Ordinal))
            {
                throw new DomainRuleException(ErrorCodes.DeviceProofInvalid, "Proof challenge mismatch.");
            }

            if (!root.TryGetProperty("method", out var methodElem) ||
                !string.Equals(methodElem.GetString(), expectedMethod, StringComparison.OrdinalIgnoreCase))
            {
                throw new DomainRuleException(ErrorCodes.DeviceProofInvalid, "Proof HTTP method mismatch.");
            }

            if (!root.TryGetProperty("path", out var pathElem) ||
                !string.Equals(pathElem.GetString(), expectedPath, StringComparison.OrdinalIgnoreCase))
            {
                throw new DomainRuleException(ErrorCodes.DeviceProofInvalid, "Proof HTTP path mismatch.");
            }

            if (!string.IsNullOrWhiteSpace(expectedIdempotencyKey))
            {
                if (!root.TryGetProperty("idempotency_key", out var idempElem) ||
                    !string.Equals(idempElem.GetString(), expectedIdempotencyKey, StringComparison.OrdinalIgnoreCase))
                {
                    throw new DomainRuleException(ErrorCodes.DeviceProofInvalid, "Proof Idempotency-Key mismatch.");
                }
            }

            if (!root.TryGetProperty("body_hash", out var hashElem) ||
                !string.Equals(hashElem.GetString(), expectedBodyHash, StringComparison.OrdinalIgnoreCase))
            {
                throw new DomainRuleException(ErrorCodes.DeviceProofInvalid, "Proof body hash mismatch.");
            }
        }
    }

    public static void VerifyServerChallenge(
        string challengeToken,
        ILicenseTokenSigner signer,
        DateTimeOffset now,
        Func<string, bool> tryConsumeNonce,
        string expectedDeviceJkt)
    {
        if (string.IsNullOrWhiteSpace(challengeToken))
            throw new DomainRuleException(ErrorCodes.DeviceProofInvalid, "Challenge token is missing.");

        if (!signer.Verify(challengeToken, out _, out var payloadDoc) || payloadDoc is null)
            throw new DomainRuleException(ErrorCodes.DeviceProofInvalid, "Challenge token signature verification failed.");

        using (payloadDoc)
        {
            var root = payloadDoc.RootElement;

            if (!root.TryGetProperty("exp", out var expElem) || expElem.GetInt64() <= now.ToUnixTimeSeconds())
                throw new DomainRuleException(ErrorCodes.DeviceProofInvalid, "Challenge token has expired.");

            if (!root.TryGetProperty("nonce", out var nonceElem) || string.IsNullOrWhiteSpace(nonceElem.GetString()))
                throw new DomainRuleException(ErrorCodes.DeviceProofInvalid, "Challenge token is missing nonce.");

            var nonce = nonceElem.GetString()!;
            if (!tryConsumeNonce(nonce))
                throw new DomainRuleException(ErrorCodes.DeviceProofInvalid, "Challenge token nonce has already been consumed.");

            if (!root.TryGetProperty("device_jkt", out var jktElem) ||
                !string.Equals(jktElem.GetString(), expectedDeviceJkt, StringComparison.OrdinalIgnoreCase))
            {
                throw new DomainRuleException(ErrorCodes.DeviceProofInvalid, "Challenge token device JKT mismatch.");
            }
        }
    }
}
