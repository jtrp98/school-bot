#include "request.hpp"
#include "validation.hpp"
#include <cstdio>
#include <set>

namespace zide {
namespace {
json supported_actions();

// Project failures are reported as PROJECT_INVALID; library exception text never reaches the response.
template <typename Work>
auto as_project_work(Work&& work) -> decltype(work()) {
    try {
        return work();
    } catch (const json::exception&) {
        throw ActionError("PROJECT_INVALID", "project data has an unsupported shape");
    } catch (const std::exception& error) {
        throw ActionError("PROJECT_INVALID", error.what());
    }
}

void plan_engine_info(Plan& plan) {
    plan.result["actions"] = supported_actions();
    plan.result["hardware_platforms"] = {"windows"};
    plan.result["limits"] = {{"servos_per_group", 32}, {"pwm_min", 500}, {"pwm_max", 2500},
                             {"time_ms_max", 9999}, {"command_bytes", 4096}};
}

void plan_device_list(Plan& plan) {
    plan.result["devices"] = json::array();
    try {
        for (const auto& device : zide::list_devices()) {
            plan.result["devices"].push_back(
                {{"transport", device.transport}, {"path", device.path}, {"name", device.name}});
        }
    } catch (const std::exception& error) {
        throw ActionError("TRANSPORT_ERROR", error.what());
    }
}

void plan_servo_set(const json& data, Plan& plan) {
    int servo_id = 0;
    int pwm = 0;
    if (data.contains("servo") || data.contains("angle")) {
        require(!data.contains("id") && !data.contains("pwm"), "use servo+angle or id+pwm");
        servo_id = integer(data, "servo", 1, 4);
        const int angle = integer(data, "angle", 0, 180);
        pwm = 500 + (angle * 2000 + 90) / 180;
        plan.result["servo"] = servo_id;
        plan.result["angle"] = angle;
    } else {
        servo_id = integer(data, "id", 0, 254);
        pwm = integer(data, "pwm", 500, 2500);
    }
    const int duration = integer(data, "time_ms", 0, 9999, 1000);
    const int bias = integer(data, "bias", -500, 500, 0);
    plan.commands.push_back(zide::servo_command(servo_id, pwm, duration, bias));
    plan.result["id"] = servo_id;
    plan.result["pwm"] = pwm + bias;
    plan.result["time_ms"] = duration;
}

void plan_servo_read(const json& data, Plan& plan) {
    const int servo_id = integer(data, "id", 0, 254);
    char command[32];
    std::snprintf(command, sizeof(command), "#%03dPRAD!", servo_id);
    plan.commands.emplace_back(command);
}

void plan_servo_torque(const json& data, Plan& plan) {
    require(data.contains("enabled") && data["enabled"].is_boolean(), "enabled boolean required");
    const int servo_id = integer(data, "id", 0, 255, 255);
    const char* verb = data["enabled"].get<bool>() ? "PSCK" : "PULK";
    char command[32];
    std::snprintf(command, sizeof(command), "#%03d%s!", servo_id, verb);
    plan.commands.emplace_back(command);
}

void plan_groups(const std::string& action, const json& data, Plan& plan) {
    require(!(data.contains("group") && data.contains("groups")), "use group or groups");
    json groups = data.contains("group") ? json::array({data["group"]}) : data.value("groups", json::array());
    require(groups.is_array() && !groups.empty() && groups.size() <= 8, "use 1..8 groups per download");
    require(action != "group.execute" || groups.size() == 1, "execute accepts one group");
    std::set<int> indices;
    for (const auto& group : groups) {
        const auto command = zide::group_command(group, action == "group.download");
        require(indices.insert(group["index"].get<int>()).second, "duplicate group index");
        plan.commands.push_back(command);
    }
}

void plan_group_range(const std::string& action, const json& data, Plan& plan, bool simulated) {
    const int start = integer(data, "start", 0, 9999);
    const int end = integer(data, "end", start, 9999);
    const std::string range = std::to_string(start) + "-" + std::to_string(end);
    if (action == "group.read") {
        plan.commands.push_back("$PTG:" + range + "!");
        return;
    }
    const int times = integer(data, "times", 1, 9999);
    plan.commands.push_back("$DGT:" + range + "," + std::to_string(times) + "!");
    plan.result["runs_on_device"] = !simulated;
}

void plan_robot_stop(const json&, Plan& plan, bool) {
    plan.commands.emplace_back("$DST!");
    plan.result["stop_requested"] = true;
}

void plan_board_info(const json&, Plan& plan, bool) {
    plan.commands.emplace_back("$DRS!");
}

void plan_project_import(const json& data, Plan& plan, bool) {
    require(data.contains("content") && data["content"].is_string(), "content must be a string");
    const std::string content = data["content"].get<std::string>();
    plan.result["groups"] = as_project_work([&] { return import_groups(content); });
}

void plan_project_export(const json& data, Plan& plan, bool) {
    require(data.contains("groups"), "groups is required");
    plan.result["content"] = as_project_work([&] { return export_groups(data["groups"]); });
}

using Handler = void (*)(const json&, Plan&, bool);
struct Action {
    const char* name;
    bool device_transport;
    Handler handler;
};

const Action actions[] = {
    {"engine.info", false, [](const json&, Plan& plan, bool) { plan_engine_info(plan); }},
    {"device.list", false, [](const json&, Plan& plan, bool) { plan_device_list(plan); }},
    {"servo.set", true, [](const json& data, Plan& plan, bool) { plan_servo_set(data, plan); }},
    {"servo.read", true, [](const json& data, Plan& plan, bool) { plan_servo_read(data, plan); }},
    {"servo.torque", true, [](const json& data, Plan& plan, bool) { plan_servo_torque(data, plan); }},
    {"robot.stop", true, plan_robot_stop},
    {"board.info", true, plan_board_info},
    {"group.execute", true, [](const json& data, Plan& plan, bool) { plan_groups("group.execute", data, plan); }},
    {"group.download", true, [](const json& data, Plan& plan, bool) { plan_groups("group.download", data, plan); }},
    {"group.run", true, [](const json& data, Plan& plan, bool simulated) {
        plan_group_range("group.run", data, plan, simulated);
    }},
    {"group.read", true, [](const json& data, Plan& plan, bool simulated) {
        plan_group_range("group.read", data, plan, simulated);
    }},
    {"project.import", false, plan_project_import},
    {"project.export", false, plan_project_export},
};

const Action& find_action(const std::string& name) {
    for (const auto& action : actions) {
        if (name == action.name) return action;
    }
    throw ActionError("ACTION_UNKNOWN", "unknown Zide action: " + name);
}

json supported_actions() {
    json result = json::array();
    for (const auto& action : actions) result.push_back(action.name);
    return result;
}
}

bool uses_device_transport(const std::string& action) {
    return find_action(action).device_transport;
}

void plan_action(const std::string& action, const json& data, Plan& plan, bool simulated) {
    find_action(action).handler(data, plan, simulated);
}
}
