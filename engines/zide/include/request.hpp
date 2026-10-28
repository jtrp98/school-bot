#pragma once

#include "protocol.hpp"
#include "transport.hpp"
#include <utility>

namespace zide {
struct ActionError : std::runtime_error {
    ActionError(std::string error_code, const std::string& message)
        : std::runtime_error(message), code(std::move(error_code)) {}
    std::string code;
};

struct Plan {
    json result;
    std::vector<std::string> commands;
};

bool uses_device_transport(const std::string& action);
void plan_action(const std::string& action, const json& data, Plan& plan, bool simulated);
json error_response(const std::string& id, const std::string& code, const std::string& message);
json handle_request(const json& request);
}
