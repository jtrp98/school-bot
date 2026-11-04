#include "transport_darwin.hpp"
#include "transport_io.hpp"

#ifdef __APPLE__
#include <array>
#include <deque>

namespace zide::detail {
namespace {
struct HidIO final : TransportIO {
    IOHIDDeviceRef device = nullptr;
    std::array<uint8_t, 64> buffer{};
    std::deque<std::string> pending;
    bool failed = false;
    bool opened = false;
    IOReturn write_status = kIOReturnNotReady;
    std::string write_buffer;
    ~HidIO() override {
        if (!device) return;
        if (opened) {
            IOHIDDeviceUnscheduleFromRunLoop(device, CFRunLoopGetCurrent(), kCFRunLoopDefaultMode);
            IOHIDDeviceRegisterInputReportCallback(device, buffer.data(), buffer.size(), nullptr, nullptr);
            IOHIDDeviceRegisterRemovalCallback(device, nullptr, nullptr);
            IOHIDDeviceClose(device, kIOHIDOptionsTypeNone);
        }
        CFRelease(device);
    }
    Clock::time_point now() override { return Clock::now(); }
    void wait(int milliseconds) override {
        CFRunLoopRunInMode(kCFRunLoopDefaultMode, milliseconds / 1000.0, false);
    }
    static void report(void* context, IOReturn status, void*, IOHIDReportType,
                       uint32_t report_id, uint8_t* bytes, CFIndex length) {
        auto& io = *static_cast<HidIO*>(context);
        if (status != kIOReturnSuccess || report_id != 2 || length != 64 || io.pending.size() >= 1024) {
            io.failed = true;
            return;
        }
        io.pending.emplace_back(reinterpret_cast<char*>(bytes), static_cast<std::size_t>(length));
    }
    static void removed(void* context, IOReturn, void*) { static_cast<HidIO*>(context)->failed = true; }
    std::string read() override {
        CFRunLoopRunInMode(kCFRunLoopDefaultMode, 0, true);
        if (failed) throw std::runtime_error("HID read failed or device disconnected");
        if (pending.empty()) return {};
        auto bytes = std::move(pending.front());
        pending.pop_front();
        return bytes;
    }
    static void sent(void* context, IOReturn status, void*, IOHIDReportType, uint32_t, uint8_t*, CFIndex) {
        *static_cast<IOReturn*>(context) = status;
    }
    std::size_t write(const std::string& bytes) override {
        if (failed) throw std::runtime_error("HID device disconnected");
        // The synchronous IOKit API has no deadline; use its bounded asynchronous report operation.
        write_status = kIOReturnNotReady;
        write_buffer = bytes;
        const auto started = IOHIDDeviceSetReportWithCallback(device, kIOHIDReportTypeOutput, 2,
            reinterpret_cast<const uint8_t*>(write_buffer.data()), static_cast<CFIndex>(write_buffer.size()),
            0.25, sent, &write_status);
        if (started != kIOReturnSuccess) throw std::runtime_error("HID output report failed");
        const auto deadline = now() + Milliseconds(300);
        while (write_status == kIOReturnNotReady && now() < deadline && !failed) wait(1);
        if (write_status != kIOReturnSuccess || failed)
            throw std::runtime_error("HID output report failed or timed out; delivery unknown");
        return bytes.size();
    }
};
}

ExchangeResult exchange_darwin_hid(const TransportOptions& options, const std::vector<std::string>& commands) {
    auto manager = hid_manager();
    auto devices = IOHIDManagerCopyDevices(manager);
    HidIO io;
    try {
        if (devices) {
            std::vector<const void*> values(static_cast<std::size_t>(CFSetGetCount(devices)));
            CFSetGetValues(devices, values.data());
            if (options.device.empty() && values.size() != 1)
                throw std::runtime_error("select one exact HID device identity");
            for (auto value : values) {
                auto device = static_cast<IOHIDDeviceRef>(const_cast<void*>(value));
                if (options.device.empty() || options.device == hid_token(device)) {
                    io.device = device;
                    CFRetain(device);
                    break;
                }
            }
        }
    } catch (...) {
        if (devices) CFRelease(devices);
        CFRelease(manager);
        throw;
    }
    if (devices) CFRelease(devices);
    CFRelease(manager);
    if (!io.device) throw std::runtime_error("selected HID identity is not currently enumerated");
    if (IOHIDDeviceOpen(io.device, kIOHIDOptionsTypeSeizeDevice) != kIOReturnSuccess)
        throw std::runtime_error("cannot open selected ZIDE HID device exclusively");
    io.opened = true;
    IOHIDDeviceRegisterInputReportCallback(io.device, io.buffer.data(), io.buffer.size(), HidIO::report, &io);
    IOHIDDeviceRegisterRemovalCallback(io.device, HidIO::removed, &io);
    IOHIDDeviceScheduleWithRunLoop(io.device, CFRunLoopGetCurrent(), kCFRunLoopDefaultMode);
    return exchange_io(io, true, options, commands);
}
}
#endif
