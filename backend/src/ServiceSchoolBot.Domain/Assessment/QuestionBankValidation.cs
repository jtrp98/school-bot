using System.Globalization;
using System.Text.Json;

namespace ServiceSchoolBot.Domain.Assessment;

public static class QuestionBankValidation
{
    public static void ValidateText(JsonElement text)
    {
        if (text.ValueKind != JsonValueKind.Object) Invalid();
        var found = false;
        foreach (var property in text.EnumerateObject())
        {
            if (property.Name == "default_locale")
            {
                if (property.Value.ValueKind != JsonValueKind.String ||
                    property.Value.GetString() is not ("th" or "en" or "zh")) Invalid();
                continue;
            }
            if (property.Name is not ("th" or "en" or "zh") ||
                property.Value.ValueKind != JsonValueKind.String ||
                string.IsNullOrWhiteSpace(property.Value.GetString()) || property.Value.GetString()!.Length > 8000)
                Invalid();
            found = true;
        }
        if (!found) Invalid();
        if (text.TryGetProperty("default_locale", out var locale) &&
            !text.TryGetProperty(locale.GetString()!, out _)) Invalid();
    }

    public static void Validate(string title, IReadOnlyList<QuestionBankQuestion> questions, string sourceRef)
    {
        if (string.IsNullOrWhiteSpace(title)) Invalid();
        try
        {
            using var document = JsonDocument.Parse(title);
            ValidateText(document.RootElement);
        }
        catch (JsonException) { Invalid(); }
        if (string.IsNullOrWhiteSpace(sourceRef) || sourceRef.Length > 2048 || questions is null || questions.Count == 0)
            Invalid();
        var keys = new HashSet<Guid>();
        foreach (var question in questions!)
        {
            if (question is null || question.QuestionKey == Guid.Empty || !keys.Add(question.QuestionKey)) Invalid();
            if (question!.Kind is not ("single_choice" or "boolean"))
                throw new DomainRuleException(ErrorCodes.QuestionKindUnsupported, "Question kind is unsupported.");
            ValidateText(question.Prompt);
            if (question.Options is null || question.Options.Count < 2 ||
                (question.Kind == "boolean" && question.Options.Count != 2)) Invalid();
            var options = new HashSet<Guid>();
            foreach (var option in question.Options!)
            {
                if (option is null || option.OptionKey == Guid.Empty || !options.Add(option.OptionKey)) Invalid();
                ValidateText(option!.Text);
            }
            if (question.CorrectOptionKeys is null || question.CorrectOptionKeys.Count != 1 ||
                !options.Contains(question.CorrectOptionKeys[0])) Invalid();
            var score = question.MaxScore;
            if (string.IsNullOrEmpty(score) || score.Any(c => c is not (>= '0' and <= '9') and not '.') ||
                score.Count(c => c == '.') > 1 || score.StartsWith('.') || score.EndsWith('.') ||
                (score.Contains('.') && score.Length - score.IndexOf('.') - 1 > 6) ||
                !decimal.TryParse(score, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var parsed) ||
                parsed <= 0 || parsed >= 1_000_000_000_000m) Invalid();
        }
    }

    [System.Diagnostics.CodeAnalysis.DoesNotReturn]
    public static void Invalid() => throw new DomainRuleException(ErrorCodes.InvalidInput, "Invalid question bank input.");
}
