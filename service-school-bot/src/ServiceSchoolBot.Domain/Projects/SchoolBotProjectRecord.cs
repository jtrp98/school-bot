namespace ServiceSchoolBot.Domain.Projects;

public sealed record SchoolBotProjectRecord(
    int SchoolBotProjectId,
    string OwnerKind,
    int OwnerId,
    string Mode,
    string Name,
    int CurrentRevision,
    bool IsDelete,
    int? CreateBy,
    DateTimeOffset? CreateDate,
    int? UpdateBy,
    DateTimeOffset? UpdateDate,
    int SchoolId);

public sealed record SchoolBotProjectRevisionRecord(
    int SchoolBotProjectRevisionId,
    int ProjectId,
    int Revision,
    string Content,
    string ContentHash,
    bool IsDelete,
    int? CreateBy,
    DateTimeOffset? CreateDate,
    int? UpdateBy,
    DateTimeOffset? UpdateDate,
    int SchoolId);

public sealed record CommitProjectRequest(
    int? ProjectId,
    int SchoolId,
    string OwnerKind,
    int OwnerId,
    string Mode,
    string Name,
    string Content,
    string? ExpectedETag = null,
    int? UserId = null);

public sealed record ProjectCommitResult(
    SchoolBotProjectRecord Project,
    SchoolBotProjectRevisionRecord Revision);
