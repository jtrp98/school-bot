using ServiceSchoolBot.Domain.Licensing;
using ServiceSchoolBot.Domain.Releases;

namespace ServiceSchoolBot.Application.Databases.Repository.Interfaces;

public interface IManagementStore
{
    IReadOnlyList<SchoolLicense> Licenses();
    SchoolLicense? FindLicenseById(string id);
    SchoolLicense? FindLicenseByKeyHash(byte[] keyHash);
    void Save(SchoolLicense license);
    IReadOnlyList<DesktopRelease> Releases();
    DesktopRelease? FindReleaseById(string id);
    bool ReleaseExists(string version, string channel, string platform);
    IReadOnlyList<DesktopRelease> PublishedReleases(string channel, string platform);
    void Save(DesktopRelease release);
}
