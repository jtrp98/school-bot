#pragma once

#include "transport.hpp"
#include <algorithm>
#include <chrono>
#include <stdexcept>
#include <thread>

#ifdef _WIN32
#ifndef NOMINMAX
#define NOMINMAX
#endif
#include <windows.h>

namespace zide::detail {
using Clock = std::chrono::steady_clock;
using Milliseconds = std::chrono::milliseconds;
inline bool valid_port(const std::string& name) {
    const bool prefix = name.size() >= 4 && name.size() <= 7 && name.compare(0, 3, "COM") == 0;
    if (!prefix || name[3] < '1' || name[3] > '9') return false;
    return std::all_of(name.begin() + 4, name.end(), [](char c) { return c >= '0' && c <= '9'; });
}
inline std::runtime_error win_error(const std::string& operation) {
    return std::runtime_error(operation + " (Windows error " + std::to_string(GetLastError()) + ")");
}
inline void check_deadline(Clock::time_point deadline) {
    if (Clock::now() >= deadline) {
        throw std::runtime_error("hardware exchange exceeded 4 seconds; delivery may be partial");
    }
}
inline void append(ExchangeResult& result, const std::string& data) {
    if (result.received.size() + data.size() > 65536) throw std::runtime_error("receive limit exceeded (64 KiB)");
    result.received += data;
    result.bytes_read = result.received.size();
}
struct Handle {
    HANDLE value = INVALID_HANDLE_VALUE;
    ~Handle() { if (value != INVALID_HANDLE_VALUE) CloseHandle(value); }
};
template<class Poll>
void receive_until(Clock::time_point end, Clock::time_point deadline, Poll poll) {
    while (Clock::now() < end) {
        check_deadline(deadline);
        poll();
        std::this_thread::sleep_for(Milliseconds(1));
    }
}
std::vector<std::string> hid_paths();
ExchangeResult exchange_hid(const TransportOptions&, const std::vector<std::string>&, Clock::time_point);
ExchangeResult exchange_serial(const TransportOptions&, const std::vector<std::string>&, Clock::time_point);
}
#endif
