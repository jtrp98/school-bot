namespace ServiceSchoolBot.Domain.Assessment;

public sealed record SchoolBotLevelRecord(int Id, int Ordinal, string Title);
public sealed record SchoolBotLevelPromotionPolicyRecord(int Id, int LevelId, int Revision, decimal Threshold);
public sealed record SchoolBotLevelProposalRecord(int Id, int SchoolId, int CourseRunId, int TargetCurriculumId,
    string SnapshotHash, string Snapshot, string State, string? DecisionBy, DateTimeOffset? DecisionAt,
    string? Reason, int? SuccessorRunId, int EmployeeId, DateTimeOffset CreatedAt, DateTimeOffset UpdateDate);
