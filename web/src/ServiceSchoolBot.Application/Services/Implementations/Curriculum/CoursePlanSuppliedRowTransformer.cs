using ServiceSchoolBot.Application.Common.Model.Curriculum;
using ServiceSchoolBot.Domain;

namespace ServiceSchoolBot.Application.Services.Implementations.Curriculum;

public static class CoursePlanSuppliedRowTransformer
{
    private const string SlideKind = "slide";
    private const string ExerciseKind = "exercise";

    public static TransformedCoursePlanLesson Transform(CoursePlanSuppliedRow row)
    {
        ArgumentNullException.ThrowIfNull(row);

        var mediaSet1 = new List<CoursePlanMediaItem>();
        if (!string.IsNullOrWhiteSpace(row.SlideSet1))
            mediaSet1.Add(new CoursePlanMediaItem(1, SlideKind, row.SlideSet1));

        if (!string.IsNullOrWhiteSpace(row.ExerciseSet1Link) || !string.IsNullOrWhiteSpace(row.ExerciseSet1Details))
            mediaSet1.Add(new CoursePlanMediaItem(1, ExerciseKind, row.ExerciseSet1Link, row.ExerciseSet1Details));

        var mediaSet2 = new List<CoursePlanMediaItem>();
        if (!string.IsNullOrWhiteSpace(row.SlideSet2))
            mediaSet2.Add(new CoursePlanMediaItem(2, SlideKind, row.SlideSet2));

        if (!string.IsNullOrWhiteSpace(row.ExerciseSet2Link) || !string.IsNullOrWhiteSpace(row.ExerciseSet2Details))
            mediaSet2.Add(new CoursePlanMediaItem(2, ExerciseKind, row.ExerciseSet2Link, row.ExerciseSet2Details));

        var allMedia = new List<CoursePlanMediaItem>(mediaSet1.Count + mediaSet2.Count);
        allMedia.AddRange(mediaSet1);
        allMedia.AddRange(mediaSet2);

        return new TransformedCoursePlanLesson(
            LessonKey: row.SourceRowKey,
            SourceRowKey: row.SourceRowKey,
            SourcePosition: row.SourcePosition,
            IsDraft: true,
            SourceApproved: row.SourceApproved == true,
            GradeLevel: row.GradeLevel,
            LearningConcept: row.LearningConcept,
            SourceUnitReference: row.UnitNumber,
            Title: row.Topic,
            Periods: row.Periods,
            Details: row.Details,
            Interdisciplinary: row.Interdisciplinary,
            LessonPlan: row.LessonPlan,
            ReviewComment: row.ReviewComment,
            SecondaryTopic: row.TopicSet2,
            SecondaryDetails: row.DetailsSet2,
            SecondaryInterdisciplinary: row.InterdisciplinarySet2,
            MediaSet1: mediaSet1,
            MediaSet2: mediaSet2,
            AllMedia: allMedia
        );
    }

    public static IReadOnlyList<TransformedCoursePlanLesson> TransformBatch(IEnumerable<CoursePlanSuppliedRow> rows)
    {
        ArgumentNullException.ThrowIfNull(rows);
        return rows.Select(Transform).ToArray();
    }

    public static CoursePlanTransformResult ValidateAndTransform(IReadOnlyList<CoursePlanSuppliedRow> rows)
    {
        ArgumentNullException.ThrowIfNull(rows);

        var errors = new List<CoursePlanRowValidationError>();

        foreach (var row in rows)
        {
            if (row.SourceRowKey == Guid.Empty)
            {
                errors.Add(new CoursePlanRowValidationError(
                    row.SourceRowKey,
                    row.SourcePosition,
                    "source_row_key",
                    ErrorCodes.InvalidInput));
            }

            if (row.SourcePosition <= 0)
            {
                errors.Add(new CoursePlanRowValidationError(
                    row.SourceRowKey,
                    row.SourcePosition,
                    "source_position",
                    ErrorCodes.InvalidInput));
            }
        }

        if (errors.Count > 0)
        {
            return new CoursePlanTransformResult(
                IsValid: false,
                Lessons: Array.Empty<TransformedCoursePlanLesson>(),
                Errors: errors);
        }

        var lessons = TransformBatch(rows);
        return new CoursePlanTransformResult(
            IsValid: true,
            Lessons: lessons,
            Errors: Array.Empty<CoursePlanRowValidationError>());
    }
}
