using ServiceSchoolBot.Domain.Curriculum;
using ServiceSchoolBot.Domain.Identity;
using ServiceSchoolBot.Domain.Licensing;
using ServiceSchoolBot.Domain.Projects;

namespace ServiceSchoolBot.Infrastructure.Repositories.Storage;

public interface ISchoolBotBackingStore
{
    Task<IReadOnlyList<SchoolBotLicenseRecord>> LoadLicensesAsync(CancellationToken ct = default);

    Task<IReadOnlyList<SchoolBotLicenseBindingRecord>> LoadBindingsAsync(CancellationToken ct = default);
    Task<LicenseStoreSnapshot> LoadLicenseUnitAsync(CancellationToken ct = default);

    Task<T> MutateLicenseUnitAsync<T>(
        Func<LicenseStoreSnapshot, LicenseStoreMutation<T>> mutate,
        CancellationToken ct = default);

    Task<IReadOnlyList<SchoolBotProjectRecord>> LoadProjectsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<SchoolBotProjectRevisionRecord>> LoadProjectRevisionsAsync(CancellationToken ct = default);
    Task CommitProjectAsync(
        IReadOnlyList<SchoolBotProjectRecord> projects,
        IReadOnlyList<SchoolBotProjectRevisionRecord> revisions,
        CancellationToken ct = default);

    Task<IReadOnlyList<SchoolBotSessionRecord>> LoadSessionsAsync(CancellationToken ct = default);
    Task SaveSessionsAsync(IReadOnlyList<SchoolBotSessionRecord> sessions, CancellationToken ct = default);

    Task<IReadOnlyList<SchoolBotCurriculumRecord>> LoadCurriculaAsync(CancellationToken ct = default);
    Task<IReadOnlyList<SchoolBotLessonRecord>> LoadLessonsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<SchoolBotMissionRecord>> LoadMissionsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<SchoolBotCriteriaRecord>> LoadCriteriaAsync(CancellationToken ct = default);
    Task<IReadOnlyList<SchoolBotActivityRecord>> LoadActivitiesAsync(CancellationToken ct = default);
    Task<IReadOnlyList<SchoolBotAuditEventRecord>> LoadAuditEventsAsync(CancellationToken ct = default);
    Task CommitCurriculumUnitAsync(
        IReadOnlyList<SchoolBotCurriculumRecord> curricula,
        IReadOnlyList<SchoolBotLessonRecord> lessons,
        IReadOnlyList<SchoolBotMissionRecord> missions,
        IReadOnlyList<SchoolBotCriteriaRecord> criteria,
        IReadOnlyList<SchoolBotActivityRecord> activities,
        IReadOnlyList<SchoolBotAuditEventRecord> auditEvents,
        CancellationToken ct = default);
}
