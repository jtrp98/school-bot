using ServiceSchoolBot.Domain;

namespace ServiceSchoolBot.Application.Services.Implementations.WeightedScore;

public readonly record struct WeightedActivityScore(
    long ActivityId, decimal Earned, decimal Max, int WeightBps, bool IsQuiz);

public static class WeightedScoreCalculator
{
    public static decimal Calculate(IReadOnlyList<WeightedActivityScore>? activities, bool quizCounts)
    {
        if (activities is null || activities.Count == 0)
            throw new DomainRuleException(ErrorCodes.PolicyNotConfigured,
                "Scoring activities are required.");

        var activityIds = new HashSet<long>();
        long totalWeight = 0;
        decimal total = 0;
        try
        {
            foreach (var activity in activities)
            {
                if (activity.ActivityId <= 0 || !activityIds.Add(activity.ActivityId) ||
                    activity.WeightBps is < 0 or > 10000 ||
                    (!quizCounts && activity.IsQuiz && activity.WeightBps != 0))
                    throw new DomainRuleException(ErrorCodes.ScoringWeightInvalid,
                        "Activity IDs and scoring weights must form a valid policy.");

                if (activity.Max <= 0 || activity.Earned < 0 || activity.Earned > activity.Max)
                    throw new DomainRuleException(ErrorCodes.PolicyNotConfigured,
                        "A numeric score within a positive source maximum is required.");

                totalWeight += activity.WeightBps;
                total += activity.Earned / activity.Max * activity.WeightBps / 100m;
            }
        }
        catch (OverflowException)
        {
            throw new DomainRuleException(ErrorCodes.PolicyNotConfigured,
                "The scoring calculation cannot be represented as a decimal.");
        }

        if (totalWeight != 10000)
            throw new DomainRuleException(ErrorCodes.ScoringWeightInvalid,
                "Scoring weights must total 10000 basis points.");

        return total;
    }
}
