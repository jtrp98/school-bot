namespace ServiceSchoolBot.Application.Common.Model.Curriculum;

public sealed record CoursePlanSuppliedRow(
    int SourcePosition,
    Guid SourceRowKey,
    string? GradeLevel = null,
    string? LearningConcept = null,
    string? UnitNumber = null,
    bool? SourceApproved = null,
    string? Topic = null,
    int? Periods = null,
    string? Details = null,
    string? Interdisciplinary = null,
    string? LessonPlan = null,
    string? ReviewComment = null,
    string? SlideSet1 = null,
    string? ExerciseSet1Details = null,
    string? ExerciseSet1Link = null,
    string? ColumnO = null,
    string? TopicSet2 = null,
    string? DetailsSet2 = null,
    string? InterdisciplinarySet2 = null,
    string? SlideSet2 = null,
    string? ExerciseSet2Details = null,
    string? ExerciseSet2Link = null
);

public sealed record CoursePlanMediaItem(
    int SetNumber,
    string Kind,
    string? TextOrUrl,
    string? Description = null
);

public sealed record TransformedCoursePlanLesson(
    Guid LessonKey,
    Guid SourceRowKey,
    int SourcePosition,
    bool IsDraft,
    bool SourceApproved,
    string? GradeLevel,
    string? LearningConcept,
    string? SourceUnitReference,
    string? Title,
    int? Periods,
    string? Details,
    string? Interdisciplinary,
    string? LessonPlan,
    string? ReviewComment,
    string? SecondaryTopic,
    string? SecondaryDetails,
    string? SecondaryInterdisciplinary,
    IReadOnlyList<CoursePlanMediaItem> MediaSet1,
    IReadOnlyList<CoursePlanMediaItem> MediaSet2,
    IReadOnlyList<CoursePlanMediaItem> AllMedia
);

public sealed record CoursePlanRowValidationError(
    Guid SourceRowKey,
    int SourcePosition,
    string Field,
    string Code
);

public sealed record CoursePlanTransformResult(
    bool IsValid,
    IReadOnlyList<TransformedCoursePlanLesson> Lessons,
    IReadOnlyList<CoursePlanRowValidationError> Errors
);
