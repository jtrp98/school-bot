#include "protocol.hpp"
#include <algorithm>
#include <array>
#include <cstdint>
#include <iostream>
#include <stdexcept>
#include <string>

namespace {
int checks = 0;
void check(bool condition, const std::string& label) {
    ++checks;
    if (!condition) throw std::runtime_error("FAILED: " + label);
}
template<class Function> void reject(Function function, const std::string& label) {
    ++checks;
    try { function(); }
    catch (const std::runtime_error&) { return; }
    throw std::runtime_error("FAILED to reject: " + label);
}
zide::json item(int id = 1, int pwm = 1500, int time = 1000) {
    return {{"id", id}, {"pwm", pwm}, {"time_ms", time}};
}
zide::json group(int index = 0) {
    return {{"index", index}, {"items", zide::json::array({item()})}};
}
void commands() {
    using namespace zide;
    check(servo_command(1, 1500, 1000) == "#001P1500T1000!", "original padded servo golden");
    check(servo_command(0, 500, 0) == "#000P0500T0000!", "minimum servo fields");
    check(servo_command(254, 2500, 9999) == "#254P2500T9999!", "maximum servo fields");
    check(servo_command(1, 1500, 0, -500) == "#001P1000T0000!", "negative bias");
    check(servo_command(1, 1500, 0, 500) == "#001P2000T0000!", "positive bias");
    reject([] { servo_command(-1, 1500, 1000); }, "negative id");
    reject([] { servo_command(255, 1500, 1000); }, "broadcast movement id");
    reject([] { servo_command(1, 499, 1000); }, "low pwm");
    reject([] { servo_command(1, 2501, 1000); }, "high pwm");
    reject([] { servo_command(1, 500, 1000, -1); }, "bias exceeds lower calibrated pwm");
    reject([] { servo_command(1, 2500, 1000, 1); }, "bias exceeds upper calibrated pwm");
    reject([] { servo_command(1, 1500, 1000, -501); }, "low bias");
    reject([] { servo_command(1, 1500, 1000, 501); }, "high bias");
    reject([] { servo_command(1, 1500, -1); }, "negative time");
    reject([] { servo_command(1, 1500, 10000); }, "excess time");
    for (const auto& [pwm, bias] : {std::pair<int, int>{500, -1}, std::pair<int, int>{2500, 1}}) {
        bool safety = false;
        try { servo_command(1, pwm, 0, bias); } catch (const SafetyLimitError&) { safety = true; }
        check(safety, "bias that leaves the PWM range is a safety limit");
    }
    bool field_is_safety = false;
    try { servo_command(1, 2501, 1000); } catch (const SafetyLimitError&) { field_is_safety = true; } catch (const std::runtime_error&) {}
    check(!field_is_safety, "a pwm field outside its own range is invalid input, not a safety limit");
    check(group_command(group(42), false) == "{G0042#001P1500T1000!}", "original live group golden");
    check(group_command(group(42), true) == "<G0042#001P1500T1000!K0000N0000B000!>", "original saved group golden");
    json empty = {{"index", 0}, {"items", json::array()}};
    check(group_command(empty, true) == "<G0000$$$$!K0000N0000B000!>", "original empty saved row");
    reject([&] { group_command(empty, false); }, "empty live group");
    for (int index : {-1, 10000}) reject([&] { group_command(group(index), false); }, "group index range");
    json duplicate = group(); duplicate["items"].push_back(item());
    reject([&] { group_command(duplicate, false); }, "duplicate servo id");
    json full = {{"index", 9999}, {"items", json::array()}};
    for (int id = 0; id < 32; ++id) full["items"].push_back(item(id));
    check(group_command(full, false).find("#031P1500T1000!") != std::string::npos, "32 unique servo group accepted");
    full["items"].push_back(item(32));
    reject([&] { group_command(full, false); }, "33 servo group rejected");
    for (const char* field : {"id", "pwm", "time_ms"}) {
        json missing = group(); missing["items"][0].erase(field);
        reject([&] { group_command(missing, false); }, std::string("missing required ") + field);
        for (const json& bad : {json(nullptr), json("1500"), json(1500.0), json(true), json::array()}) {
            json typed = group(); typed["items"][0][field] = bad;
            reject([&] { group_command(typed, false); }, std::string("strict numeric ") + field);
        }
    }
    json oversized = group(); oversized["items"][0]["bias"] = UINT64_MAX;
    reject([&] { group_command(oversized, false); }, "unsigned integer does not wrap to negative bias");
    json unknown = group(); unknown["items"][0]["typo"] = 1;
    reject([&] { group_command(unknown, false); }, "unknown servo field");
    unknown = group(); unknown["typo"] = 1;
    reject([&] { group_command(unknown, false); }, "unknown group field");
    json missing = group(); missing.erase("index");
    reject([&] { group_command(missing, false); }, "missing group index");
    missing = group(); missing.erase("items");
    reject([&] { group_command(missing, false); }, "missing group items");
    missing = group(); missing["items"] = "servos";
    reject([&] { group_command(missing, false); }, "items array required");
    missing = group(); missing["index"] = 0.0;
    reject([&] { group_command(missing, false); }, "group index integer required");
    missing = group(); missing["index"] = UINT64_MAX;
    reject([&] { group_command(missing, false); }, "oversized unsigned group index");
    reject([] { group_command(json::array(), false); }, "group object required");
}
void hid() {
    using namespace zide;
    std::array<unsigned char, 64> golden{};
    golden[0] = 2; golden[1] = 2; golden[2] = 10; golden[7] = 1;
    const std::string drs = "$DRS!";
    std::copy(drs.begin(), drs.end(), golden.begin() + 8);
    const auto frames = encode_hid(drs);
    check(frames.size() == 1 && frames[0] == golden, "DRS full64 actual frame and zero padding");
    for (std::size_t size : {55u, 56u, 57u, 112u, 4096u}) {
        const std::string command(size, 'X');
        const auto chunks = encode_hid(command);
        check(chunks.size() == (size + 55) / 56, "HID chunk count " + std::to_string(size));
        std::string decoded;
        for (std::size_t i = 0; i < chunks.size(); ++i) {
            const auto& frame = chunks[i];
            const auto count = std::min<std::size_t>(56, size - i * 56);
            const std::uint32_t address = (std::uint32_t(frame[3]) << 24) | (std::uint32_t(frame[4]) << 16) |
                (std::uint32_t(frame[5]) << 8) | frame[6];
            check(address == (size < 56 ? 0 : size), "Qt exact56 boundary and total address");
            check(frame[0] == 2 && frame[1] == 2 && frame[2] == count + 5 && frame[7] == 1, "HID chunk envelope");
            check(std::all_of(frame.begin() + 8 + count, frame.end(), [](unsigned char b) { return b == 0; }), "HID unused bytes zero");
            decoded += decode_hid(frame.data(), frame.size());
        }
        check(decoded == command, "RX fragments concatenate without inserted newline");
    }
    auto ack = golden; ack[2] = 0;
    check(decode_hid(ack.data(), ack.size()).empty(), "success ACK is not text");
    ack[2] = 1;
    check(decode_hid(ack.data(), ack.size()).empty(), "error ACK is not text");
    ack[2] = 5;
    check(decode_hid(ack.data(), ack.size()).empty(), "zero-payload transport report");
    ack[1] = 1; ack[2] = 4;
    check(decode_hid(ack.data(), ack.size()).empty(), "CHECK_FW response is not text");
    ack = golden; ack[2] = 62;
    reject([&] { decode_hid(ack.data(), ack.size()); }, "oversized RX payload");
    ack = golden; ack[0] = 1;
    reject([&] { decode_hid(ack.data(), ack.size()); }, "wrong report id");
    reject([&] { decode_hid(golden.data(), 63); }, "short report");
    reject([] { decode_hid(nullptr, 64); }, "null report");
    for (const std::string& bad : {std::string(), std::string(4097, 'X'), std::string("$DRS!\n"), std::string(1, '\0'), std::string(1, char(0x80))})
        reject([&] { encode_hid(bad); }, "command size or printable ASCII");
}
void projects() {
    using namespace zide;
    const std::string source = "\xEF\xBB\xBF; project\r\n[layout]\r\nfoo=ignored\r\n[group]\r\n"
        "G0042=\"{G0042#001P1500T1000!#002P0000T0000!}\"\r\nG0000={G0000}\r\n";
    const json imported = import_groups(source);
    check(imported.size() == 2 && imported[0]["index"] == 0 && imported[1]["index"] == 42, "BOM comments quotes and sorted preserved indices");
    check(imported[0]["items"].empty() && imported[1]["items"] == json::array({item()}), "empty cells omitted and empty row preserved");
    const std::string exported = export_groups(imported);
    check(exported == "[group]\r\nG0000={G0000}\r\nG0042={G0042#001P1500T1000!}\r\n", "group-only export golden");
    check(import_groups(exported) == imported, "project semantic round trip");
    json biased = group(); biased["items"][0]["bias"] = 100;
    check(import_groups(export_groups(json::array({biased})))[0]["items"][0]["pwm"] == 1600, "export materializes calibrated PWM");
    for (const std::string bad : {
        "", "[other]\nG0000={G0000}", "[group]\nG0000={G0001}",
        "[group]\nG0000={G0000$DST!}", "[group]\nG0000={G0000#001PRAD!}",
        "[group]\nG0000={G0000#001P1500T1000!garbage}", "[group]\nG0000={G0000}\nG0000={G0000}",
        "[group]\nG0000={G0000#001P1500T1000!#001P1600T1000!}",
        "[group]\nG0000={G0000#255P0000T0000!}", "[group]\nG0000={G0000#001P0000T1000!}",
        "[group]\nG0000=\"{G0000}", "[group]\nG0={G0000}", "[group]\ninvalid"})
        reject([&] { import_groups(bad); }, "unsupported or malformed INI");
    reject([] { import_groups(std::string(48001, 'X')); }, "oversized project input");
    std::string slots = "[group]\nG0000={G0000";
    for (int i = 0; i < 33; ++i) slots += "#001P0000T0000!";
    reject([&] { import_groups(slots + "}"); }, "blank cells still count against slot limit");
    reject([] { export_groups(json::array()); }, "empty export");
    reject([] { export_groups(json::object()); }, "export array required");
    reject([] { export_groups(json::array({group(), group()})); }, "duplicate export indices");
    json too_many = json::array();
    std::string too_many_ini = "[group]\n";
    for (int i = 0; i < 129; ++i) {
        too_many.push_back(group(i));
        std::string number = std::to_string(i); number.insert(0, 4 - number.size(), '0');
        too_many_ini += "G" + number + "={G" + number + "}\n";
    }
    reject([&] { export_groups(too_many); }, "export group count limit");
    reject([&] { import_groups(too_many_ini); }, "import group count limit");
    json oversized_export = json::array();
    for (int index = 0; index < 128; ++index) {
        json dense = {{"index", index}, {"items", json::array()}};
        for (int id = 0; id < 32; ++id) dense["items"].push_back(item(id));
        oversized_export.push_back(dense);
    }
    reject([&] { export_groups(oversized_export); }, "export byte limit within group count limit");
}
}
int main() {
    try {
        commands(); hid(); projects();
        std::cout << "protocol tests passed: " << checks << " checks\n";
        return 0;
    } catch (const std::exception& error) {
        std::cerr << error.what() << " after " << checks << " checks\n";
        return 1;
    }
}
