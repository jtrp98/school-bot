using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using ServiceSchoolBot.Domain.Identity;
using ServiceSchoolBot.Domain.Licensing;
using ServiceSchoolBot.Domain.Projects;

namespace ServiceSchoolBot.Infrastructure.Repositories.Storage;

public sealed partial class FileSchoolBotBackingStore(string directoryPath) : ISchoolBotBackingStore
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
}
