namespace ServiceSchoolBot.Application.Common.Model.Assessment;

public sealed record ClassroomLearningContext(IReadOnlyList<int> LearnerIds, int NextYear, string NextTerm,
    DateTimeOffset EffectiveAt, string AuthorityVersion);
