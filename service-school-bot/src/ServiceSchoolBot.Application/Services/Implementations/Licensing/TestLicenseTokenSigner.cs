using ServiceSchoolBot.Application.Services.Interfaces.Licensing;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace ServiceSchoolBot.Application.Services.Implementations.Licensing;

public sealed class TestLicenseTokenSigner : ILicenseTokenSigner, IDisposable
{
    public const string TestKeyId = "test-fake-signer-047";
    private readonly ECDsa ecdsa;
    private readonly bool ownsEcdsa;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    public TestLicenseTokenSigner(ECDsa? ecdsa = null)
    {
        if (ecdsa is not null)
        {
            this.ecdsa = ecdsa;
            ownsEcdsa = false;
        }
        else
        {
            this.ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
            ownsEcdsa = true;
        }
    }

    public string KeyId => TestKeyId;

    public string Sign(string typ, object payload)
    {
        var headerObj = new
        {
            typ,
            alg = "ES256",
            kid = KeyId
        };
        var headerJson = JsonSerializer.Serialize(headerObj, JsonOptions);
        var payloadJson = payload is string s ? s : JsonSerializer.Serialize(payload, JsonOptions);

        var headerB64 = Base64UrlEncode(Encoding.UTF8.GetBytes(headerJson));
        var payloadB64 = Base64UrlEncode(Encoding.UTF8.GetBytes(payloadJson));
        var dataToSign = Encoding.UTF8.GetBytes($"{headerB64}.{payloadB64}");

        var sigBytes = ecdsa.SignData(dataToSign, HashAlgorithmName.SHA256, DSASignatureFormat.IeeeP1363FixedFieldConcatenation);
        var sigB64 = Base64UrlEncode(sigBytes);

        return $"{headerB64}.{payloadB64}.{sigB64}";
    }

    public bool Verify(string token, out JsonDocument? header, out JsonDocument? payload)
    {
        header = null;
        payload = null;

        if (string.IsNullOrWhiteSpace(token)) return false;
        var parts = token.Split('.');
        if (parts.Length != 3) return false;

        try
        {
            var headerBytes = Base64UrlDecode(parts[0]);
            var payloadBytes = Base64UrlDecode(parts[1]);
            var sigBytes = Base64UrlDecode(parts[2]);

            var headerDoc = JsonDocument.Parse(headerBytes);
            if (!headerDoc.RootElement.TryGetProperty("alg", out var algElem) ||
                !string.Equals(algElem.GetString(), "ES256", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (!headerDoc.RootElement.TryGetProperty("kid", out var kidElem) ||
                !string.Equals(kidElem.GetString(), KeyId, StringComparison.Ordinal))
            {
                return false;
            }

            var dataToVerify = Encoding.UTF8.GetBytes($"{parts[0]}.{parts[1]}");
            if (!ecdsa.VerifyData(dataToVerify, sigBytes, HashAlgorithmName.SHA256, DSASignatureFormat.IeeeP1363FixedFieldConcatenation))
            {
                return false;
            }

            header = headerDoc;
            payload = JsonDocument.Parse(payloadBytes);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public void Dispose()
    {
        if (ownsEcdsa) ecdsa.Dispose();
    }

    public static string Base64UrlEncode(byte[] input) =>
        Convert.ToBase64String(input)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');

    public static byte[] Base64UrlDecode(string input)
    {
        var output = input.Replace('-', '+').Replace('_', '/');
        switch (output.Length % 4)
        {
            case 2: output += "=="; break;
            case 3: output += "="; break;
        }
        return Convert.FromBase64String(output);
    }
}
