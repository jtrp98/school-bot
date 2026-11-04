#pragma once

#include "transport.hpp"
#include <chrono>
#include <stdexcept>

namespace zide::detail {
using Clock = std::chrono::steady_clock;
using Milliseconds = std::chrono::milliseconds;

// The clock and byte stream share the native exchange loop with deterministic transport tests.
struct TransportIO {
    virtual ~TransportIO() = default;
    virtual Clock::time_point now() = 0;
    virtual void wait(int milliseconds) = 0;
    virtual std::string read() = 0;
    virtual std::size_t write(const std::string& bytes) = 0;
};
ExchangeResult exchange_io(TransportIO&, bool hid, const TransportOptions&, const std::vector<std::string>&);
}
