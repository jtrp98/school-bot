using ServiceSchoolBot.Domain;
using ServiceSchoolBot.Domain.Assessment;

namespace ServiceSchoolBot.Application.Common.Model.Assessment;

public interface IExportPrincipalResolver
{
    // Resolve the original confirming teacher from current authority; never replay a saved JWT or cached permission.
    Task<AssessmentPrincipalContext> ResolveAsync(ScoreExportRecord export, CancellationToken ct);
}
public sealed class UnconfiguredExportPrincipalResolver : IExportPrincipalResolver
{
    public Task<AssessmentPrincipalContext> ResolveAsync(ScoreExportRecord export, CancellationToken ct) =>
        throw new DomainRuleException(ErrorCodes.AuthNotConfigured, "Current teacher assignment resolver is not configured for background export.");
}

// Implementations must use the existing grade database, verified calculator, and a receipt in its own transaction.
// No browser-provided column names, JSON paths, row IDs, or calculated scores enter this boundary.
public interface IGradeDestination
{
    Task<IReadOnlyList<GradeSlotRecord>> DiscoverSlotsAsync(AssessmentPrincipalContext principal,
        SchoolBotCourseRunRecord run, CancellationToken ct);
    Task RecheckSlotAsync(AssessmentPrincipalContext principal, SchoolBotCourseRunRecord run,
        GradeSlotRecord slot, bool requireEmpty, CancellationToken ct);
    Task<IReadOnlyList<GradeTargetCell>> ReadCellsAsync(AssessmentPrincipalContext principal,
        SchoolBotCourseRunRecord run, IReadOnlyList<GradeSlotRecord> slots, IReadOnlyList<int> learnerIds, CancellationToken ct);
    Task RequireVerifiedWriterAsync(CancellationToken ct);
    Task<GradeApplyReceipt?> FindReceiptAsync(Guid operationKey, string payloadHash, CancellationToken ct);
    Task<GradeApplyReceipt> ApplyAsync(AssessmentPrincipalContext principal, Guid operationKey, string payloadHash,
        IReadOnlyList<ScorePreviewCell> cells, CancellationToken ct);
}

public sealed class UnconfiguredGradeDestination : IGradeDestination
{
    private static DomainRuleException Unavailable() => new("GRADE_SOURCE_UNAVAILABLE", "Verified legacy grade context is not configured.");
    public Task<IReadOnlyList<GradeSlotRecord>> DiscoverSlotsAsync(AssessmentPrincipalContext p, SchoolBotCourseRunRecord r, CancellationToken ct) => throw Unavailable();
    public Task RecheckSlotAsync(AssessmentPrincipalContext p, SchoolBotCourseRunRecord r, GradeSlotRecord s, bool empty, CancellationToken ct) => throw Unavailable();
    public Task<IReadOnlyList<GradeTargetCell>> ReadCellsAsync(AssessmentPrincipalContext p, SchoolBotCourseRunRecord r, IReadOnlyList<GradeSlotRecord> s, IReadOnlyList<int> l, CancellationToken ct) => throw Unavailable();
    public Task RequireVerifiedWriterAsync(CancellationToken ct) => throw new DomainRuleException("WRITER_NOT_VERIFIED", "Legacy writer, calculator parity, coexistence, and durable receipt evidence are required.");
    public Task<GradeApplyReceipt?> FindReceiptAsync(Guid key, string hash, CancellationToken ct) => throw new DomainRuleException("RECEIPT_UNAVAILABLE", "Durable grade receipt adapter is not configured.");
    public Task<GradeApplyReceipt> ApplyAsync(AssessmentPrincipalContext p, Guid key, string hash, IReadOnlyList<ScorePreviewCell> c, CancellationToken ct) => throw Unavailable();
}
