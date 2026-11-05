using System.Text.Json;

namespace ServiceSchoolBot.Domain.Curriculum;

public static class ProjectActivityReference
{
    public static int ResolveMissionId(string config, IReadOnlyList<SchoolBotMissionRecord> missions)
    {
        using var json = JsonDocument.Parse(config);
        SchoolBotMissionRecord? mission;
        if (json.RootElement.TryGetProperty("mission_key", out var key))
            mission = missions.SingleOrDefault(m => !m.IsDelete && m.MissionKey == key.GetGuid());
        else if (json.RootElement.TryGetProperty("mission_id", out var id))
            mission = missions.SingleOrDefault(m => !m.IsDelete && m.SchoolBotMissionId == id.GetInt32());
        else mission = null;
        return mission?.SchoolBotMissionId ?? throw new DomainRuleException(ErrorCodes.StateConflict, "Project activity references a missing mission in its lesson.");
    }
}
