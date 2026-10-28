#include "request.hpp"
#include "validation.hpp"
#include <set>

namespace zide {
namespace {
constexpr std::size_t kMaxRequestIdBytes = 128;

void require_protocol(bool condition, const std::string& message) {
    if (!condition) throw ActionError("PROTOCOL_INVALID", message);
}

std::string text(const json& data, const char* key, const std::string& fallback) {
    if (!data.contains(key)) return fallback;
    require(data[key].is_string(), std::string(key) + " must be a string");
    return data[key].get<std::string>();
}

std::string string_or_empty(const json& object, const char* key) {
    const auto found = object.find(key);
    if (found == object.end() || !found->is_string()) return {};
    return found->get<std::string>();
}

// An id outside the contract is not echoed, so every response still matches the response schema.
std::string request_id_of(const json& request) {
    if (!request.is_object()) return {};
    const std::string id = string_or_empty(request, "request_id");
    return id.size() <= kMaxRequestIdBytes ? id : std::string();
}

std::string validate_envelope(const json& request, const std::string& id) {
    require_protocol(request.is_object(), "request must be an object");
    static const std::set<std::string> envelope = {"protocol", "request_id", "mode", "action", "data"};
    for (const auto& field : request.items()) {
        require_protocol(envelope.count(field.key()) != 0, "unknown request field: " + field.key());
    }
    require_protocol(string_or_empty(request, "protocol") == "1.0" && !id.empty(),
                     "expected protocol 1.0 and request_id of 1..128 bytes");
    require_protocol(string_or_empty(request, "mode") == "robot-control", "zide-engine supports robot-control");
    require_protocol(request.contains("action") && request["action"].is_string(), "action required");
    return request["action"].get<std::string>();
}

void validate_data_fields(const json& data) {
    static const std::set<std::string> keys = {
        "transport", "port", "baud", "device", "read_ms", "id", "pwm", "time_ms", "bias", "servo", "angle",
        "enabled", "group", "groups", "start", "end", "times", "content"};
    for (const auto& field : data.items()) {
        require(keys.count(field.key()) != 0, "unknown data field: " + field.key());
    }
}

zide::TransportOptions parse_options(const json& data) {
    static const std::set<int> bauds = {
        9600, 19200, 38400, 57600, 115200, 128000, 230400, 256000, 460800, 512000, 921600};
    zide::TransportOptions options;
    options.transport = text(data, "transport", "dry-run");
    options.port = text(data, "port", "");
    options.device = text(data, "device", "");
    options.baud = integer(data, "baud", 9600, 921600, 115200);
    require(bauds.count(options.baud) != 0, "unsupported serial baud");
    options.read_ms = integer(data, "read_ms", 1, 1500, 1000);
    const bool known = options.transport == "dry-run" || options.transport == "hid" || options.transport == "serial";
    require(known, "transport must be dry-run, hid or serial");
    require(options.transport != "serial" || !options.port.empty(), "serial requires port");
    require(options.device.empty() || options.transport == "hid", "device is only valid for hid");
    require(options.port.empty() || options.transport == "serial", "port is only valid for serial");
    return options;
}

std::string hex_of(const std::string& bytes) {
    static constexpr char digits[] = "0123456789abcdef";
    std::string out;
    out.reserve(bytes.size() * 2);
    for (unsigned char byte : bytes) {
        out.push_back(digits[byte >> 4]);
        out.push_back(digits[byte & 15]);
    }
    return out;
}

bool expects_reply(const std::string& action) {
    return action == "servo.read" || action == "board.info" || action == "group.read";
}

void deliver(const std::string& action, const zide::TransportOptions& options, Plan& plan) {
    const bool simulated = options.transport == "dry-run";
    // Encode every command first so invalid later commands cannot cause partial hardware writes.
    json frames;
    if (simulated) frames = json::array();
    for (const auto& command : plan.commands) {
        const auto encoded = zide::encode_hid(command);
        if (simulated) {
            for (const auto& frame : encoded) frames.push_back(frame);
        }
    }
    plan.result["commands"] = plan.commands;
    if (simulated) {
        plan.result["hid_reports"] = frames;
        plan.result["delivery"] = "dry-run";
        return;
    }
    zide::ExchangeResult reply;
    try {
        reply = zide::exchange(options, plan.commands);
    } catch (const std::exception& error) {
        throw ActionError("TRANSPORT_ERROR",
                          std::string(error.what()) + "; delivery may be partial; do not automatically retry movement");
    }
    plan.result["exchange"] = {{"received", reply.received}, {"received_hex", hex_of(reply.received)},
                               {"bytes_written", reply.bytes_written}, {"bytes_read", reply.bytes_read}};
    plan.result["delivery"] = "written";
    plan.result["acknowledged"] = false;
    if (expects_reply(action) && reply.bytes_read == 0) {
        throw ActionError("DEVICE_TIMEOUT", "command written but device did not respond within read_ms");
    }
}

}

json error_response(const std::string& id, const std::string& code, const std::string& message) {
    return {{"protocol", "1.0"}, {"request_id", id}, {"status", "error"},
            {"error", {{"code", code}, {"message", message}}}};
}

json handle_request(const json& request) {
    const std::string id = request_id_of(request);
    try {
        const std::string action = validate_envelope(request, id);
        require(request.contains("data") && request["data"].is_object(), "data must be an object");
        const auto& data = request["data"];
        validate_data_fields(data);
        TransportOptions options;
        const bool device_action = uses_device_transport(action);
        if (device_action) {
            options = parse_options(data);
        } else {
            for (const auto* key : {"transport", "device", "port", "baud", "read_ms"}) {
                require(!data.contains(key), std::string(key) + " is only valid for device commands");
            }
            options.transport = "none";
        }
        const bool simulated = device_action && options.transport == "dry-run";
        Plan plan;
        plan.result = {{"engine", "zide"}, {"language", "C++"}, {"simulated", simulated},
                       {"transport", options.transport}};
        plan_action(action, data, plan, simulated);
        if (!plan.commands.empty()) deliver(action, options, plan);
        return {{"protocol", "1.0"}, {"request_id", id}, {"status", "success"}, {"data", std::move(plan.result)}};
    } catch (const ActionError& error) {
        return error_response(id, error.code, error.what());
    } catch (const zide::SafetyLimitError& error) {
        return error_response(id, "SAFETY_LIMIT", error.what());
    } catch (const json::exception&) {
        return error_response(id, "INVALID_INPUT", "request contains a value of an unsupported type");
    } catch (const std::exception& error) {
        return error_response(id, "INVALID_INPUT", error.what());
    }
}

}
