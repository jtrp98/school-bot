using ServiceSchoolBot.Application.Services.Implementations.ScoreConversion;
using ServiceSchoolBot.Application.Services.Implementations.WeightedScore;
using ServiceSchoolBot.Domain;

namespace ServiceSchoolBot.Tests;

public sealed class PureMathAdversarialTests
{
    public static IEnumerable<object[]> WeightedValidCases()
    {
        yield return WeightedCase("ตัวอย่างได้ 79", true, 79m,
            Score(1, 8, 10, 2000), Score(2, 6, 10, 3000, true), Score(3, 18, 20, 5000));
        yield return WeightedCase("ตัวอย่างได้ 81", true, 81m,
            Score(1, 8, 10, 3000), Score(2, 6, 10, 2000, true), Score(3, 18, 20, 5000));
        yield return WeightedCase("ไม่ปัดคะแนนรายส่วน", true, 62.5025m,
            Score(1, 1, 8, 3333), Score(2, 7, 8, 6667));
        yield return WeightedCase("ส่วนกิจกรรม 16", true, 16m,
            Score(1, 8, 10, 2000), Score(2, 0, 1, 8000));
        yield return WeightedCase("ส่วน Quiz 18", true, 18m,
            Score(1, 6, 10, 3000, true), Score(2, 0, 1, 7000));
        yield return WeightedCase("ส่วน Test 45", true, 45m,
            Score(1, 18, 20, 5000), Score(2, 0, 1, 5000));
        yield return WeightedCase("คะแนนศูนย์", true, 0m,
            Score(1, 0, 10, 10000));
        yield return WeightedCase("คะแนนเต็ม", true, 100m,
            Score(1, 10, 10, 10000));
        yield return WeightedCase("น้ำหนักศูนย์ไม่เปลี่ยนผล", true, 50m,
            Score(1, 1, 2, 10000), Score(2, 999, 1000, 0));
        yield return WeightedCase("quiz ศูนย์คะแนน", false, 50m,
            Score(1, 1, 2, 10000), Score(2, 0, 10, 0, true));
        yield return WeightedCase("quiz เต็มคะแนน", false, 50m,
            Score(1, 1, 2, 10000), Score(2, 10, 10, 0, true));
        yield return WeightedCase("quiz นับคะแนน", true, 75m,
            Score(1, 1, 2, 5000), Score(2, 1, 1, 5000, true));
        yield return WeightedCase("ActivityId ขอบบน canonical K", true, 100m,
            Score(int.MaxValue, 1, 1, 10000));
        yield return WeightedCase("max เล็กสุดที่ decimal แทนได้", true, 100m,
            Score(1, 0.0000000000000000000000000001m, 0.0000000000000000000000000001m, 10000));
        yield return WeightedCase("max ใหญ่สุดที่ decimal แทนได้", true, 100m,
            Score(1, decimal.MaxValue, decimal.MaxValue, 10000));
        yield return WeightedCase("เก็บทศนิยมขนาดเล็ก", true, 0.00000000000000000000000001m,
            Score(1, 1, 10000000000000000000000000000m, 10000));
        yield return WeightedCase("คะแนนและ max เป็นเศษส่วน", true, 25m,
            Score(1, 0.125m, 0.5m, 10000));
    }

