namespace ServiceSchoolBot.Application.Services.Implementations.Assessment;

public sealed record LearningScoreSource(string Kind, int Id, DateTimeOffset UpdateDate, int ActivityId,
    decimal Earned, decimal MaxScore, int WeightBps);
public sealed record LearningScoreTotal(int CourseRunId, int LearnerId, int PolicyId, decimal Total,
    bool Complete, IReadOnlyList<LearningScoreSource> Sources, IReadOnlyList<LearningScoreGap>? Gaps = null);
public sealed record LearningScoreGap(int ActivityId, string ReasonCode);
