using System.Text.Json;
using ServiceSchoolBot.Domain;

namespace ServiceSchoolBot.Application.Services.Implementations.Projects;

public sealed partial class ProjectService
{
    private static void EnsureExactProperties(JsonElement obj, params string[] allowed)
    {
        var set = allowed.ToHashSet(StringComparer.Ordinal);
        var count = 0;
        foreach (var prop in obj.EnumerateObject())
        {
            if (!set.Contains(prop.Name) || prop.Value.ValueKind == JsonValueKind.Null)
                throw new DomainRuleException(ErrorCodes.InvalidInput, $"Unexpected or null property '{prop.Name}'.");
            count++;
        }
        if (count != allowed.Length)
            throw new DomainRuleException(ErrorCodes.InvalidInput, "Missing required content properties.");
    }

    private static void ValidateCodeImportContent(JsonElement root)
    {
        EnsureExactProperties(root, "kind", "source");
        var src = root.GetProperty("source");
        if (src.ValueKind != JsonValueKind.String)
            throw new DomainRuleException(ErrorCodes.InvalidInput, "code source must be a string.");
        if (src.GetString()!.Length > 16384)
            throw new DomainRuleException(ErrorCodes.PayloadTooLarge, "code source exceeds 16384 UTF-16 code units.");
    }

    private static void ValidateBlockImportContent(JsonElement root)
    {
        EnsureExactProperties(root, "kind", "stacks");
        var stacks = root.GetProperty("stacks");
        if (stacks.ValueKind != JsonValueKind.Array)
            throw new DomainRuleException(ErrorCodes.InvalidInput, "block stacks must be an array.");

        var seenIds = new HashSet<string>(StringComparer.Ordinal);
        var totalBlocks = 0;
        foreach (var stack in stacks.EnumerateArray())
        {
            if (stack.ValueKind != JsonValueKind.Object)
                throw new DomainRuleException(ErrorCodes.InvalidInput, "stack must be an object.");
            EnsureExactProperties(stack, "x", "y", "blocks");
            if (stack.GetProperty("x").ValueKind != JsonValueKind.Number ||
                stack.GetProperty("y").ValueKind != JsonValueKind.Number)
                throw new DomainRuleException(ErrorCodes.InvalidInput, "stack coordinates must be numbers.");
            var blocks = stack.GetProperty("blocks");
            if (blocks.ValueKind != JsonValueKind.Array)
                throw new DomainRuleException(ErrorCodes.InvalidInput, "stack blocks must be an array.");
            foreach (var blk in blocks.EnumerateArray())
                ValidateBlockElement(blk, seenIds, ref totalBlocks, 0);
        }
    }

    private static void ValidateBlockElement(JsonElement blk, HashSet<string> seenIds, ref int total, int depth)
    {
        if (blk.ValueKind != JsonValueKind.Object)
            throw new DomainRuleException(ErrorCodes.InvalidInput, "block must be an object.");
        total++;
        if (total > 1000)
            throw new DomainRuleException(ErrorCodes.PayloadTooLarge, "block count exceeds 1000.");
        if (depth > 8)
            throw new DomainRuleException(ErrorCodes.InvalidInput, "block nesting depth exceeds 8.");

        if (!blk.TryGetProperty("id", out var idProp) || idProp.ValueKind != JsonValueKind.String ||
            !Guid.TryParseExact(idProp.GetString(), "D", out _) ||
            !seenIds.Add(idProp.GetString()!))
            throw new DomainRuleException(ErrorCodes.InvalidInput, "block id must be a unique canonical UUID.");
        if (!blk.TryGetProperty("enabled", out var enProp) ||
            enProp.ValueKind is not (JsonValueKind.True or JsonValueKind.False))
            throw new DomainRuleException(ErrorCodes.InvalidInput, "block enabled must be a boolean.");
        if (!blk.TryGetProperty("type", out var typeProp) || typeProp.ValueKind != JsonValueKind.String)
            throw new DomainRuleException(ErrorCodes.InvalidInput, "block type must be a string.");

        switch (typeProp.GetString())
        {
            case "move":
                EnsureExactProperties(blk, "id", "enabled", "type", "direction", "tiles");
                var dir = blk.GetProperty("direction").GetString();
                if (dir is not ("move_right" or "move_left"))
                    throw new DomainRuleException(ErrorCodes.InvalidInput, "invalid move direction.");
                if (!blk.GetProperty("tiles").TryGetInt32(out var tiles) || tiles is < 1 or > 10)
                    throw new DomainRuleException(ErrorCodes.InvalidInput, "move tiles must be 1..10.");
                break;
            case "jump":
                EnsureExactProperties(blk, "id", "enabled", "type");
                break;
            case "repeat":
                EnsureExactProperties(blk, "id", "enabled", "type", "times", "body");
                if (!blk.GetProperty("times").TryGetInt32(out var times) || times is < 1 or > 10)
                    throw new DomainRuleException(ErrorCodes.InvalidInput, "repeat times must be 1..10.");
                var body = blk.GetProperty("body");
                if (body.ValueKind != JsonValueKind.Array)
                    throw new DomainRuleException(ErrorCodes.InvalidInput, "repeat body must be an array.");
                foreach (var child in body.EnumerateArray())
                    ValidateBlockElement(child, seenIds, ref total, depth + 1);
                break;
            default:
                throw new DomainRuleException(ErrorCodes.InvalidInput, "unsupported block type.");
        }
    }