    public static IEnumerable<object?[]> WeightedInvalidCases()
    {
        yield return new object?[] { "null", null, true, "POLICY_NOT_CONFIGURED" };
        yield return new object?[]
        {
            "ไม่มี activity", Array.Empty<WeightedActivityScore>(), true, "POLICY_NOT_CONFIGURED"
        };
        yield return InvalidWeightedCase("ActivityId ศูนย์", Score(0, 1, 1, 10000), "SCORING_WEIGHT_INVALID");
        yield return InvalidWeightedCase("ActivityId ติดลบ", Score(-1, 1, 1, 10000), "SCORING_WEIGHT_INVALID");
        yield return new object?[]
        {
            "ActivityId ซ้ำ", new[] { Score(1, 1, 1, 5000), Score(1, 1, 1, 5000) }, true, "SCORING_WEIGHT_INVALID"
        };
        yield return InvalidWeightedCase("น้ำหนักติดลบ", Score(1, 1, 1, -1), "SCORING_WEIGHT_INVALID");
        yield return InvalidWeightedCase("น้ำหนักเกิน 10000", Score(1, 1, 1, 10001), "SCORING_WEIGHT_INVALID");
        yield return InvalidWeightedCase("น้ำหนัก int ต่ำสุด", Score(1, 1, 1, int.MinValue), "SCORING_WEIGHT_INVALID");
        yield return InvalidWeightedCase("น้ำหนัก int สูงสุด", Score(1, 1, 1, int.MaxValue), "SCORING_WEIGHT_INVALID");
        yield return InvalidWeightedCase("น้ำหนักรวมศูนย์", Score(1, 1, 1, 0), "SCORING_WEIGHT_INVALID");
        yield return InvalidWeightedCase("ไม่ปรับน้ำหนักรวม 9999 ใหม่", Score(1, 1, 1, 9999), "SCORING_WEIGHT_INVALID");
        yield return new object?[]
        {
            "น้ำหนักรวมเกิน", new[] { Score(1, 1, 1, 5001), Score(2, 1, 1, 5000) }, true, "SCORING_WEIGHT_INVALID"
        };
        yield return new object?[]
        {
            "quiz ไม่นับต้องมีน้ำหนักศูนย์",
            new[] { Score(1, 1, 1, 9999), Score(2, 1, 1, 1, true) },
            false, "SCORING_WEIGHT_INVALID"
        };
        yield return InvalidWeightedCase("max ศูนย์", Score(1, 0, 0, 10000), "POLICY_NOT_CONFIGURED");
        yield return InvalidWeightedCase("max ติดลบ", Score(1, 0, -1, 10000), "POLICY_NOT_CONFIGURED");
        yield return InvalidWeightedCase(
            "earned ติดลบ", Score(1, -0.0000000000000000000000000001m, 1, 10000), "POLICY_NOT_CONFIGURED");
        yield return InvalidWeightedCase(
            "earned เกิน max", Score(1, 1.0000000000000000000000000001m, 1, 10000), "POLICY_NOT_CONFIGURED");
        yield return InvalidWeightedCase(
            "earned decimal ต่ำสุด", Score(1, decimal.MinValue, 1, 10000), "POLICY_NOT_CONFIGURED");
        yield return new object?[]
        {
            "ศูนย์น้ำหนักยังตรวจ max", new[] { Score(1, 1, 1, 10000), Score(2, 0, 0, 0) }, true, "POLICY_NOT_CONFIGURED"
        };
        yield return new object?[]
        {
            "quiz ไม่นับยังตรวจคะแนน", new[] { Score(1, 1, 1, 10000), Score(2, 2, 1, 0, true) }, false, "POLICY_NOT_CONFIGURED"
        };
    }

    public static IEnumerable<object[]> ConversionValidCases()
    {
        yield return ConversionCase("8 จาก 10 เป็น 20", 8, 10, 20, 16, 16);
        yield return ConversionCase("3 จาก 4 เป็น 10", 3, 4, 10, 7.5m, 8);
        yield return ConversionCase("3 จาก 4 เป็น 12", 3, 4, 12, 9, 9);
        yield return ConversionCase("7.4 ปัดลง", 74, 100, 10, 7.4m, 7);
        yield return ConversionCase("ศูนย์", 0, 10, 20, 0, 0);
        yield return ConversionCase("เต็ม", 10, 10, 20, 20, 20);
        yield return ConversionCase("half-up ที่ 0.5", 1, 2, 1, 0.5m, 1);
        yield return ConversionCase("half-up ที่ 2.5", 1, 4, 10, 2.5m, 3);
        yield return ConversionCase("ไม่ปัดสองครั้งต่ำกว่า midpoint", 249, 1000, 10, 2.49m, 2);
        yield return ConversionCase("สูงกว่า midpoint", 251, 1000, 10, 2.51m, 3);
        yield return ConversionCase(
            "ต่ำกว่า midpoint หนึ่งหน่วย decimal", 0.4999999999999999999999999999m, 1, 1, 0.4999999999999999999999999999m, 0);
        yield return ConversionCase(
            "สูงกว่า midpoint หนึ่งหน่วย decimal", 0.5000000000000000000000000001m, 1, 1, 0.5000000000000000000000000001m, 1);
        yield return ConversionCase("target เป็นเศษส่วน", 1, 1, 2.5m, 2.5m, 3);
        yield return ConversionCase("ไม่บังคับผลเป็น int32", 1, 1, 2147483648m, 2147483648m, 2147483648m);
        yield return ConversionCase(
            "target decimal สูงสุด", 1, 1, decimal.MaxValue, decimal.MaxValue, decimal.MaxValue);
        yield return ConversionCase("earned และ max decimal สูงสุด", decimal.MaxValue, decimal.MaxValue, 10, 10, 10);
        yield return ConversionCase(
            "target เล็กสุด", 1, 1, 0.0000000000000000000000000001m, 0.0000000000000000000000000001m, 0);
        yield return ConversionCase(
            "max เล็กสุด", 0.0000000000000000000000000001m, 0.0000000000000000000000000001m, 10, 10, 10);
        yield return ConversionCase("ทศนิยมวนตามลำดับ decimal", 1, 3, 10, 3.333333333333333333333333333m, 3);
    }

