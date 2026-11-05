#include "transport_io.hpp"
#include "protocol.hpp"
#include <deque>
#include <iostream>

namespace {
struct FakeIO final : zide::detail::TransportIO {
    zide::detail::Clock::time_point clock{};
    std::vector<std::string> writes;
    std::deque<std::string> replies;
    bool partial = false, disconnected = false, endless = false;
    int write_delay = 0;
    zide::detail::Clock::time_point now() override { return clock; }
    void wait(int ms) override { clock += zide::detail::Milliseconds(ms); }
    std::string read() override {
        if (disconnected) throw std::runtime_error("disconnected");
        if (endless) return std::string(4096, 'x');
        if (replies.empty()) return {};
        auto reply = replies.front();
        replies.pop_front();
        return reply;
    }
    std::size_t write(const std::string& bytes) override {
        writes.push_back(bytes);
        wait(write_delay);
        return bytes.size() - (partial ? 1 : 0);
    }
};
void check(bool ok, const char* label) { if (!ok) throw std::runtime_error(label); }
template<class Work> void rejects(Work work) {
    bool failed = false;
    try { work(); } catch (const std::runtime_error&) { failed = true; }
    check(failed, "expected transport rejection");
}
}
int main() {
    try {
        zide::TransportOptions options;
        options.read_ms = 1;
        const std::vector<std::string> commands = {"$DRS!"};
        FakeIO serial;
        serial.replies = {"#000P", "1500!"};
        auto result = zide::detail::exchange_io(serial, false, options, commands);
        check(result.received == "#000P1500!" && result.bytes_written == 5, "fragmented serial read");
        FakeIO hid;
        result = zide::detail::exchange_io(hid, true, options, commands);
        check(hid.writes.size() == 2 && hid.writes[0][1] == 1 && hid.writes[1][1] == 2,
              "CHECK_FW before TRAN_DATA");
        check(result.bytes_written == 64 && hid.now().time_since_epoch().count() > 0, "HID budget");
        FakeIO bootloader;
        std::string reply(64, '\0');
        reply[0] = 2;
        reply[1] = 1;
        reply[2] = 4;
        bootloader.replies.push_back(reply);
        rejects([&] { zide::detail::exchange_io(bootloader, true, options, commands); });
        check(bootloader.writes.size() == 1, "bootloader never receives passthrough");
        for (bool is_hid : {false, true}) {
            FakeIO partial;
            partial.partial = true;
            rejects([&] { zide::detail::exchange_io(partial, is_hid, options, commands); });
            check(partial.writes.size() == 1, "partial writes never retry");
            FakeIO lost;
            lost.disconnected = true;
            rejects([&] { zide::detail::exchange_io(lost, is_hid, options, commands); });
            FakeIO invalid;
            rejects([&] { zide::detail::exchange_io(invalid, is_hid, options, {"$DST!", "\n"}); });
            check(invalid.writes.empty(), "validate all commands before first write");
        }
        FakeIO expired;
        expired.write_delay = 4000;
        rejects([&] { zide::detail::exchange_io(expired, false, options, commands); });
        FakeIO too_many;
        std::vector<std::string> batch(30, "$DRS!");
        rejects([&] { zide::detail::exchange_io(too_many, false, options, batch); });
        check(too_many.writes.size() < batch.size(), "deadline bounds command dispatch");
        FakeIO overflow;
        overflow.endless = true;
        options.read_ms = 1500;
        rejects([&] { zide::detail::exchange_io(overflow, false, options, commands); });
        FakeIO silent;
        result = zide::detail::exchange_io(silent, false, options, commands);
        check(result.bytes_read == 0, "silence is not fabricated telemetry");
        std::cout << "transport scenarios passed\n";
    } catch (const std::exception& error) {
        std::cerr << error.what() << '\n';
        return 1;
    }
}
