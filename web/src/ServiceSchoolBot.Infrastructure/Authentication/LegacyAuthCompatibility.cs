using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace ServiceSchoolBot.Infrastructure.Authentication;

public enum LegacyClientChannel { Web, Mobile }
public enum LegacyUserCategory { Student, Employee }
public enum LegacyPasswordFormat { Sha1Hex, ModernPbkdf2, DateCandidate, Plaintext, Unknown }

public sealed record LegacyAccountRecord(
    int AccountId,
    int? SystemId,
    int SchoolId,
    string Username,
    string? PasswordHash,
    string? PlainPassword,
    bool UseEncryptPassword,
    char? CType,
    char? CDel,
    bool AccountLocked,
    DateTimeOffset? AccountLockedUntil);

public sealed record LegacyProfileRecord(
    int ProfileId,
    int SchoolId,
    char CType,
    int? WorkStatus,
    char? CDel,
    bool IsActive);

public sealed record LegacyDirectoryBinding(
    int AccountId,
    int ProfileId,
    int SchoolId,
    LegacyUserCategory Category,
    bool IsAdmin);

public sealed record LegacyValidationResult(
    bool IsValid,
    string? ErrorCode,
    LegacyDirectoryBinding? Binding = null);

public static class LegacyAuthCompatibility
{
    private static readonly string[] DefaultDateCultures = ["en-US", "th-TH"];

    public static string NormalizeUsername(string? username, LegacyClientChannel channel)
    {
        if (string.IsNullOrEmpty(username)) return string.Empty;
        return channel switch
        {
            LegacyClientChannel.Web => username.Trim(),
            LegacyClientChannel.Mobile => username,
            _ => username.Trim()
        };
    }

    public static bool TryParseCategory(char? cType, out LegacyUserCategory category)
    {
        category = default;
        switch (cType)
        {
            case '0':
                category = LegacyUserCategory.Student;
                return true;
            case '1':
                category = LegacyUserCategory.Employee;
                return true;
            default:
                return false;
        }
    }

    public static bool IsAccountDeleted(char? cDel, LegacyClientChannel channel)
    {
        return channel switch
        {
            LegacyClientChannel.Web => cDel is not null,
            LegacyClientChannel.Mobile => cDel == '1',
            _ => true
        };
    }

    public static bool IsEmployeeActive(int? workStatus) => workStatus.HasValue && workStatus.Value is not (2 or 3);

    public static LegacyPasswordFormat DetectPasswordFormat(string? storedHash)
    {
        if (string.IsNullOrEmpty(storedHash)) return LegacyPasswordFormat.Unknown;
        if (storedHash.StartsWith("v2:", StringComparison.Ordinal))
        {
            var parts = storedHash.Split(':');
            if (parts.Length == 3 &&
                TryDecodeBase64(parts[1], out var salt) && salt.Length > 0 &&
                TryDecodeBase64(parts[2], out var hash) && hash.Length == 32)
            {
                return LegacyPasswordFormat.ModernPbkdf2;
            }
            return LegacyPasswordFormat.Unknown;
        }
        if (storedHash.Length == 40 && IsHexString(storedHash)) return LegacyPasswordFormat.Sha1Hex;
        return LegacyPasswordFormat.Unknown;
    }

    public static bool VerifySha1(string? inputPassword, string? storedHash, bool ordinalOnly = true)
    {
        if (string.IsNullOrEmpty(inputPassword) || inputPassword.Length > 1024) return false;
        if (string.IsNullOrEmpty(storedHash) || storedHash.Length != 40 || !IsHexString(storedHash)) return false;

        var computed = Convert.ToHexString(SHA1.HashData(Encoding.UTF8.GetBytes(inputPassword)));
        var comparison = ordinalOnly ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
        return string.Equals(computed, storedHash, comparison);
    }

    public static bool VerifyDateCandidate(string? inputPassword, string? storedPlain, string[]? cultures = null)
    {
        if (string.IsNullOrEmpty(inputPassword) || inputPassword.Length > 1024 || string.IsNullOrEmpty(storedPlain))
            return false;

        if (string.Equals(inputPassword, storedPlain, StringComparison.Ordinal))
            return true;

        var targetCultures = cultures ?? DefaultDateCultures;
        foreach (var cultureName in targetCultures)
        {
            CultureInfo culture;
            try
            {
                culture = CultureInfo.GetCultureInfo(cultureName);
            }
            catch (Exception ex) when (ex is CultureNotFoundException or ArgumentException)
            {
                continue;
            }

            if (DateTime.TryParseExact(inputPassword, "ddMMyyyy", culture, DateTimeStyles.None, out var parsedInCulture))
            {
                if (string.Equals(parsedInCulture.ToString("ddMMyyyy", culture), storedPlain, StringComparison.Ordinal))
                    return true;
            }

            if (string.Equals(culture.Name, "th-TH", StringComparison.OrdinalIgnoreCase))
            {
                if (DateTime.TryParseExact(inputPassword, "ddMMyyyy", CultureInfo.GetCultureInfo("en-US"), DateTimeStyles.None, out var dateEn))
                {
                    if (string.Equals(dateEn.ToString("ddMMyyyy", culture), storedPlain, StringComparison.Ordinal))
                        return true;
                }
            }
            else if (string.Equals(culture.Name, "en-US", StringComparison.OrdinalIgnoreCase))
            {
                if (DateTime.TryParseExact(inputPassword, "ddMMyyyy", CultureInfo.GetCultureInfo("th-TH"), DateTimeStyles.None, out var dateTh))
                {
                    if (string.Equals(dateTh.ToString("ddMMyyyy", culture), storedPlain, StringComparison.Ordinal))
                        return true;
                }
            }
        }

        return false;
    }

