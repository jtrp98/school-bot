using ServiceSchoolBot.Domain.Curriculum;
using ServiceSchoolBot.Domain.Identity;
using ServiceSchoolBot.Domain.Licensing;
using ServiceSchoolBot.Domain.Projects;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ServiceSchoolBot.Infrastructure.Repositories.Storage;

public sealed class FileSchoolBotBackingStore(string directoryPath) : ISchoolBotBackingStore, ICurriculumBackingStore, IWebSessionBackingStore
{
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> DirectoryLocks = new(StringComparer.OrdinalIgnoreCase);
    private SemaphoreSlim fileLock => DirectoryLocks.GetOrAdd(Path.TrimEndingDirectorySeparator(Path.GetFullPath(directoryPath)), _ => new SemaphoreSlim(1, 1));
    private readonly JsonSerializerOptions jsonOptions = new() { WriteIndented = false };

    private string LicensePath => Path.Combine(directoryPath, "licenses.json");
    private string BindingsPath => Path.Combine(directoryPath, "bindings.json");
    private string ProjectsSnapshotPath => Path.Combine(directoryPath, "projects_snapshot.json");
    private string SessionsPath => Path.Combine(directoryPath, "sessions.json");
    private string CurriculaSnapshotPath => Path.Combine(directoryPath, "curricula_snapshot.json");

    private sealed record ProjectSnapshot(
        [property: JsonPropertyName("format")] int Format,
        [property: JsonPropertyName("projects")] IReadOnlyList<SchoolBotProjectRecord> Projects,
        [property: JsonPropertyName("revisions")] IReadOnlyList<SchoolBotProjectRevisionRecord> Revisions);

    private async Task<IReadOnlyList<T>> ReadFileAsync<T>(string filePath, CancellationToken ct)
    {
        await fileLock.WaitAsync(ct);
        try
        {
            if (!File.Exists(filePath)) return [];
            var json = await File.ReadAllTextAsync(filePath, ct);
            return string.IsNullOrWhiteSpace(json) ? [] : JsonSerializer.Deserialize<List<T>>(json, jsonOptions) ?? [];
        }
        finally
        {
            fileLock.Release();
        }
    }

    private async Task WriteFileAsync<T>(string filePath, IReadOnlyList<T> items, CancellationToken ct)
    {
        await fileLock.WaitAsync(ct);
        try
        {
            Directory.CreateDirectory(directoryPath);
            var json = JsonSerializer.Serialize(items, jsonOptions);
            var tempPath = Path.Combine(directoryPath, $"{Path.GetFileName(filePath)}.{Guid.NewGuid():N}.tmp");
            await File.WriteAllTextAsync(tempPath, json, ct);
            File.Move(tempPath, filePath, overwrite: true);
        }
        finally
        {
            fileLock.Release();
        }
    }

    private async Task<ProjectSnapshot> ReadSnapshotAsync(CancellationToken ct)
    {
        await fileLock.WaitAsync(ct);
        try
        {
            if (!File.Exists(ProjectsSnapshotPath)) return new ProjectSnapshot(1, [], []);
            var json = await File.ReadAllTextAsync(ProjectsSnapshotPath, ct);
            return string.IsNullOrWhiteSpace(json) ? new ProjectSnapshot(1, [], []) : JsonSerializer.Deserialize<ProjectSnapshot>(json, jsonOptions) ?? new ProjectSnapshot(1, [], []);
        }
        finally
        {
            fileLock.Release();
        }
    }

    public async Task<IReadOnlyList<SchoolBotProjectRecord>> LoadProjectsAsync(CancellationToken ct = default)
    {
        var snapshot = await ReadSnapshotAsync(ct);
        return snapshot.Projects ?? [];
    }

    public async Task<IReadOnlyList<SchoolBotProjectRevisionRecord>> LoadProjectRevisionsAsync(CancellationToken ct = default)
    {
        var snapshot = await ReadSnapshotAsync(ct);
        return snapshot.Revisions ?? [];
    }

    public async Task CommitProjectAsync(
        IReadOnlyList<SchoolBotProjectRecord> projects,
        IReadOnlyList<SchoolBotProjectRevisionRecord> revisions,
        CancellationToken ct = default)
    {
        await fileLock.WaitAsync(ct);
        string? tempPath = null;
        try
        {
            Directory.CreateDirectory(directoryPath);
            var candidateSnapshot = new ProjectSnapshot(1, projects, revisions);
            var json = JsonSerializer.Serialize(candidateSnapshot, jsonOptions);
            tempPath = Path.Combine(directoryPath, $"projects_snapshot.{Guid.NewGuid():N}.tmp");

            await using (var stream = new FileStream(tempPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 4096, useAsync: true))
            await using (var writer = new StreamWriter(stream, Encoding.UTF8))
            {
                await writer.WriteAsync(json);
                await writer.FlushAsync(ct);
                await stream.FlushAsync(ct);
            }

            File.Move(tempPath, ProjectsSnapshotPath, overwrite: true);
        }
        finally
        {
            if (tempPath is not null && File.Exists(tempPath))
            {
                try { File.Delete(tempPath); } catch { }
            }
            fileLock.Release();
        }
    }

