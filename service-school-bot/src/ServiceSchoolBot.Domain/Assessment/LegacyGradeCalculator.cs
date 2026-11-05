using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace ServiceSchoolBot.Domain.Assessment;

public static class LegacyGradeCalculator
{
    public const string Version = "schoolbright-scoredata-four-periods-v1";

    public static IReadOnlyDictionary<string, string> Calculate(JsonArray scores, JsonObject before, LegacyGradeMathSettings settings)
    {
        if (settings.DecimalPlaces is not (0 or 2) || new[] { settings.BeforeMax, settings.AfterMax, settings.MidMax,
            settings.FinalMax, settings.BeforeRatio, settings.AfterRatio, settings.MidRatio, settings.FinalRatio, settings.QuizRatio }.Any(n => n < 0))
            throw Invalid("Unsupported grade calculation settings.");
        decimal Sum(string prefix, int maximum)
        {
            decimal sum = 0;
            foreach (var item in scores)
            {
                var id = item?["ScoreIdentifier"]?.GetValue<string>();
                if (id is null || !id.StartsWith(prefix, StringComparison.Ordinal) ||
                    !int.TryParse(id.AsSpan(prefix.Length), NumberStyles.None, CultureInfo.InvariantCulture, out var ordinal) ||
                    ordinal < 1 || ordinal > maximum || id != prefix + ordinal.ToString(CultureInfo.InvariantCulture)) continue;
                var text = item?["Score"]?.GetValue<string>() ?? "";
                if (text.Length > 64) throw Invalid("Legacy score text exceeds the supported numeric format.");
                var cleaned = text.Trim();
                if (cleaned.Length == 0) continue;
                if (!decimal.TryParse(cleaned, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var value) || value < 0)
                    throw Invalid("A legacy score cannot be interpreted without changing its meaning.");
                sum += value;
            }
            return sum;
        }
        decimal Scale(decimal value, decimal ratio, decimal maximum) => maximum > 0
            ? decimal.Round(value * ratio / maximum, settings.DecimalPlaces, MidpointRounding.ToEven) : 0;
        static string Text(decimal value) => value.ToString("0.############################", CultureInfo.InvariantCulture);
        var first = Sum("scoreGrade", 20);
        var second = Sum("scoreCheewat", 20);
        var mid = Sum("scoreMid", 10);
        var final = Sum("scoreFinal", 10);
        var before100 = Scale(first, settings.BeforeRatio, settings.BeforeMax);
        var after100 = Scale(second, settings.AfterRatio, settings.AfterMax);
        var mid100 = Scale(mid, settings.MidRatio, settings.MidMax);
        var final100 = Scale(final, settings.FinalRatio, settings.FinalMax);
        var total = before100 + after100 + mid100 + final100;
        var result = new Dictionary<string, string>
        {
            ["ScoreBeforeMidTerm"] = Text(decimal.Round(first, 2, MidpointRounding.ToEven)),
            ["ScoreAfterMidTerm"] = Text(decimal.Round(second, 2, MidpointRounding.ToEven)),
            ["scoreMidTerm"] = Text(decimal.Round(mid, 2, MidpointRounding.ToEven)),
            ["scoreFinalTerm"] = Text(decimal.Round(final, 2, MidpointRounding.ToEven)),
            ["scoreBeforeAfterMidTerm"] = Text(decimal.Round(first + second, 2, MidpointRounding.ToEven)),
            ["getBeforeQuiz100"] = Text(before100), ["getAfterQuiz100"] = Text(after100),
            ["getMid100"] = Text(mid100), ["getFinal100"] = Text(final100),
            ["getQuiz100"] = Text(Scale(first + second, settings.QuizRatio, settings.BeforeMax + settings.AfterMax)),
            ["getScore100"] = Text(total),
            ["getGradeLabel"] = before["getGradeLabel"]?.GetValue<string>() ?? "",
            ["GradeSymbol"] = before["GradeSymbol"]?.GetValue<string>() ?? ""
        };
        // Special results and manual edit rows retain their own authority.
        if (before["getSpecial"]?.GetValue<string>() != "-1") return result;
        if (!string.IsNullOrWhiteSpace(settings.TemplateGrades))
        {
            using var template = JsonDocument.Parse(settings.TemplateGrades, new JsonDocumentOptions { MaxDepth = 16 });
            if (template.RootElement.ValueKind != JsonValueKind.Array || template.RootElement.GetArrayLength() > 100)
                throw Invalid("Grade template is unsupported.");
            var matches = template.RootElement.EnumerateArray().Where(t => total >= t.GetProperty("minScore").GetDecimal() &&
                total <= t.GetProperty("maxScore").GetDecimal()).ToArray();
            if (matches.Length != 1) throw Invalid("Grade template bands are incomplete or overlap.");
            var gpa = matches[0].GetProperty("gradeForGpa").GetDecimal();
            result["getGradeLabel"] = gpa != 0 && gpa % 1 == 0 ? gpa.ToString("0.0", CultureInfo.InvariantCulture) : gpa.ToString("0.##", CultureInfo.InvariantCulture);
            var symbol = matches[0].GetProperty("grade").GetString();
            if (!string.IsNullOrEmpty(symbol)) result["GradeSymbol"] = symbol;
        }
        else
        {
            result["getGradeLabel"] = total > 79 ? "4.0" : total > 74 ? "3.5" : total > 69 ? "3.0" : total > 64 ? "2.5" :
                total > 59 ? "2.0" : total > 54 ? "1.5" : total > 49 ? "1.0" : "0";
            if (total <= 49) result["GradeSymbol"] = "0";
        }
        return result;
    }

    private static DomainRuleException Invalid(string reason) => new("CALCULATOR_NOT_VERIFIED", reason);
}
