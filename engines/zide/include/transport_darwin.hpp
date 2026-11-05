#pragma once

#include "transport.hpp"

#ifdef __APPLE__
#include <IOKit/hid/IOHIDManager.h>

namespace zide::detail {
struct SerialDevice { std::string token, callout; };
std::vector<SerialDevice> serial_devices();
std::string hid_token(IOHIDDeviceRef);
IOHIDManagerRef hid_manager();
std::vector<Device> darwin_devices();
ExchangeResult exchange_darwin_hid(const TransportOptions&, const std::vector<std::string>&);
ExchangeResult exchange_darwin_serial(const TransportOptions&, const std::vector<std::string>&);
}
#endif
