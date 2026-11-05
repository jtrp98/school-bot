namespace ServiceSchoolBot.Domain.Assessment;

public sealed record GradeSlotRecord(int Id, int SchoolId, int CourseRunId, int PlanCourseId, int CourseId,
    int NGradeId, int AssessmentId, string ScoreIdentifier, string Category, string TargetSnapshot,
    string OwnershipReason, DateTimeOffset? RetiredAt, DateTimeOffset UpdateDate);
public sealed record ScoreMappingRecord(int Id, int SchoolId, int CourseRunId, int GradeSlotId,
    int ActivityId, DateTimeOffset? RetiredAt, DateTimeOffset UpdateDate);
public sealed record ScoreMappingSource(
    [property: System.Text.Json.Serialization.JsonPropertyName("activity_id")] int ActivityId,
    [property: System.Text.Json.Serialization.JsonPropertyName("indicator_key")] Guid? IndicatorKey = null,
    [property: System.Text.Json.Serialization.JsonPropertyName("weight_bps")] int WeightBps = 10000);
public sealed record GradeTargetCell(int LearnerId, int GradeDetailId, string Xmin, string BeforeDocumentHash,
    int GradeSlotId, decimal TargetMax, decimal? CurrentValue, string ContextVersion, bool IsLocked);
public sealed record ScorePreviewCell(ScoreMappingRecord Mapping, GradeSlotRecord Slot, GradeTargetCell Target,
    LearningScoreEvidence Source, decimal RawTarget, decimal IntegerTarget);
public sealed record LearningScoreEvidence(string Kind, int Id, int ActivityId, decimal Earned, decimal Max,
    DateTimeOffset UpdatedAt, int PolicyId);
public sealed record SkippedScoreCell(int MappingId, int GradeSlotId, string ReasonCode, decimal? OldScore);
public sealed record ScorePreviewLearner(int LearnerId, string State, IReadOnlyList<string> Reasons,
    IReadOnlyList<ScorePreviewCell> Cells, IReadOnlyList<SkippedScoreCell>? SkippedCells = null);
public sealed record ScorePreviewRecord(int Id, int SchoolId, int CourseRunId, string Principal, string SnapshotHash,
    DateTimeOffset ExpiresAt, string State, string Context, IReadOnlyList<ScorePreviewLearner> Learners,
    DateTimeOffset UpdateDate);
public sealed record ScoreExportRecord(int Id, int SchoolId, int PreviewId, string ConfirmedBy, int EmployeeId,
    DateTimeOffset ConfirmedAt, string State, DateTimeOffset UpdateDate);
public sealed record ScoreOutboxRecord(int Id, int SchoolId, int ExportId, int LearnerId, Guid OperationKey,
    string PayloadHash, string State, int Attempts, string? LeaseOwner, DateTimeOffset? LeaseUntil,
    DateTimeOffset NextAttemptAt, long? ReceiptId, string? ReceiptHash, string? LastErrorCode, DateTimeOffset UpdateDate);
public sealed record GradeApplyReceipt(long Id, string Hash);

public static class GradeConversion
{
    public static (decimal Raw, decimal Integer) Convert(decimal earned, decimal max, decimal targetMax)
    {
        if (max <= 0 || targetMax <= 0 || earned < 0 || earned > max)
            throw new DomainRuleException("SOURCE_INVALID", "A valid numeric source and destination maximum are required.");
        var raw = earned / max * targetMax;
        var integer = decimal.Floor(raw + 0.5m);
        if (integer > targetMax) throw new DomainRuleException("TARGET_MAX_EXCEEDED", "Rounded score exceeds the destination maximum.");
        return (raw, integer);
    }
}