    public static LegacyAccountRecord? ResolveDuplicateAccount(
        IEnumerable<LegacyAccountRecord>? candidates, int targetSchoolId)
    {
        if (candidates == null || targetSchoolId <= 0)
            return null;

        var activeCandidates = new List<LegacyAccountRecord>();
        foreach (var c in candidates)
        {
            if (c == null) continue;
            if (c.SchoolId != targetSchoolId) continue;
            if (c.CDel == '1') continue;
            if (c.AccountLocked || (c.AccountLockedUntil.HasValue && c.AccountLockedUntil.Value > DateTimeOffset.UtcNow)) continue;
            if (!TryParseCategory(c.CType, out _)) return null;

            activeCandidates.Add(c);
        }

        if (activeCandidates.Count != 1)
        {
            return null;
        }

        return activeCandidates[0];
    }

    public static LegacyAccountRecord? SelectHighestIdPortDemonstration(
        IEnumerable<LegacyAccountRecord>? candidates, int targetSchoolId)
    {
        if (candidates == null || targetSchoolId <= 0)
            return null;

        return candidates
            .Where(a => a != null && a.SchoolId == targetSchoolId)
            .OrderByDescending(a => a.AccountId)
            .FirstOrDefault();
    }

    public static LegacyValidationResult ValidateBinding(
        LegacyAccountRecord? account,
        LegacyProfileRecord? profile,
        int targetSchoolId,
        bool serverAdminVerified = false,
        Func<LegacyAccountRecord, LegacyProfileRecord, bool>? authoritativeLinkVerifier = null,
        Func<LegacyAccountRecord, LegacyProfileRecord, bool>? authoritativeAdminVerifier = null)
    {
        if (account == null || profile == null)
            return new LegacyValidationResult(false, "INVALID_INPUT");

        if (targetSchoolId <= 0)
            return new LegacyValidationResult(false, "INVALID_TENANT");

        if (account.SchoolId != targetSchoolId || profile.SchoolId != targetSchoolId)
            return new LegacyValidationResult(false, "CROSS_TENANT_FORBIDDEN");

        if (account.CDel == '1' || profile.CDel == '1')
            return new LegacyValidationResult(false, "DELETED_ENTITY");

        if (!profile.IsActive)
            return new LegacyValidationResult(false, "PROFILE_INACTIVE");

        if (account.AccountLocked || (account.AccountLockedUntil.HasValue && account.AccountLockedUntil.Value > DateTimeOffset.UtcNow))
            return new LegacyValidationResult(false, "ACCOUNT_LOCKED");

        if (!TryParseCategory(account.CType, out var accountCategory) ||
            !TryParseCategory(profile.CType, out var profileCategory) ||
            accountCategory != profileCategory)
            return new LegacyValidationResult(false, "CATEGORY_MISMATCH");

        if (profileCategory == LegacyUserCategory.Employee && !IsEmployeeActive(profile.WorkStatus))
            return new LegacyValidationResult(false, "EMPLOYEE_TERMINATED");

        if (accountCategory == LegacyUserCategory.Employee && (!account.SystemId.HasValue || account.SystemId.Value <= 0))
            return new LegacyValidationResult(false, "AUTH_NOT_CONFIGURED");

        if (authoritativeLinkVerifier == null || !authoritativeLinkVerifier(account, profile))
        {
            return new LegacyValidationResult(false, "AUTH_NOT_CONFIGURED");
        }

        var isAdmin = authoritativeAdminVerifier != null && authoritativeAdminVerifier(account, profile);

        var binding = new LegacyDirectoryBinding(
            account.AccountId,
            profile.ProfileId,
            targetSchoolId,
            accountCategory,
            isAdmin);

        return new LegacyValidationResult(true, null, binding);
    }

    private static bool IsHexString(string s)
    {
        foreach (var c in s)
        {
            if (!char.IsAsciiHexDigit(c)) return false;
        }
        return true;
    }

    private static bool TryDecodeBase64(string s, out byte[] bytes)
    {
        try
        {
            bytes = Convert.FromBase64String(s);
            return true;
        }
        catch
        {
            bytes = [];
            return false;
        }
    }
}
