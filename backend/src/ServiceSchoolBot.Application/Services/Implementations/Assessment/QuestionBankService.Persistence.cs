using ServiceSchoolBot.Domain;
using ServiceSchoolBot.Domain.Assessment;
using ServiceSchoolBot.Domain.Concurrency;
using ServiceSchoolBot.Domain.Curriculum;

namespace ServiceSchoolBot.Application.Services.Implementations.Assessment;

public sealed partial class QuestionBankService
{
    public static void ValidateStoredBankAudit(SchoolBotAuditEventRecord audit, string eventName)
    {
        ArgumentNullException.ThrowIfNull(audit);
        if (audit.Event != eventName || audit.EntityType != "SchoolBotQuestionBank" || audit.SchoolId is not null ||
            audit.IsDelete || audit.RequestId == Guid.Empty || string.IsNullOrWhiteSpace(audit.Actor) ||
            audit.CreateBy is null || string.IsNullOrWhiteSpace(audit.Reason) || audit.Reason.Length > 1000)
            QuestionBankValidation.Invalid();
    }

    public static void ValidateStoredNewBank(SchoolBotQuestionBankRecord record)
    {
        ArgumentNullException.ThrowIfNull(record);
        if (record.BankKey == Guid.Empty || record.Revision < 1 || record.SchoolId is not null ||
            record.Lifecycle != "draft" || record.IsDelete || record.PublishedAt is not null)
            QuestionBankValidation.Invalid();
        QuestionBankValidation.Validate(record.Title, record.Questions, record.SourceRef);
    }

    public static SchoolBotQuestionBankRecord PrepareStoredBankPublication(
        SchoolBotQuestionBankRecord existing, SchoolBotAuditEventRecord audit, DateTimeOffset now)
    {
        if (existing.CreateBy == audit.CreateBy || existing.UpdateBy == audit.CreateBy)
            throw new DomainRuleException(ErrorCodes.FourEyesRequired, "The author cannot approve this draft.");
        QuestionBankValidation.Validate(existing.Title, existing.Questions, existing.SourceRef);
        return existing with
        {
            Lifecycle = "published",
            PublishedAt = now,
            UpdateBy = audit.CreateBy,
            UpdateDate = now
        };
    }

    public static void ValidateStoredBankChange(
        SchoolBotQuestionBankRecord existing, string expectedETag, SchoolBotAuditEventRecord audit,
        IReadOnlyList<SchoolBotAuditEventRecord> auditEvents, int id)
    {
        if (existing.Lifecycle != "draft")
            throw new DomainRuleException(ErrorCodes.StateConflict, "Question bank must be a draft.");
        if (!OpaqueConcurrencyToken.Matches(expectedETag, existing.UpdateDate))
            throw new DomainRuleException(ErrorCodes.VersionConflict, "Concurrency token mismatch.");
        if (audit.Event == "QuestionBank.Published" && auditEvents.Any(entry =>
            !entry.IsDelete && entry.EntityType == "SchoolBotQuestionBank" && entry.EntityId == id &&
            entry.Event is "QuestionBank.Created" or "QuestionBank.Updated" &&
            (entry.Actor == audit.Actor || entry.CreateBy == audit.CreateBy)))
            throw new DomainRuleException(ErrorCodes.FourEyesRequired, "A draft contributor cannot approve this bank.");
    }
}
