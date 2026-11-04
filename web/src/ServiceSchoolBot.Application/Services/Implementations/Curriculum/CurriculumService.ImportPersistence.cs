using ServiceSchoolBot.Application.Common.Model.Curriculum;
using System.Text.Json;
using ServiceSchoolBot.Domain;
using ServiceSchoolBot.Domain.Curriculum;

namespace ServiceSchoolBot.Application.Services.Implementations.Curriculum;

public sealed partial class CurriculumService
{
    public static (
        List<CurriculumImportLessonResult> Results,
        List<PreparedCurriculumImportRow> Created) PrepareStoredImportRows(
        int curriculumId, SchoolBotCurriculumRecord curriculum,
        IReadOnlyList<SchoolBotLessonRecord> storedLessons, IReadOnlyList<PreparedCurriculumImportRow> rows)
    {
        if (curriculum.Lifecycle != "draft" || curriculum.PublishedAt is not null || curriculum.SchoolId is not null)
            throw new DomainRuleException(ErrorCodes.StateConflict, "Curriculum is not a central draft.");
        var existingLessons = storedLessons.Where(lesson => lesson.CurriculumId == curriculumId).ToArray();
        var results = new List<CurriculumImportLessonResult>();
        var created = new List<PreparedCurriculumImportRow>();
        var keys = new HashSet<Guid>();
        var ordinals = new HashSet<int>();
        foreach (var row in rows)
        {
            if (row is null || row.Lesson is null)
                throw new DomainRuleException(ErrorCodes.InvalidInput, "Invalid import row.");
            var lesson = row.Lesson;
            var source = LessonImportSourceCodec.Decode(row.ImportSource, lesson.LessonKey);
            if (!keys.Add(lesson.LessonKey) || lesson.Ordinal < 1 || !ordinals.Add(lesson.Ordinal))
                throw new DomainRuleException(ErrorCodes.InvalidInput, "Duplicate import identity or ordinal.");
            var matches = existingLessons.Where(item => item.LessonKey == lesson.LessonKey).ToArray();
            if (matches.Length == 0)
            {
                if (existingLessons.Any(item => !item.IsDelete && item.Ordinal == lesson.Ordinal))
                    throw ImportConflict(lesson.LessonKey);
                created.Add(row);
                results.Add(new(lesson.LessonKey, 0, "created"));
                continue;
            }
            var stored = matches[0];
            if (matches.Length != 1 || stored.IsDelete || stored.ImportSource is null)
                throw ImportConflict(lesson.LessonKey);
            LessonImportSource storedSource;
            try { storedSource = LessonImportSourceCodec.Decode(stored.ImportSource, stored.LessonKey); }
            catch (JsonException) { throw ImportConflict(lesson.LessonKey); }
            if (storedSource.RowHash != source.RowHash) throw ImportConflict(lesson.LessonKey);
            results.Add(new(lesson.LessonKey, stored.SchoolBotLessonId, "unchanged"));
        }
        return (results, created);
    }

    private static DomainRuleException ImportConflict(Guid lessonKey)
    {
        var error = new DomainRuleException(ErrorCodes.StateConflict, "Imported lesson conflicts with stored content.");
        error.Data["details"] = new { source_row_key = lessonKey };
        return error;
    }
}
