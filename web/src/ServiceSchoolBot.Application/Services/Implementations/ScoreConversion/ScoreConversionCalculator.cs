using System.Numerics;
using ServiceSchoolBot.Domain;

namespace ServiceSchoolBot.Application.Services.Implementations.ScoreConversion;

public readonly record struct ScoreConversion(decimal RawTarget, decimal RoundedScore);

public static class ScoreConversionCalculator
{
    public static ScoreConversion Calculate(decimal earned, decimal max, decimal targetMax)
    {
        if (targetMax <= 0)
            throw new DomainRuleException(ErrorCodes.TargetNotReady,
                "A positive target maximum is required.");

        if (max <= 0 || earned < 0 || earned > max)
            throw new DomainRuleException(ErrorCodes.PolicyNotConfigured,
                "A numeric score within a positive source maximum is required.");

        try
        {
            var rawTarget = earned / max * targetMax;
            // Intermediate decimal division can move an exact midpoint below half.
            var numerator = ToScaledInteger(earned) * ToScaledInteger(targetMax);
            var denominator = ToScaledInteger(max) * BigInteger.Pow(10, 28);
            var roundedScore = BigInteger.DivRem(numerator, denominator, out var remainder);
            if (remainder * 2 >= denominator)
                roundedScore++;

            return new(rawTarget, (decimal)roundedScore);
        }
        catch (OverflowException)
        {
            throw new DomainRuleException(ErrorCodes.TargetNotReady,
                "The target score cannot be represented as a decimal.");
        }
    }

    private static BigInteger ToScaledInteger(decimal value)
    {
        var bits = decimal.GetBits(value);
        var coefficient = (BigInteger)(uint)bits[0] +
            ((BigInteger)(uint)bits[1] << 32) + ((BigInteger)(uint)bits[2] << 64);
        var scale = (bits[3] >> 16) & 0xff;
        return coefficient * BigInteger.Pow(10, 28 - scale);
    }
}