    public static IEnumerable<object[]> ConversionInvalidCases()
    {
        yield return InvalidConversionCase("target ศูนย์", 1, 1, 0, "TARGET_NOT_READY");
        yield return InvalidConversionCase("target ติดลบ", 1, 1, -1, "TARGET_NOT_READY");
        yield return InvalidConversionCase("target decimal ต่ำสุด", 1, 1, decimal.MinValue, "TARGET_NOT_READY");
        yield return InvalidConversionCase("max ศูนย์", 0, 0, 10, "POLICY_NOT_CONFIGURED");
        yield return InvalidConversionCase("max ติดลบ", 0, -1, 10, "POLICY_NOT_CONFIGURED");
        yield return InvalidConversionCase("max decimal ต่ำสุด", 0, decimal.MinValue, 10, "POLICY_NOT_CONFIGURED");
        yield return InvalidConversionCase("earned ติดลบ", -1, 10, 10, "POLICY_NOT_CONFIGURED");
        yield return InvalidConversionCase("earned เกิน max", 11, 10, 10, "POLICY_NOT_CONFIGURED");
        yield return InvalidConversionCase("earned decimal ต่ำสุด", decimal.MinValue, 1, 10, "POLICY_NOT_CONFIGURED");
        yield return InvalidConversionCase(
            "earned เกินหนึ่งหน่วย decimal", 1.0000000000000000000000000001m, 1, 10, "POLICY_NOT_CONFIGURED");
    }

    [Theory]
    [MemberData(nameof(WeightedValidCases))]
    public void WeightedPreservesContract(
        string behavior, WeightedActivityScore[] scores, bool quizCounts, decimal expected)
    {
        Assert.True(expected == WeightedScoreCalculator.Calculate(scores, quizCounts), behavior);
    }

    [Theory]
    [MemberData(nameof(WeightedInvalidCases))]
    public void WeightedRejectsInvalidInput(
        string behavior, WeightedActivityScore[]? scores, bool quizCounts, string code)
    {
        var error = Assert.Throws<DomainRuleException>(() => WeightedScoreCalculator.Calculate(scores, quizCounts));
        Assert.True(code == error.Code, behavior);
    }

    [Theory]
    [MemberData(nameof(ConversionValidCases))]
    public void ConversionPreservesContract(
        string behavior, decimal earned, decimal max, decimal target, decimal raw, decimal rounded)
    {
        var actual = ScoreConversionCalculator.Calculate(earned, max, target);
        Assert.True(raw == actual.RawTarget, behavior);
        Assert.True(rounded == actual.RoundedScore, behavior);
    }

    [Theory]
    [MemberData(nameof(ConversionInvalidCases))]
    public void ConversionRejectsInvalidInput(
        string behavior, decimal earned, decimal max, decimal target, string code)
    {
        var error = Assert.Throws<DomainRuleException>(() => ScoreConversionCalculator.Calculate(earned, max, target));
        Assert.True(code == error.Code, behavior);
    }

    [Theory]
    [InlineData(0, 6, 6, 1)]
    [InlineData(1, 6, 6, 2)]
    [InlineData(2, 6, 6, 3)]
    public void ConversionKeepsExactHalfUp(int whole, int max, int target, int rounded)
    {
        var earned = whole + 0.5m;
        var actual = ScoreConversionCalculator.Calculate(earned, max, target);
        Console.WriteLine($"half_up earned={earned} max={max} target={target} raw={actual.RawTarget} rounded={actual.RoundedScore}");
        Assert.Equal((decimal)rounded, actual.RoundedScore);
    }

    [Fact]
    public void ConversionKeepsExactHalfUpAtNumericBoundary()
    {
        var actual = ScoreConversionCalculator.Calculate(368276.5m, 366218635856m, 366218635856m);
        Console.WriteLine($"half_up raw={actual.RawTarget} rounded={actual.RoundedScore}");
        Assert.Equal(368277m, actual.RoundedScore);
    }
    private static object[] WeightedCase(
        string behavior, bool quizCounts, decimal expected, params WeightedActivityScore[] scores) =>
        new object[] { behavior, scores, quizCounts, expected };

    private static WeightedActivityScore Score(long id, decimal earned, decimal max, int weight, bool quiz = false) =>
        new(id, earned, max, weight, quiz);

    private static object?[] InvalidWeightedCase(string behavior, WeightedActivityScore score, string code) =>
        new object?[] { behavior, new[] { score }, true, code };

    private static object[] ConversionCase(
        string behavior, decimal earned, decimal max, decimal target, decimal raw, decimal rounded) =>
        new object[] { behavior, earned, max, target, raw, rounded };

    private static object[] InvalidConversionCase(
        string behavior, decimal earned, decimal max, decimal target, string code) =>
        new object[] { behavior, earned, max, target, code };
}