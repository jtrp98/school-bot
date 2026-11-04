using System.Text.Json;
using ServiceSchoolBot.Domain.Licensing;

namespace ServiceSchoolBot.Infrastructure.Repositories.Storage;

public sealed partial class FileSchoolBotBackingStore
{
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

}
