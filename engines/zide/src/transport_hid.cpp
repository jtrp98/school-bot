#include "transport_windows.hpp"
#include "protocol.hpp"

#ifdef _WIN32
#include "hidapi/hidapi.h"
#include <array>
#include <memory>

namespace zide::detail {
namespace {
using Hid = std::unique_ptr<hid_device, decltype(&hid_close)>;
int read_hid(hid_device* handle, std::array<unsigned char, 64>& report, int timeout) {
    const int n = hid_read_timeout(handle, report.data(), report.size(), timeout);
    if (n < 0) throw std::runtime_error("HID read failed; board may have disconnected");
    if (n != 0 && n != 64) throw std::runtime_error("unsupported HID report length " + std::to_string(n));
    return n;
}
void check_firmware(hid_device* handle, Clock::time_point deadline) {
    std::array<unsigned char, 64> probe{};
    probe[0] = 2;
    probe[1] = 1;
    probe[2] = 4;
    std::fill(probe.begin() + 3, probe.begin() + 7, 0x55);
    if (hid_write(handle, probe.data(), probe.size()) != 64) throw std::runtime_error("CHECK_FW probe write failed");
    // Silence is the original board application-mode signal; bypassing this can send TRAN_DATA to flash.
    const auto end = std::min(deadline, Clock::now() + Milliseconds(1500));
    while (Clock::now() < end) {
        std::array<unsigned char, 64> report{};
        const int n = read_hid(handle, report, 20);
        if (n == 0 || report[0] != 2 || report[1] != 1) continue;
        if (report[2] != 4) throw std::runtime_error("malformed CHECK_FW response; HID passthrough refused");
        if (std::all_of(report.begin() + 3, report.begin() + 7, [](unsigned char b) { return b == 0xaa; }))
            throw std::runtime_error("BOOTLOADER_MODE: HID passthrough refused because TRAN_DATA would write flash; "
                                     "restart board in application mode");
        throw std::runtime_error("unrecognized CHECK_FW response; HID passthrough refused");
    }
    check_deadline(deadline);
}
}

ExchangeResult exchange_hid(const TransportOptions& options, const std::vector<std::string>& commands,
                            Clock::time_point deadline) {
    const auto paths = hid_paths();
    std::string selected = options.device;
    if (selected.empty()) {
        if (paths.size() != 1) {
            throw std::runtime_error("found " + std::to_string(paths.size()) +
                                     " ZIDE HID devices; select one exact device path");
        }
        selected = paths.front();
    } else if (std::find(paths.begin(), paths.end(), selected) == paths.end()) {
        throw std::runtime_error("selected device is not a connected ZIDE HID board (0483:5750)");
    }
    Hid handle(hid_open_path(selected.c_str()), hid_close);
    if (!handle) throw std::runtime_error("cannot open ZIDE HID board with supported 64-byte reports");
    if (hid_set_nonblocking(handle.get(), 1) != 0) throw std::runtime_error("cannot set HID read mode");
    check_firmware(handle.get(), deadline);
    ExchangeResult result;
    const auto poll = [&] {
        std::array<unsigned char, 64> report{};
        if (read_hid(handle.get(), report, 0) == 0) return;
        if (report[0] != 2 || report[1] != 2) return; // Control replies are not command text.
        append(result, decode_hid(report.data(), report.size()));
    };
    for (std::size_t i = 0; i < commands.size(); ++i) {
        if (i) receive_until(Clock::now() + Milliseconds(200), deadline, poll);
        auto reports = encode_hid(commands[i]);
        for (std::size_t j = 0; j < reports.size(); ++j) {
            if (j) receive_until(Clock::now() + Milliseconds(1), deadline, poll);
            check_deadline(deadline);
            const auto& report = reports[j];
            const int n = hid_write(handle.get(), report.data(), report.size());
            if (n > 0) result.bytes_written += static_cast<std::size_t>(n);
            if (n != 64) {
                throw std::runtime_error("HID write failed or was partial (" + std::to_string(n) +
                                         "/64 bytes); do not automatically retry");
            }
            poll();
        }
    }
    receive_until(Clock::now() + Milliseconds(options.read_ms), deadline, poll);
    return result;
}
}
#endif