    private static void ValidateArmImportContent(JsonElement root)
    {
        EnsureExactProperties(root, "kind", "poses", "order", "groups", "joint_names", "time_mode", "loops", "view");
        var timeMode = root.GetProperty("time_mode").GetString();
        if (timeMode is not ("same" or "split"))
            throw new DomainRuleException(ErrorCodes.InvalidInput, "time_mode must be 'same' or 'split'.");
        var view = root.GetProperty("view").GetString();
        if (view is not ("cards" or "table"))
            throw new DomainRuleException(ErrorCodes.InvalidInput, "view must be 'cards' or 'table'.");
        if (!root.GetProperty("loops").TryGetInt32(out var loops) || loops is < 1 or > 9999)
            throw new DomainRuleException(ErrorCodes.InvalidInput, "loops must be 1..9999.");

        var jnames = root.GetProperty("joint_names");
        if (jnames.ValueKind != JsonValueKind.Array || jnames.GetArrayLength() != 6)
            throw new DomainRuleException(ErrorCodes.InvalidInput, "joint_names must contain 6 strings.");
        foreach (var jn in jnames.EnumerateArray())
        {
            if (jn.ValueKind != JsonValueKind.String || jn.GetString()!.Length is < 1 or > 200)
                throw new DomainRuleException(ErrorCodes.InvalidInput, "joint name must be 1..200 chars.");
        }

        var poses = root.GetProperty("poses");
        if (poses.ValueKind != JsonValueKind.Array)
            throw new DomainRuleException(ErrorCodes.InvalidInput, "poses must be an array.");
        var poseIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (var p in poses.EnumerateArray())
        {
            if (p.ValueKind != JsonValueKind.Object)
                throw new DomainRuleException(ErrorCodes.InvalidInput, "pose must be an object.");
            EnsureExactProperties(p, "id", "name", "values", "duration_ms", "joint_duration_ms");
            var pid = p.GetProperty("id").GetString();
            if (pid is null || !Guid.TryParseExact(pid, "D", out _) || !poseIds.Add(pid))
                throw new DomainRuleException(ErrorCodes.InvalidInput, "pose id must be a unique canonical UUID.");
            var pname = p.GetProperty("name").GetString();
            if (string.IsNullOrEmpty(pname) || pname.Length > 200)
                throw new DomainRuleException(ErrorCodes.InvalidInput, "pose name must be 1..200 chars.");
            if (!p.GetProperty("duration_ms").TryGetInt32(out var dur) || dur is < 1000 or > 9000)
                throw new DomainRuleException(ErrorCodes.InvalidInput, "pose duration_ms must be 1000..9000.");

            var vals = p.GetProperty("values");
            if (vals.ValueKind != JsonValueKind.Array || vals.GetArrayLength() != 6)
                throw new DomainRuleException(ErrorCodes.InvalidInput, "pose values must have 6 numbers.");
            var vArr = vals.EnumerateArray().Select(x => x.GetDouble()).ToArray();
            if (vArr[0] is < -180 or > 180 ||
                vArr[1] is < -135 or > 135 || vArr[2] is < -135 or > 135 ||
                vArr[3] is < -135 or > 135 || vArr[4] is < -135 or > 135 ||
                vArr[5] is < 0 or > 100)
                throw new DomainRuleException(ErrorCodes.InvalidInput, "pose joint values out of bounds.");

            var jdur = p.GetProperty("joint_duration_ms");
            if (jdur.ValueKind != JsonValueKind.Array || jdur.GetArrayLength() != 6)
                throw new DomainRuleException(ErrorCodes.InvalidInput, "pose joint_duration_ms must have 6 integers.");
            foreach (var d in jdur.EnumerateArray())
            {
                if (!d.TryGetInt32(out var dv) || dv is < 1000 or > 9000)
                    throw new DomainRuleException(ErrorCodes.InvalidInput, "joint_duration_ms out of bounds.");
            }
        }

        var order = root.GetProperty("order");
        if (order.ValueKind != JsonValueKind.Array || order.GetArrayLength() != poseIds.Count)
            throw new DomainRuleException(ErrorCodes.InvalidInput, "order must reference all poses once.");
        var orderSeen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var oid in order.EnumerateArray())
        {
            var s = oid.GetString();
            if (s is null || !poseIds.Contains(s) || !orderSeen.Add(s))
                throw new DomainRuleException(ErrorCodes.InvalidInput, "invalid pose id in order.");
        }

        var groups = root.GetProperty("groups");
        if (groups.ValueKind != JsonValueKind.Array)
            throw new DomainRuleException(ErrorCodes.InvalidInput, "groups must be an array.");
        var groupIds = new HashSet<string>(StringComparer.Ordinal);
        var groupNames = new HashSet<string>(StringComparer.Ordinal);
        foreach (var g in groups.EnumerateArray())
        {
            if (g.ValueKind != JsonValueKind.Object)
                throw new DomainRuleException(ErrorCodes.InvalidInput, "group must be an object.");
            EnsureExactProperties(g, "id", "name", "pose_ids");
            var gid = g.GetProperty("id").GetString();
            if (gid is null || !Guid.TryParseExact(gid, "D", out _) || !groupIds.Add(gid))
                throw new DomainRuleException(ErrorCodes.InvalidInput, "group id must be a unique canonical UUID.");
            var gname = g.GetProperty("name").GetString();
            if (string.IsNullOrEmpty(gname) || gname.Length > 20 || !groupNames.Add(gname))
                throw new DomainRuleException(ErrorCodes.InvalidInput, "group name must be unique and 1..20 chars.");
            var pids = g.GetProperty("pose_ids");
            if (pids.ValueKind != JsonValueKind.Array || pids.GetArrayLength() < 2)
                throw new DomainRuleException(ErrorCodes.InvalidInput, "group pose_ids must contain >= 2 pose IDs.");
            foreach (var pid in pids.EnumerateArray())
            {
                if (pid.GetString() is not { } ps || !orderSeen.Contains(ps))
                    throw new DomainRuleException(ErrorCodes.InvalidInput, "group references unknown pose id.");
            }
        }
    }
}
