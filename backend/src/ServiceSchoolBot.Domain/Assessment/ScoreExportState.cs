namespace ServiceSchoolBot.Domain.Assessment;

public static class ScoreExportState
{
    public static string Resolve(IReadOnlyList<ScoreOutboxRecord> items, bool hasSkippedCells) =>
        Resolve(items.Select(i => i.State).ToArray(), hasSkippedCells,
            items.Any(i => i.State == "blocked" && i.LastErrorCode == "SUPERSEDED_BY_PREVIEW"));

    public static string Resolve(IReadOnlyList<string> states, bool hasSkippedCells, bool hasSupersededItems = false)
    {
        if (states.Contains("conflict")) return "conflict";
        if (states.Count > 0 && hasSupersededItems && states.All(s => s is "applied" or "skipped" or "blocked")) return "superseded";
        if (states.Count == 0 || states.Any(s => s is not ("applied" or "skipped"))) return "queued";
        return hasSkippedCells || states.Contains("skipped") ? "completed_with_skips" : "applied";
    }
}
