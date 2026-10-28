#include "protocol.hpp"
#include "validation.hpp"

#include <algorithm>
#include <cstdint>
#include <iomanip>
#include <initializer_list>
#include <regex>
#include <set>
#include <sstream>
#include <stdexcept>

namespace zide {
namespace {
std::string padded(int number, int width) {
    std::ostringstream out;
    out << std::setfill('0') << std::setw(width) << number;
    return out.str();
}

std::string trim(const std::string& value) {
    const auto start = value.find_first_not_of(" \t\r\n");
    if (start == std::string::npos) return {};
    return value.substr(start, value.find_last_not_of(" \t\r\n") - start + 1);
}

void keys(const json& value, std::initializer_list<const char*> allowed,
          const char* label) {
    if (!value.is_object()) invalid(std::string(label) + " must be an object");
    for (auto it = value.begin(); it != value.end(); ++it) {
        bool found = false;
        for (const char* key : allowed) if (it.key() == key) found = true;
        if (!found) invalid(std::string("unknown ") + label + " field: " + it.key());
    }
}

std::string servo_json(const json& servo, int& id) {
    keys(servo, {"id", "pwm", "time_ms", "bias"}, "servo");
    id = integer(servo, "id", 0, 254);
    const int pwm = integer(servo, "pwm", 500, 2500);
    const int time = integer(servo, "time_ms", 0, 9999);
    const int bias = servo.contains("bias") ? integer(servo, "bias", -500, 500) : 0;
    return servo_command(id, pwm, time, bias);
}

// Empty groups are valid project rows and board downloads, but not live moves.
std::string validated_group(const json& group, bool download, bool allow_empty) {
    keys(group, {"index", "items"}, "group");
    const int index = integer(group, "index", 0, 9999);
    if (!group.contains("items") || !group.at("items").is_array())
        invalid("items must be an array");
    const auto& items = group.at("items");
    if (items.size() > 32) invalid("a group supports at most 32 servos");
    if (items.empty() && !download && !allow_empty)
        invalid("live group must contain servos");
    std::string command = std::string(download ? "<G" : "{G") + padded(index, 4);
    std::set<int> seen;
    for (const auto& item : items) {
        int id = 0;
        const std::string servo = servo_json(item, id);
        if (!seen.insert(id).second) invalid("duplicate servo id " + std::to_string(id));
        command += servo;
    }
    if (download) {
        if (items.empty()) command += "$$$$!";
        command += "K0000N0000B000!>";
    } else command += '}';
    return command;
}

std::string unquote(const std::string& input) {
    if (input.empty() || input.front() != '"') return input;
    if (input.size() < 2 || input.back() != '"') invalid("invalid quoted group value");
    std::string out;
    for (std::size_t i = 1; i + 1 < input.size(); ++i) {
        char c = input[i];
        if (c == '"') invalid("unexpected quote in group value");
        if (c == '\\') {
            if (++i + 1 >= input.size()) invalid("incomplete quoted group escape");
            c = input[i];
            // Numeric servo commands do not need other QSettings escape forms.
            if (c != '\\' && c != '"') invalid("unsupported quoted group escape");
        }
        out += c;
    }
    return out;
}
}

std::string servo_command(int id, int pwm, int time_ms, int bias) {
    if (id < 0 || id > 254) invalid("id must be 0-254");
    if (bias < -500 || bias > 500) invalid("bias must be -500-500");
    if (pwm < 500 || pwm > 2500) invalid("pwm must be 500-2500");
    if (pwm + bias < 500 || pwm + bias > 2500)
        throw SafetyLimitError("pwm including bias must be 500-2500");
    if (time_ms < 0 || time_ms > 9999) invalid("time_ms must be 0-9999");
    return "#" + padded(id, 3) + "P" + padded(pwm + bias, 4) +
           "T" + padded(time_ms, 4) + "!";
}

std::string group_command(const json& group, bool download) {
    return validated_group(group, download, false);
}

std::vector<std::array<unsigned char, 64>> encode_hid(const std::string& command) {
    if (command.empty() || command.size() > 4096)
        invalid("command must contain 1-4096 ASCII bytes");
    for (unsigned char c : command)
        if (c < 32 || c > 126) invalid("command must contain printable ASCII only");
    // Qt uart_send_str uses the queued path at exactly 56 bytes as well.
    const std::uint32_t address = command.size() < 56 ? 0 :
        static_cast<std::uint32_t>(command.size());
    std::vector<std::array<unsigned char, 64>> reports;
    for (std::size_t offset = 0; offset < command.size(); offset += 56) {
        const auto count = std::min<std::size_t>(56, command.size() - offset);
        std::array<unsigned char, 64> report{};
        report[0] = 2;
        report[1] = 2;
        report[2] = static_cast<unsigned char>(count + 5);
        for (int i = 0; i < 4; ++i)
            report[3 + i] = static_cast<unsigned char>(address >> (24 - 8 * i));
        report[7] = 1;
        std::copy_n(command.begin() + offset, count, report.begin() + 8);
        reports.push_back(report);
    }
    return reports;
}

std::string decode_hid(const unsigned char* report, std::size_t size) {
    if (report == nullptr || size != 64) invalid("HID report must contain 64 bytes");
    if (report[0] != 2) invalid("unexpected HID report id");
    if (report[1] != 2 || report[2] <= 5) return {};
    const std::size_t count = report[2] - 5;
    if (count > 56) invalid("invalid HID payload length");
    return std::string(reinterpret_cast<const char*>(report + 8), count);
}

json import_groups(const std::string& content) {
    if (content.empty() || content.size() > 48000)
        invalid("INI text must contain 1-48000 bytes");
    static const std::regex key_pattern("^G([0-9]{4})$");
    static const std::regex group_pattern("^\\{G([0-9]{4})(.*)\\}$");
    static const std::regex servo_pattern("^#([0-9]{1,3})P([0-9]{1,4})T([0-9]{1,4})!");
    std::string source = content;
    if (source.compare(0, 3, "\xEF\xBB\xBF") == 0) source.erase(0, 3);
    std::istringstream input(source);
    std::string line, section;
    json groups = json::array();
    std::set<int> seen;
    int line_number = 0;
    while (std::getline(input, line)) {
        ++line_number;
        line = trim(line);
        if (line.empty() || line.front() == ';' || line.front() == '#') continue;
        if (line.front() == '[' && line.back() == ']') {
            section = line.substr(1, line.size() - 2);
            continue;
        }
        if (section != "group") continue;
        const auto separator = line.find('=');
        if (separator == std::string::npos)
            invalid("line " + std::to_string(line_number) + ": expected group=value");
        const auto key = trim(line.substr(0, separator));
        std::smatch key_match;
        if (!std::regex_match(key, key_match, key_pattern)) invalid("invalid group key");
        const int index = std::stoi(key_match[1].str());
        if (!seen.insert(index).second) invalid("duplicate group " + std::to_string(index));
        const auto value = unquote(trim(line.substr(separator + 1)));
        std::smatch group_match;
        if (!std::regex_match(value, group_match, group_pattern) || group_match[1] != key_match[1])
            invalid("expected matching {G" + padded(index, 4) + "...}");
        json group = {{"index", index}, {"items", json::array()}};
        std::string rest = group_match[2].str();
        std::set<int> ids;
        int slots = 0;
        while (!rest.empty()) {
            std::smatch match;
            if (!std::regex_search(rest, match, servo_pattern))
                invalid("group contains an unsupported command or invalid servo");
            if (++slots > 32) invalid("group exceeds 32 servo slots");
            const int id = std::stoi(match[1].str());
            const int pwm = std::stoi(match[2].str());
            const int time_ms = std::stoi(match[3].str());
            rest.erase(0, match.length());
            if (id > 254) invalid("id must be 0-254");
            // Original Qt stores blank cells as P0000T0000.
            if (pwm == 0 && time_ms == 0) continue;
            servo_command(id, pwm, time_ms);
            if (!ids.insert(id).second) invalid("group has duplicate servo id");
            group["items"].push_back({{"id", id}, {"pwm", pwm}, {"time_ms", time_ms}});
        }
        groups.push_back(group);
        if (groups.size() > 128) invalid("at most 128 groups per import");
    }
    if (groups.empty()) invalid("no groups found in [group] section");
    std::sort(groups.begin(), groups.end(), [](const json& left, const json& right) {
        return left.at("index").get<int>() < right.at("index").get<int>();
    });
    return groups;
}

std::string export_groups(const json& groups) {
    if (!groups.is_array() || groups.empty() || groups.size() > 128)
        invalid("export accepts an array of 1-128 groups");
    std::string output = "[group]\r\n";
    std::set<int> seen;
    for (const auto& group : groups) {
        const std::string command = validated_group(group, false, true);
        const int index = integer(group, "index", 0, 9999);
        if (!seen.insert(index).second) invalid("duplicate group index");
        output += "G" + padded(index, 4) + "=" + command + "\r\n";
        if (output.size() > 48000) invalid("export exceeds 48000 bytes");
    }
    return output;
}
}
