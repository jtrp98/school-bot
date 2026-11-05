using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace ServiceSchoolBot.Domain.Assessment;

public static partial class LearningProgramEvaluator
{
    public const string Version = "bounded-program-v1";

    [GeneratedRegex(@"^hero\.(move_right|move_left|jump)\(([0-9]*)\)$", RegexOptions.CultureInvariant)]
    private static partial Regex Instruction();

    public static IReadOnlyList<LearningCommand> Compile(string mode, string content)
    {
        try
        {
            using var document = JsonDocument.Parse(content, new JsonDocumentOptions { MaxDepth = 32 });
            var root = document.RootElement;
            var commands = new List<LearningCommand>();
            if (mode == "code" && root.GetProperty("kind").GetString() == "code")
            {
                var source = root.GetProperty("source").GetString()!;
                if (Encoding.UTF8.GetByteCount(source) > 16384) Invalid();
                foreach (var raw in source.Split('\n'))
                {
                    var line = raw.Trim();
                    if (line.Length == 0 || line.StartsWith('#')) continue;
                    var match = Instruction().Match(line);
                    if (!match.Success) Invalid();
                    var op = match.Groups[1].Value;
                    var argument = match.Groups[2].Value;
                    if (op == "jump")
                    {
                        if (argument.Length != 0) Invalid();
                        commands.Add(new(op));
                    }
                    else
                    {
                        if (!int.TryParse(argument, out var amount) || amount is < 1 or > 10) Invalid();
                        commands.Add(new(op, amount));
                    }
                    Bound(commands);
                }
            }
            else if (mode == "block" && root.GetProperty("kind").GetString() == "block")
            {
                var seen = new HashSet<Guid>();
                var count = 0;
                foreach (var stack in root.GetProperty("stacks").EnumerateArray())
                    CompileBlocks(stack.GetProperty("blocks"), commands, seen, ref count, 0);
            }
            else Invalid();
            if (commands.Count == 0) Invalid();
            return commands;
        }
        catch (Exception ex) when (ex is JsonException or KeyNotFoundException or InvalidOperationException or FormatException)
        {
            throw new DomainRuleException("PROGRAM_INVALID", "Program does not match the supported bounded format.");
        }
    }

    private static void CompileBlocks(JsonElement blocks, List<LearningCommand> commands,
        HashSet<Guid> seen, ref int count, int depth)
    {
        if (depth > 8) Invalid();
        foreach (var block in blocks.EnumerateArray())
        {
            if (++count > 1000 || !seen.Add(block.GetProperty("id").GetGuid())) Invalid();
            var enabled = block.GetProperty("enabled").GetBoolean();
            var nested = new List<LearningCommand>();
            switch (block.GetProperty("type").GetString())
            {
                case "jump": nested.Add(new("jump")); break;
                case "move":
                    var op = block.GetProperty("direction").GetString();
                    var tiles = block.GetProperty("tiles").GetInt32();
                    if (op is not ("move_left" or "move_right") || tiles is < 1 or > 10) Invalid();
                    nested.Add(new(op!, tiles));
                    break;
                case "repeat":
                    var times = block.GetProperty("times").GetInt32();
                    if (times is < 1 or > 10) Invalid();
                    var body = new List<LearningCommand>();
                    CompileBlocks(block.GetProperty("body"), body, seen, ref count, depth + 1);
                    for (var i = 0; i < times; i++)
                    {
                        nested.AddRange(body);
                        Bound(nested);
                    }
                    break;
                default: Invalid(); break;
            }
            if (enabled) commands.AddRange(nested);
            Bound(commands);
        }
    }

    private static void Bound(IReadOnlyList<LearningCommand> commands)
    {
        if (commands.Count > 64 || commands.Sum(c => c.Value ?? 1) > 100) Invalid();
    }

    public static LearningEvaluation Evaluate(IReadOnlyList<LearningCommand> commands, string criteria)
    {
        try
        {
            using var document = JsonDocument.Parse(criteria);
            var root = document.RootElement;
            if (root.GetProperty("kind").GetString() != "automatic-v1") InvalidCriteria();
            var results = root.GetProperty("rules").EnumerateArray().Select(rule =>
            {
                var matched = rule.GetProperty("type").GetString() switch
                {
                    "uses_op" => commands.Count(c => c.Op == rule.GetProperty("op").GetString()) >=
                        rule.GetProperty("minimum").GetInt32(),
                    "steps_at_most" => commands.Sum(c => c.Value ?? 1) <= rule.GetProperty("maximum").GetInt32(),
                    "trace_equals" => commands.SequenceEqual(rule.GetProperty("commands").EnumerateArray()
                        .Select(c => new LearningCommand(c.GetProperty("op").GetString()!,
                            c.TryGetProperty("value", out var value) ? value.GetInt32() : null))),
                    _ => throw new DomainRuleException(ErrorCodes.GraderUnavailable, "Unsupported grading rule.")
                };
                return new LearningRuleResult(rule.GetProperty("id").GetGuid(), matched);
            }).ToArray();
            var combine = root.GetProperty("combine").GetString();
            if (results.Length is < 1 or > 64 || results.Select(r => r.RuleId).Distinct().Count() != results.Length ||
                combine is not ("all" or "any")) InvalidCriteria();
            var passed = combine == "all" ? results.All(r => r.Matched) : results.Any(r => r.Matched);
            return new(passed ? "passed" : "not_yet", results, Version);
        }
        catch (Exception ex) when (ex is JsonException or KeyNotFoundException or InvalidOperationException or FormatException)
        {
            throw new DomainRuleException(ErrorCodes.GraderUnavailable, "Approved grading rules are unavailable.");
        }
    }

    private static void Invalid() => throw new DomainRuleException("PROGRAM_INVALID", "Program exceeds supported syntax or bounds.");
    private static void InvalidCriteria() => throw new DomainRuleException(ErrorCodes.GraderUnavailable, "Approved grading rules are unavailable.");
}

