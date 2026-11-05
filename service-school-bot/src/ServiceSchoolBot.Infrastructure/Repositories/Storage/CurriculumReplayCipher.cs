using ServiceSchoolBot.Application.Databases.Repository.Interfaces;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ServiceSchoolBot.Domain;

namespace ServiceSchoolBot.Infrastructure.Repositories.Storage;

internal static class CurriculumReplayCipher
{
    private static readonly byte[] StubKey = RandomNumberGenerator.GetBytes(32);

    private static byte[] Key()
    {
        var configured = Environment.GetEnvironmentVariable("SCHOOL_BOT_REPLAY_KEY");
        // shortcut: ephemeral keys cover the single-process stub; production requires an external custody key.
        if (configured is null) return StubKey;
        var key = Convert.FromBase64String(configured);
        if (key.Length != 32) throw new InvalidOperationException("Invalid replay key configuration.");
        return key;
    }

    private static byte[] Context(IdempotencyCommand command) =>
        Encoding.UTF8.GetBytes(JsonSerializer.Serialize(command));

    public static (byte[] Cipher, string KeyId) Encrypt<T>(MutationResult<T> result, IdempotencyCommand command)
    {
        var key = Key();
        var plain = JsonSerializer.SerializeToUtf8Bytes(result);
        var cipher = new byte[28 + plain.Length];
        RandomNumberGenerator.Fill(cipher.AsSpan(0, 12));
        using var aes = new AesGcm(key, 16);
        aes.Encrypt(cipher.AsSpan(0, 12), plain, cipher.AsSpan(28), cipher.AsSpan(12, 16), Context(command));
        return (cipher, Convert.ToHexStringLower(SHA256.HashData(key)));
    }

    public static MutationResult<T> Decrypt<T>(CurriculumIdempotencyRecord entry, IdempotencyCommand command)
    {
        try
        {
            var key = Key();
            var cipher = entry.ResultCipher;
            if (cipher is null || cipher.Length < 28 ||
                entry.ResultKeyId != Convert.ToHexStringLower(SHA256.HashData(key)))
                throw new CryptographicException();
            var plain = new byte[cipher.Length - 28];
            using var aes = new AesGcm(key, 16);
            aes.Decrypt(cipher.AsSpan(0, 12), cipher.AsSpan(28), cipher.AsSpan(12, 16), plain, Context(command));
            return JsonSerializer.Deserialize<MutationResult<T>>(plain) ?? throw new JsonException();
        }
        catch (Exception ex) when (ex is CryptographicException or JsonException or FormatException)
        {
            throw new DomainRuleException(ErrorCodes.StateConflict, "Stored result is unavailable.");
        }
    }
}
