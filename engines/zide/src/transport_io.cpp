#include "transport_io.hpp"
#include "protocol.hpp"
#include <algorithm>

namespace zide::detail {
ExchangeResult exchange_io(TransportIO& io, bool hid, const TransportOptions& options,
                           const std::vector<std::string>& commands) {
    if (options.read_ms < 1 || options.read_ms > 1500) throw std::runtime_error("invalid read_ms");
    std::vector<std::vector<std::array<unsigned char, 64>>> frames;
    for (const auto& command : commands) frames.push_back(encode_hid(command));
    const auto deadline = io.now() + Milliseconds(4000);
    const auto check = [&] {
        if (io.now() >= deadline) throw std::runtime_error("hardware exchange exceeded 4 seconds; delivery may be partial");
    };
    const auto write = [&](const std::string& bytes) {
        check();
        const auto count = io.write(bytes);
        check();
        if (count != bytes.size()) throw std::runtime_error("partial transport write; do not automatically retry");
        return count;
    };
    if (hid) {
        std::string probe(64, '\0');
        probe[0] = 2;
        probe[1] = 1;
        probe[2] = 4;
        std::fill(probe.begin() + 3, probe.begin() + 7, 0x55);
        write(probe);
        const auto end = io.now() + Milliseconds(1500);
        while (io.now() < end) {
            check();
            const auto reply = io.read();
            if (!reply.empty()) {
                if (reply.size() != 64) throw std::runtime_error("unsupported HID report length");
                // Preserve CHECK_FW silence semantics; a control reply must never permit flash passthrough.
                if (reply[0] == 2 && reply[1] == 1)
                    throw std::runtime_error("CHECK_FW response received; HID passthrough refused");
            }
            io.wait(1);
        }
    }
    ExchangeResult result;
    const auto poll = [&] {
        check();
        auto bytes = io.read();
        if (hid && !bytes.empty()) {
            if (bytes.size() != 64) throw std::runtime_error("unsupported HID report length");
            if (bytes[0] != 2 || bytes[1] != 2) return;
            bytes = decode_hid(reinterpret_cast<const unsigned char*>(bytes.data()), bytes.size());
        }
        if (result.received.size() + bytes.size() > 65536) throw std::runtime_error("receive limit exceeded (64 KiB)");
        result.received += bytes;
        result.bytes_read = result.received.size();
    };
    const auto receive = [&](int milliseconds) {
        const auto end = io.now() + Milliseconds(milliseconds);
        while (io.now() < end) {
            poll();
            io.wait(1);
        }
    };
    for (std::size_t index = 0; index < commands.size(); ++index) {
        if (index) receive(200);
        if (hid) {
            for (std::size_t frame = 0; frame < frames[index].size(); ++frame) {
                if (frame) receive(1);
                const auto& report = frames[index][frame];
                result.bytes_written += write(std::string(reinterpret_cast<const char*>(report.data()), report.size()));
                poll();
            }
        } else {
            result.bytes_written += write(commands[index]);
            poll();
        }
    }
    receive(options.read_ms);
    check();
    return result;
}
}
