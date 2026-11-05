namespace ServiceSchoolBot.Domain.Assessment;

public sealed record LegacyGradeMathSettings(decimal BeforeMax, decimal AfterMax, decimal MidMax, decimal FinalMax,
    decimal BeforeRatio, decimal AfterRatio, decimal MidRatio, decimal FinalRatio, decimal QuizRatio,
    int DecimalPlaces, string? TemplateGrades);
