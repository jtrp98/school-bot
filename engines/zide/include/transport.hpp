#pragma once

#include <cstddef>
#include <string>
#include <vector>

namespace zide {
struct TransportOptions {
    std::string transport = "dry-run", port, device;
    int baud = 115200, read_ms = 1000;
};
struct ExchangeResult {
    std::string received;
    std::size_t bytes_written = 0, bytes_read = 0;
};
struct Device { std::string transport, path, name; };
std::vector<Device> list_devices();
ExchangeResult exchange(const TransportOptions&, const std::vector<std::string>&);
}
