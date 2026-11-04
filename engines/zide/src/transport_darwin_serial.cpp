#include "transport_darwin.hpp"
#include "transport_io.hpp"

#ifdef __APPLE__
#include <IOKit/serial/ioss.h>
#include <cerrno>
#include <fcntl.h>
#include <poll.h>
#include <sys/ioctl.h>
#include <sys/stat.h>
#include <termios.h>
#include <thread>
#include <unistd.h>

namespace zide::detail {
namespace {
struct SerialIO final : TransportIO {
    int descriptor = -1;
    ~SerialIO() override { if (descriptor >= 0) close(descriptor); }
    Clock::time_point now() override { return Clock::now(); }
    void wait(int milliseconds) override { std::this_thread::sleep_for(Milliseconds(milliseconds)); }
    std::string read() override {
        pollfd descriptor_state{descriptor, POLLIN, 0};
        const auto ready = poll(&descriptor_state, 1, 0);
        if (ready < 0 && errno == EINTR) return {};
        if (ready < 0 || (descriptor_state.revents & (POLLHUP | POLLERR | POLLNVAL)))
            throw std::runtime_error("serial device disconnected");
        if (ready == 0) return {};
        char bytes[4096];
        const auto count = ::read(descriptor, bytes, sizeof(bytes));
        if (count < 0 && (errno == EAGAIN || errno == EWOULDBLOCK || errno == EINTR)) return {};
        if (count <= 0) throw std::runtime_error("serial read failed; device may have disconnected");
        return std::string(bytes, static_cast<std::size_t>(count));
    }
    std::size_t write(const std::string& bytes) override {
        const auto count = ::write(descriptor, bytes.data(), bytes.size());
        if (count < 0) throw std::runtime_error("serial write failed; delivery unknown; do not retry");
        return static_cast<std::size_t>(count);
    }
};
}

ExchangeResult exchange_darwin_serial(const TransportOptions& options, const std::vector<std::string>& commands) {
    std::string callout;
    for (const auto& device : serial_devices()) {
        if (device.token == options.port) callout = device.callout;
    }
    if (callout.empty()) throw std::runtime_error("selected serial identity is not currently enumerated");
    SerialIO io;
    io.descriptor = open(callout.c_str(), O_RDWR | O_NOCTTY | O_NONBLOCK | O_NOFOLLOW);
    if (io.descriptor < 0) throw std::runtime_error("cannot open selected serial callout device");
    struct stat opened{}, current{};
    if (fstat(io.descriptor, &opened) != 0 || !S_ISCHR(opened.st_mode) ||
        lstat(callout.c_str(), &current) != 0 || !S_ISCHR(current.st_mode) || opened.st_rdev != current.st_rdev)
        throw std::runtime_error("serial device changed while opening");
    bool matches = false;
    for (const auto& device : serial_devices()) {
        if (device.token == options.port && device.callout == callout) matches = true;
    }
    if (!matches) throw std::runtime_error("serial identity disappeared while opening");
    if (ioctl(io.descriptor, TIOCEXCL) != 0) throw std::runtime_error("cannot acquire exclusive serial access");
    termios state{};
    if (tcgetattr(io.descriptor, &state) != 0) throw std::runtime_error("cannot read serial configuration");
    cfmakeraw(&state);
    state.c_cflag &= ~(CSIZE | PARENB | CSTOPB | CRTSCTS);
    state.c_cflag |= CS8 | CLOCAL | CREAD;
    state.c_cc[VMIN] = 0;
    state.c_cc[VTIME] = 0;
    if (cfsetspeed(&state, B9600) != 0 || tcsetattr(io.descriptor, TCSANOW, &state) != 0)
        throw std::runtime_error("cannot set serial raw 8N1 configuration");
    speed_t speed = static_cast<speed_t>(options.baud);
    if (ioctl(io.descriptor, IOSSIOSPEED, &speed) != 0) throw std::runtime_error("cannot set serial baud");
    if (tcflush(io.descriptor, TCIFLUSH) != 0) throw std::runtime_error("cannot discard stale serial input");
    return exchange_io(io, false, options, commands);
}
}
#endif
