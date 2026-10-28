#pragma once

#include <array>
#include <cstddef>
#include <stdexcept>
#include <string>
#include <vector>
#include <nlohmann/json.hpp>

namespace zide {
using json = nlohmann::json;

// Separates a well-formed command that would drive a servo past its PWM limits from malformed input.
struct SafetyLimitError : std::runtime_error {
    using std::runtime_error::runtime_error;
};

std::vector<std::array<unsigned char, 64>> encode_hid(const std::string& command);
std::string decode_hid(const unsigned char* report, std::size_t size);
std::string servo_command(int id, int pwm, int time_ms, int bias = 0);
std::string group_command(const json& group, bool download);

json import_groups(const std::string& content);
std::string export_groups(const json& groups);
}
