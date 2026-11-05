using System.Globalization;
using System.Text.Json;

namespace ServiceSchoolBot.Domain.Assessment;

public static class AttemptEvaluation
{
    public const string Version = "choice-v1";

    public static DateTimeOffset? Deadline(DateTimeOffset start, AssessmentPolicy policy, DateTimeOffset? closes)
    {
        var timed = policy.TimeLimitSeconds is { } seconds ? start.AddSeconds(seconds) : (DateTimeOffset?)null;
        return timed is null ? closes : closes is null || timed < closes ? timed : closes;
    }

    public static void ValidateAnswers(AttemptSnapshot snapshot, IReadOnlyList<AssessmentAnswer> answers)
    {
        if (answers is null || answers.Count > snapshot.Questions.Count) QuestionBankValidation.Invalid();
        var seen = new HashSet<Guid>();
        foreach (var answer in answers!)
        {
            if (answer is null || !seen.Add(answer.QuestionKey)) QuestionBankValidation.Invalid();
            var question = snapshot.Questions.SingleOrDefault(q => q.QuestionKey == answer!.QuestionKey);
            if (question is null || answer!.SelectedOptionKeys is null || answer.SelectedOptionKeys.Count > 1 ||
                answer.SelectedOptionKeys.Any(key => !question.Options.Any(o => o.OptionKey == key)))
                QuestionBankValidation.Invalid();
        }
    }

    public static decimal Score(AttemptSnapshot snapshot, IReadOnlyList<AssessmentAnswer> answers)
    {
        if (snapshot.EvaluatorVersion != Version)
            throw new DomainRuleException(ErrorCodes.GraderUnavailable, "Stored evaluator is unavailable.");
        ValidateAnswers(snapshot, answers);
        return snapshot.Questions.Where(q => answers.Any(a => a.QuestionKey == q.QuestionKey &&
            a.SelectedOptionKeys.SequenceEqual(q.CorrectOptionKeys)))
            .Sum(q => decimal.Parse(q.MaxScore, CultureInfo.InvariantCulture));
    }

    public static SchoolBotAttemptRecord Apply(SchoolBotAttemptRecord attempt,
        IReadOnlyList<AssessmentAnswer>? answers, bool submit, DateTimeOffset now, string? expectedETag)
    {
        if (attempt.State is "submitted" or "voided" || attempt.State == "expired" && attempt.EarnedScore is not null)
            return attempt;
        var snapshot = JsonSerializer.Deserialize<AttemptSnapshot>(attempt.Snapshot)
            ?? throw new DomainRuleException(ErrorCodes.StateConflict, "Stored snapshot is unavailable.");
        var expired = attempt.State == "expired" || attempt.DeadlineAt is { } deadline && now >= deadline;
        if (!expired)
        {
            if (string.IsNullOrWhiteSpace(expectedETag))
                throw new DomainRuleException(ErrorCodes.PreconditionRequired, "If-Match is required.");
            if (!Concurrency.OpaqueConcurrencyToken.Matches(expectedETag, attempt.UpdateDate))
                throw new DomainRuleException(ErrorCodes.VersionConflict, "Concurrency token mismatch.");
        }
        // Expiry evaluates only answers accepted by an earlier successful guarded write.
        var accepted = expired || answers is null
            ? JsonSerializer.Deserialize<AssessmentAnswer[]>(attempt.Answers)! : answers;
        ValidateAnswers(snapshot, accepted);
        var terminal = expired || submit;
        return attempt with
        {
            Answers = JsonSerializer.Serialize(accepted),
            State = expired ? "expired" : submit ? "submitted" : "started",
            SubmittedAt = !expired && submit ? now : null,
            EarnedScore = terminal ? Score(snapshot, accepted) : null,
            UpdateDate = now > attempt.UpdateDate ? now : attempt.UpdateDate!.Value.AddTicks(10)
        };
    }
}
