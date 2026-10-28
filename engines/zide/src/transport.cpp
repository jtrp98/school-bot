#include "transport.hpp"
#include "transport_windows.hpp"
#include <stdexcept>

namespace zide {
ExchangeResult exchange(const TransportOptions& options, const std::vector<std::string>& commands) {
#ifdef _WIN32
    const auto deadline = detail::Clock::now() + detail::Milliseconds(4000);
    if (options.transport == "hid") return detail::exchange_hid(options, commands, deadline);
    return detail::exchange_serial(options, commands, deadline);
#else
    throw std::runtime_error("native ZIDE hardware transport is supported only on Windows; use dry-run on this platform");
#endif
}
}
