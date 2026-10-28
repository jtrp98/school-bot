#include "transport_windows.hpp"

#ifdef _WIN32
#include <array>

namespace zide::detail {
ExchangeResult exchange_serial(const TransportOptions& options, const std::vector<std::string>& commands,
                               Clock::time_point deadline) {
    if (!valid_port(options.port)) throw std::runtime_error("serial port must be COM1 through COM9999");
    Handle handle;
    const std::string path = "\\\\.\\" + options.port;
    handle.value = CreateFileA(path.c_str(), GENERIC_READ | GENERIC_WRITE, 0, nullptr, OPEN_EXISTING, 0, nullptr);
    if (handle.value == INVALID_HANDLE_VALUE) throw win_error("open serial port");
    DCB state{};
    state.DCBlength = sizeof(state);
    if (!GetCommState(handle.value, &state)) throw win_error("get serial state");
    state.BaudRate = options.baud;
    state.ByteSize = 8;
    state.Parity = NOPARITY;
    state.StopBits = ONESTOPBIT;
    state.fBinary = TRUE;
    state.fParity = FALSE;
    state.fOutxCtsFlow = FALSE;
    state.fOutxDsrFlow = FALSE;
    state.fDtrControl = DTR_CONTROL_DISABLE;
    state.fDsrSensitivity = FALSE;
    state.fTXContinueOnXoff = TRUE;
    state.fOutX = FALSE;
    state.fInX = FALSE;
    state.fErrorChar = FALSE;
    state.fNull = FALSE;
    state.fRtsControl = RTS_CONTROL_DISABLE;
    state.fAbortOnError = FALSE;
    if (!SetCommState(handle.value, &state)) throw win_error("set serial 8N1 state");
    COMMTIMEOUTS timeouts{};
    timeouts.ReadIntervalTimeout = MAXDWORD; // Immediate buffered reads; preserve response fragments.
    timeouts.WriteTotalTimeoutConstant = 1000;
    if (!SetCommTimeouts(handle.value, &timeouts)) throw win_error("set serial timeouts");
    ExchangeResult result;
    const auto poll = [&] {
        std::array<char, 4096> bytes{};
        DWORD n = 0;
        if (!ReadFile(handle.value, bytes.data(), static_cast<DWORD>(bytes.size()), &n, nullptr)) {
            throw win_error("read serial response");
        }
        if (n) append(result, std::string(bytes.data(), n));
    };
    for (std::size_t i = 0; i < commands.size(); ++i) {
        if (i) receive_until(Clock::now() + Milliseconds(200), deadline, poll);
        check_deadline(deadline);
        const auto remaining = std::chrono::duration_cast<Milliseconds>(deadline - Clock::now()).count();
        const auto write_ms = std::max<long long>(1, std::min<long long>(1000, remaining));
        timeouts.WriteTotalTimeoutConstant = static_cast<DWORD>(write_ms);
        if (!SetCommTimeouts(handle.value, &timeouts)) throw win_error("set serial write timeout");
        DWORD n = 0;
        const auto& command = commands[i];
        const BOOL written = WriteFile(handle.value, command.data(), static_cast<DWORD>(command.size()), &n, nullptr);
        result.bytes_written += n;
        if (!written) {
            throw win_error("serial write failed after " + std::to_string(n) + " bytes; delivery may be partial");
        }
        if (n != command.size()) {
            throw std::runtime_error("short serial write " + std::to_string(n) + "/" +
                                     std::to_string(command.size()) + " bytes; do not automatically retry");
        }
        poll();
    }
    receive_until(Clock::now() + Milliseconds(options.read_ms), deadline, poll);
    return result;
}
}
#endif
