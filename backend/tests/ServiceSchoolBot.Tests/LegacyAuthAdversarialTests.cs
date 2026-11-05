using System;
using System.Security.Cryptography;
using System.Text;
using ServiceSchoolBot.Infrastructure.Authentication;
using Compatibility = ServiceSchoolBot.Infrastructure.Authentication.LegacyAuthCompatibility;

namespace ServiceSchoolBot.Tests;

public sealed class LegacyAuthAdversarialTests
{
    private const string Sha1Abc = "A9993E364706816ABA3E25717850C26C9CD0D89D";
    private static LegacyAccountRecord Account() => LegacyAuthFixtures.CreateValidTeacherAccount();
    private static LegacyProfileRecord Profile() => LegacyAuthFixtures.CreateValidTeacherProfile();
    private static bool Link(LegacyAccountRecord account, LegacyProfileRecord profile) =>
        account.AccountId == 10001 && account.SystemId == 90001 &&
        profile.ProfileId == 20001 && account.SchoolId == 501 && profile.SchoolId == 501;
    private static LegacyValidationResult Bind(
        LegacyAccountRecord? account, LegacyProfileRecord? profile) =>
        Compatibility.ValidateBinding(account, profile, 501,
            authoritativeLinkVerifier: Link);
    private static string Sha1(string value) =>
        Convert.ToHexString(SHA1.HashData(Encoding.UTF8.GetBytes(value)));
    private static bool FixtureHashMatches()
    {
        var parts = LegacyAuthFixtures.ModernPbkdf2Hash.Split(':');
        var derived = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(LegacyAuthFixtures.SyntheticRawPassword),
            Convert.FromBase64String(parts[1]), 100000, HashAlgorithmName.SHA256, 32);
        return CryptographicOperations.FixedTimeEquals(derived, Convert.FromBase64String(parts[2]));
    }

    [Theory]
    [InlineData(1, "SHA1 ยอมรับ public vector abc", true)]
    [InlineData(2, "SHA1 ปฏิเสธรหัสผิด", false)]
    [InlineData(3, "SHA1 ปฏิเสธ null", false)]
    [InlineData(4, "SHA1 ปฏิเสธค่าว่าง", false)]
    [InlineData(5, "SHA1 รับขอบเขต 1024", true)]
    [InlineData(6, "SHA1 ปฏิเสธ 1025 แม้ hash ตรง", false)]
    [InlineData(7, "SHA1 ไม่ trim password", false)]
    [InlineData(8, "SHA1 ปฏิเสธ hash ที่ไม่ใช่ hex", false)]
    [InlineData(9, "Web observation เทียบ hex แบบ Ordinal", false)]
    [InlineData(10, "Mobile observation เทียบ hex แบบ insensitive เมื่อระบุ", true)]
    [InlineData(11, "PBKDF2 fixture ตรงกับ synthetic password ที่ประกาศ", true)]
    [InlineData(12, "ปฏิเสธ v2 ที่ Base64 ผิดรูป", true)]
    [InlineData(13, "Plaintext ไม่ trim password", false)]
    [InlineData(14, "Date candidate ปฏิเสธ 1025 แม้ plaintext ตรง", false)]
    [InlineData(15, "Binding ปฏิเสธ category ของ account และ profile ไม่ตรงกัน", false)]
    [InlineData(16, "cType 0 เป็น Student", true)]
    [InlineData(17, "cType 1 เป็น Employee", true)]
    [InlineData(18, "cType null ถูกปฏิเสธ", false)]
    [InlineData(19, "cType ไม่รู้จักถูกปฏิเสธ", false)]
    [InlineData(20, "Selector ปฏิเสธ candidates null", false)]
    [InlineData(21, "Selector รับ active account เดียว", true)]
    [InlineData(22, "Selector ปฏิเสธ active ซ้ำประเภทเดียวกัน", false)]
    [InlineData(23, "Selector ปฏิเสธ active ซ้ำคนละประเภท", false)]
    [InlineData(24, "Selector ปฏิเสธ active ที่ปน category ไม่รู้จัก", false)]
    [InlineData(25, "Selector ปฏิเสธบัญชีที่ลบทั้งหมด", false)]
    [InlineData(26, "Selector ปฏิเสธบัญชีที่ล็อกถึงอนาคต", false)]
    [InlineData(27, "Selector ไม่เลือกบัญชีข้าม tenant", false)]
    [InlineData(28, "Selector ปฏิเสธ active ที่ปน category null", false)]
    [InlineData(29, "Binding ปฏิเสธ profile null", false)]
    [InlineData(30, "Binding ปฏิเสธ account null", false)]
    [InlineData(31, "Binding ปฏิเสธ tenant ศูนย์", false)]
    [InlineData(32, "Binding ปฏิเสธ account ข้าม tenant", false)]
    [InlineData(33, "Binding ปฏิเสธ profile ข้าม tenant", false)]
    [InlineData(34, "Binding ปฏิเสธบัญชีที่ลบ", false)]
    [InlineData(35, "Binding ปฏิเสธ profile ที่ลบ", false)]
    [InlineData(36, "Binding ปฏิเสธ inactive profile", false)]
    [InlineData(37, "Binding ปฏิเสธ AccountLocked", false)]
    [InlineData(38, "Binding ปฏิเสธ WorkStatus 2", false)]
    [InlineData(39, "Binding ปฏิเสธ WorkStatus 3", false)]
    [InlineData(40, "Binding ไม่ถือ WorkStatus null ว่า active", false)]
    [InlineData(41, "Binding ปฏิเสธ SystemId null", false)]
    [InlineData(42, "Binding ปฏิเสธ SystemId ศูนย์", false)]
    [InlineData(43, "ID เท่ากันไม่ทดแทน authoritative linkage", false)]
    [InlineData(44, "Boolean assertion ไม่สร้าง admin", false)]
    [InlineData(45, "Trusted admin lookup ให้สิทธิ์แม้ assertion false", true)]
    public void MeetsContract(int caseNumber, string behavior, bool expected)
    {
        bool actual = Evaluate(caseNumber);
        Assert.True(actual == expected, $"{behavior}: expected={expected}, actual={actual}");
    }

    private static bool Evaluate(int caseNumber) => caseNumber switch
    {
        1 => Compatibility.VerifySha1("abc", Sha1Abc),
        2 => Compatibility.VerifySha1("abd", Sha1Abc),
        3 => Compatibility.VerifySha1(null, Sha1Abc),
        4 => Compatibility.VerifySha1("", Sha1Abc),
        5 => Compatibility.VerifySha1(new string('x', 1024), Sha1(new string('x', 1024))),
        6 => Compatibility.VerifySha1(new string('x', 1025), Sha1(new string('x', 1025))),
        7 => Compatibility.VerifySha1(" abc ", Sha1Abc),
        8 => Compatibility.VerifySha1("abc", new string('Z', 40)),
        9 => Compatibility.VerifySha1("abc", Sha1Abc.ToLowerInvariant()),
        10 => Compatibility.VerifySha1("abc", Sha1Abc.ToLowerInvariant(), false),
        11 => FixtureHashMatches(),
        12 => Compatibility.DetectPasswordFormat("v2:!:!") == LegacyPasswordFormat.Unknown,
        13 => Compatibility.VerifyDateCandidate(" abc ", "abc", Array.Empty<string>()),
        14 => Compatibility.VerifyDateCandidate(new string('x', 1025), new string('x', 1025)),
        15 => Compatibility.ValidateBinding(
            Account(), Profile() with { CType = '0' }, 501,
            authoritativeLinkVerifier: (_, _) => true).IsValid,
        16 => Compatibility.TryParseCategory('0', out var studentCategory) &&
            studentCategory == LegacyUserCategory.Student,
        17 => Compatibility.TryParseCategory('1', out var employeeCategory) &&
            employeeCategory == LegacyUserCategory.Employee,
        18 => Compatibility.TryParseCategory(null, out _),
        19 => Compatibility.TryParseCategory('9', out _),
        20 => Compatibility.ResolveDuplicateAccount(null, 501) != null,
        21 => Compatibility.ResolveDuplicateAccount(new[] { Account() }, 501)?.AccountId == 10001,
        22 => Compatibility.ResolveDuplicateAccount(
            new[] { Account(), Account() with { AccountId = 10002 } }, 501) != null,
        23 => Compatibility.ResolveDuplicateAccount(
            new[] { Account(), Account() with { AccountId = 10002, CType = '0' } }, 501) != null,
        24 => Compatibility.ResolveDuplicateAccount(
            new[] { Account(), Account() with { AccountId = 10002, CType = '9' } }, 501) != null,
        25 => Compatibility.ResolveDuplicateAccount(new[] { Account() with { CDel = '1' } }, 501) != null,
        26 => Compatibility.ResolveDuplicateAccount(
            new[] { Account() with { AccountLockedUntil = DateTimeOffset.UtcNow.AddDays(1) } }, 501) != null,
        27 => Compatibility.ResolveDuplicateAccount(
            new[] { Account() with { SchoolId = 502 } }, 501) != null,
        28 => Compatibility.ResolveDuplicateAccount(
            new[] { Account(), Account() with { AccountId = 10002, CType = null } }, 501) != null,
        29 => Bind(Account(), null).IsValid,
        30 => Bind(null, Profile()).IsValid,
        31 => Compatibility.ValidateBinding(Account(), Profile(), 0, authoritativeLinkVerifier: Link).IsValid,
        32 => Bind(Account() with { SchoolId = 502 }, Profile()).IsValid,
        33 => Bind(Account(), Profile() with { SchoolId = 502 }).IsValid,
        34 => Bind(Account() with { CDel = '1' }, Profile()).IsValid,
        35 => Bind(Account(), Profile() with { CDel = '1' }).IsValid,
        36 => Bind(Account(), Profile() with { IsActive = false }).IsValid,
        37 => Bind(Account() with { AccountLocked = true }, Profile()).IsValid,
        38 => Bind(Account(), Profile() with { WorkStatus = 2 }).IsValid,
        39 => Bind(Account(), Profile() with { WorkStatus = 3 }).IsValid,
        40 => Bind(Account(), Profile() with { WorkStatus = null }).IsValid,
        41 => Bind(Account() with { SystemId = null }, Profile()).IsValid,
        42 => Bind(Account() with { SystemId = 0 }, Profile()).IsValid,
        43 => Compatibility.ValidateBinding(Account(), Profile() with { ProfileId = 10001 }, 501).IsValid,
        44 =>
            Compatibility.ValidateBinding(
                Account(), Profile(),
                501,
                serverAdminVerified: true,
                authoritativeLinkVerifier: Link).Binding?.IsAdmin == true,
        45 =>
            Compatibility.ValidateBinding(
                Account(), Profile(),
                501,
                serverAdminVerified: false,
                authoritativeLinkVerifier: Link,
                authoritativeAdminVerifier: Link).Binding?.IsAdmin == true,
        _ => throw new ArgumentOutOfRangeException(nameof(caseNumber))
    };
}
