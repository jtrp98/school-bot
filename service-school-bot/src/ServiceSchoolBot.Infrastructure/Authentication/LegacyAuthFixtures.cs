using System.Security.Cryptography;
using System.Text;

namespace ServiceSchoolBot.Infrastructure.Authentication;

public static class LegacyAuthFixtures
{
    public const int PrimaryTenantId = 501;
    public const int SecondaryTenantId = 502;

    public const int TeacherAccountId = 10001;
    public const int TeacherSystemId = 90001;
    public const int TeacherProfileId = 20001;

    public const int StudentAccountId = 10002;
    public const int StudentSystemId = 90002;
    public const int StudentProfileId = 30001;

    public const string SyntheticRawPassword = "SyntheticPass123!";
    public const string SyntheticDatePassword = "15082005";
    public const string ModernPbkdf2Hash = "v2:c3ludGgtc2FsdC0wMDAxIQ==:S7n9Vv/BuyLw9mKlAf0UPNgUjYH7ycxvhFY1cxUzHtM=";

    public static string ComputedSha1Uppercase =>
        Convert.ToHexString(SHA1.HashData(Encoding.UTF8.GetBytes(SyntheticRawPassword)));

    public static LegacyAccountRecord CreateValidTeacherAccount() => new(
        AccountId: TeacherAccountId,
        SystemId: TeacherSystemId,
        SchoolId: PrimaryTenantId,
        Username: "synth.teacher01",
        PasswordHash: ComputedSha1Uppercase,
        PlainPassword: null,
        UseEncryptPassword: true,
        CType: '1',
        CDel: null,
        AccountLocked: false,
        AccountLockedUntil: null);

    public static LegacyProfileRecord CreateValidTeacherProfile() => new(
        ProfileId: TeacherProfileId,
        SchoolId: PrimaryTenantId,
        CType: '1',
        WorkStatus: 1,
        CDel: null,
        IsActive: true);

    public static LegacyAccountRecord CreateValidStudentAccount() => new(
        AccountId: StudentAccountId,
        SystemId: StudentSystemId,
        SchoolId: PrimaryTenantId,
        Username: "synth.student01",
        PasswordHash: ModernPbkdf2Hash,
        PlainPassword: null,
        UseEncryptPassword: true,
        CType: '0',
        CDel: null,
        AccountLocked: false,
        AccountLockedUntil: null);

    public static LegacyProfileRecord CreateValidStudentProfile() => new(
        ProfileId: StudentProfileId,
        SchoolId: PrimaryTenantId,
        CType: '0',
        WorkStatus: null,
        CDel: null,
        IsActive: true);

    public static (LegacyAccountRecord Account, LegacyProfileRecord Profile) CreateCrossTenantPair() => (
        CreateValidTeacherAccount(),
        new LegacyProfileRecord(
            ProfileId: 40001,
            SchoolId: SecondaryTenantId,
            CType: '1',
            WorkStatus: 1,
            CDel: null,
            IsActive: true));

    public static LegacyAccountRecord CreateDeletedAccount(char cDelValue = '1') => new(
        AccountId: 10003,
        SystemId: 90003,
        SchoolId: PrimaryTenantId,
        Username: "synth.deleted",
        PasswordHash: ComputedSha1Uppercase,
        PlainPassword: null,
        UseEncryptPassword: true,
        CType: '1',
        CDel: cDelValue,
        AccountLocked: false,
        AccountLockedUntil: null);

    public static LegacyProfileRecord CreateTerminatedEmployeeProfile(int workStatus = 2) => new(
        ProfileId: 20002,
        SchoolId: PrimaryTenantId,
        CType: '1',
        WorkStatus: workStatus,
        CDel: null,
        IsActive: true);

    public static LegacyAccountRecord CreateLockedAccount() => new(
        AccountId: 10004,
        SystemId: 90004,
        SchoolId: PrimaryTenantId,
        Username: "synth.locked",
        PasswordHash: ComputedSha1Uppercase,
        PlainPassword: null,
        UseEncryptPassword: true,
        CType: '1',
        CDel: null,
        AccountLocked: true,
        AccountLockedUntil: DateTimeOffset.UtcNow.AddHours(2));

    public static (LegacyAccountRecord Older, LegacyAccountRecord Newer) CreateDuplicateAccounts()
    {
        const string duplicateUsername = "synth.duplicate";
        var older = new LegacyAccountRecord(
            AccountId: 10010,
            SystemId: 90010,
            SchoolId: PrimaryTenantId,
            Username: duplicateUsername,
            PasswordHash: ComputedSha1Uppercase,
            PlainPassword: null,
            UseEncryptPassword: true,
            CType: '1',
            CDel: null,
            AccountLocked: false,
            AccountLockedUntil: null);

        var newer = new LegacyAccountRecord(
            AccountId: 10020,
            SystemId: 90020,
            SchoolId: PrimaryTenantId,
            Username: duplicateUsername,
            PasswordHash: ComputedSha1Uppercase,
            PlainPassword: null,
            UseEncryptPassword: true,
            CType: '1',
            CDel: null,
            AccountLocked: false,
            AccountLockedUntil: null);

        return (older, newer);
    }
}