    public async Task<IReadOnlyList<SchoolBotSessionRecord>> LoadSessionsAsync(CancellationToken ct = default) =>
        (await LoadCurriculumUnitAsync(ct)).Sessions ?? [];

    public Task SaveSessionsAsync(IReadOnlyList<SchoolBotSessionRecord> items, CancellationToken ct = default) =>
        MutateCurriculumUnitAsync((snapshot, token) => Task.FromResult(
            new CurriculumStoreMutation<bool>(snapshot with { Sessions = items.ToArray() }, true)), ct);

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
            await ReplaceSnapshotAsync(tempPath, CurriculaSnapshotPath, ct);
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

    private string LicenseSnapshotPath => Path.Combine(directoryPath, "licenses_snapshot.json");

    private async Task<LicenseStoreSnapshot> ReadLicenseUnitUnderLockAsync(CancellationToken ct)
    {
        if (File.Exists(LicenseSnapshotPath))
        {
            var json = await File.ReadAllTextAsync(LicenseSnapshotPath, ct);
            return JsonSerializer.Deserialize<LicenseStoreSnapshot>(json, jsonOptions)
                ?? throw new InvalidDataException("Invalid license snapshot.");
        }

        // Existing installations migrate both legacy files at the first successful publication.
        var licenses = File.Exists(LicensePath)
            ? JsonSerializer.Deserialize<SchoolBotLicenseRecord[]>(await File.ReadAllTextAsync(LicensePath, ct), jsonOptions) ?? []
            : [];
        var bindings = File.Exists(BindingsPath)
            ? JsonSerializer.Deserialize<SchoolBotLicenseBindingRecord[]>(await File.ReadAllTextAsync(BindingsPath, ct), jsonOptions) ?? []
            : [];
        return new LicenseStoreSnapshot(licenses, bindings);
    }

    public async Task<LicenseStoreSnapshot> LoadLicenseUnitAsync(CancellationToken ct = default)
    {
        await fileLock.WaitAsync(ct);
        try { return await ReadLicenseUnitUnderLockAsync(ct); }
        finally { fileLock.Release(); }
    }

    public async Task<T> MutateLicenseUnitAsync<T>(
        Func<LicenseStoreSnapshot, LicenseStoreMutation<T>> mutate, CancellationToken ct = default)
    {
        await fileLock.WaitAsync(ct);
        string? tempPath = null;
        try
        {
            var previous = await ReadLicenseUnitUnderLockAsync(ct);
            var candidate = mutate(previous);
            candidate.Snapshot.ValidateMutation(previous);
            Directory.CreateDirectory(directoryPath);
            tempPath = Path.Combine(directoryPath, $"licenses_snapshot.{Guid.NewGuid():N}.tmp");
            await using (var stream = new FileStream(tempPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                await JsonSerializer.SerializeAsync(stream, candidate.Snapshot, jsonOptions, ct);
                await stream.FlushAsync(ct);
                stream.Flush(flushToDisk: true);
            }
            ct.ThrowIfCancellationRequested();
            File.Move(tempPath, LicenseSnapshotPath, overwrite: true);
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

    public async Task<IReadOnlyList<SchoolBotLicenseRecord>> LoadLicensesAsync(CancellationToken ct = default) =>
        (await LoadLicenseUnitAsync(ct)).Licenses;

    public async Task<IReadOnlyList<SchoolBotLicenseBindingRecord>> LoadBindingsAsync(CancellationToken ct = default) =>
        (await LoadLicenseUnitAsync(ct)).Bindings;

    // Windows indexers can briefly deny atomic replacement even after our own readers close.
    private static async Task ReplaceSnapshotAsync(string source, string destination, CancellationToken ct)
    {
        for (var retry = 0; ; retry++)
        {
            try { File.Move(source, destination, overwrite: true); return; }
            catch (Exception ex) when (OperatingSystem.IsWindows() && retry < 5 &&
                ex is IOException or UnauthorizedAccessException && (ex.HResult & 0xffff) is 5 or 32)
            { await Task.Delay(TimeSpan.FromMilliseconds(20 * (1 << retry)), ct); }
        }
    }

    public Task<T> MutateWebSessionUnitAsync<T>(
        Func<CurriculumStoreSnapshot, IReadOnlyList<SchoolBotLicenseRecord>, CurriculumStoreMutation<T>> mutate,
        CancellationToken ct) =>
        MutateCurriculumUnitAsync(async (snapshot, token) =>
            mutate(snapshot, (await ReadLicenseUnitUnderLockAsync(token)).Licenses), ct);
}
