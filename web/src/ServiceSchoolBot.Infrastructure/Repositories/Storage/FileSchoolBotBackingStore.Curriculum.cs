using System.Text.Json;
using ServiceSchoolBot.Domain.Curriculum;

namespace ServiceSchoolBot.Infrastructure.Repositories.Storage;

public sealed partial class FileSchoolBotBackingStore : ICurriculumBackingStore
{
    private async Task<CurriculumStoreSnapshot> ReadCurriculumUnitUnderLockAsync(CancellationToken ct)
    {
        if (!File.Exists(CurriculaSnapshotPath))
            return CurriculumStoreSnapshot.Empty with { Sessions = await ReadLegacySessionsUnderLockAsync(ct) };
        var json = await File.ReadAllTextAsync(CurriculaSnapshotPath, ct);
        var snapshot = JsonSerializer.Deserialize<CurriculumStoreSnapshot>(json, jsonOptions)
            ?? throw new InvalidDataException("Invalid curriculum snapshot.");
        return snapshot.Sessions is null
            ? snapshot with { Sessions = await ReadLegacySessionsUnderLockAsync(ct) }
            : snapshot;
    }

    private async Task<IReadOnlyList<ServiceSchoolBot.Domain.Identity.SchoolBotSessionRecord>> ReadLegacySessionsUnderLockAsync(
        CancellationToken ct) => File.Exists(SessionsPath)
        ? JsonSerializer.Deserialize<ServiceSchoolBot.Domain.Identity.SchoolBotSessionRecord[]>(
            await File.ReadAllTextAsync(SessionsPath, ct), jsonOptions) ?? []
        : [];

    public async Task<CurriculumStoreSnapshot> LoadCurriculumUnitAsync(CancellationToken ct = default)
    {
        await fileLock.WaitAsync(ct);
        try { return await ReadCurriculumUnitUnderLockAsync(ct); }
        finally { fileLock.Release(); }
    }

    public async Task<T> MutateCurriculumUnitAsync<T>(
        Func<CurriculumStoreSnapshot, CancellationToken, Task<CurriculumStoreMutation<T>>> mutate,
        CancellationToken ct = default)
    {
        await fileLock.WaitAsync(ct);
        string? tempPath = null;
        try
        {
            var current = await ReadCurriculumUnitUnderLockAsync(ct);
            var candidate = await mutate(current, ct);
            if (!candidate.Changed) return candidate.Result;
            var json = JsonSerializer.Serialize(candidate.Snapshot, jsonOptions);
            Directory.CreateDirectory(directoryPath);
            tempPath = Path.Combine(directoryPath, $"curricula_snapshot.{Guid.NewGuid():N}.tmp");
            await File.WriteAllTextAsync(tempPath, json, ct);
            ct.ThrowIfCancellationRequested();
            File.Move(tempPath, CurriculaSnapshotPath, overwrite: true);
            return candidate.Result;
        }
        finally
        {
            try
            {
                if (tempPath is not null && File.Exists(tempPath)) File.Delete(tempPath);
            }
            finally { fileLock.Release(); }
        }
    }

    public async Task<IReadOnlyList<SchoolBotCurriculumRecord>> LoadCurriculaAsync(CancellationToken ct = default)
    {
        var snapshot = await LoadCurriculumUnitAsync(ct);
        return snapshot.Curricula ?? [];
    }

    public async Task<IReadOnlyList<SchoolBotLessonRecord>> LoadLessonsAsync(CancellationToken ct = default)
    {
        var snapshot = await LoadCurriculumUnitAsync(ct);
        return snapshot.Lessons ?? [];
    }

    public async Task<IReadOnlyList<SchoolBotMissionRecord>> LoadMissionsAsync(CancellationToken ct = default)
    {
        var snapshot = await LoadCurriculumUnitAsync(ct);
        return snapshot.Missions ?? [];
    }

    public async Task<IReadOnlyList<SchoolBotCriteriaRecord>> LoadCriteriaAsync(CancellationToken ct = default)
    {
        var snapshot = await LoadCurriculumUnitAsync(ct);
        return snapshot.Criteria ?? [];
    }

    public async Task<IReadOnlyList<SchoolBotActivityRecord>> LoadActivitiesAsync(CancellationToken ct = default)
    {
        var snapshot = await LoadCurriculumUnitAsync(ct);
        return snapshot.Activities ?? [];
    }

    public async Task<IReadOnlyList<SchoolBotAuditEventRecord>> LoadAuditEventsAsync(CancellationToken ct = default)
    {
        var snapshot = await LoadCurriculumUnitAsync(ct);
        return snapshot.AuditEvents ?? [];
    }

    public async Task CommitCurriculumUnitAsync(
        IReadOnlyList<SchoolBotCurriculumRecord> curricula,
        IReadOnlyList<SchoolBotLessonRecord> lessons,
        IReadOnlyList<SchoolBotMissionRecord> missions,
        IReadOnlyList<SchoolBotCriteriaRecord> criteria,
        IReadOnlyList<SchoolBotActivityRecord> activities,
        IReadOnlyList<SchoolBotAuditEventRecord> auditEvents,
        CancellationToken ct = default)
    {
        await MutateCurriculumUnitAsync((current, token) => Task.FromResult(
            new CurriculumStoreMutation<bool>(current with
            {
                Curricula = curricula,
                Lessons = lessons,
                Missions = missions,
                Criteria = criteria,
                Activities = activities,
                AuditEvents = auditEvents
            }, true)), ct);
    }
}
