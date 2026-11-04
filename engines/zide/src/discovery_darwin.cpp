#include "transport_darwin.hpp"

#ifdef __APPLE__
#include <IOKit/IOKitLib.h>
#include <IOKit/hid/IOHIDKeys.h>
#include <IOKit/serial/IOSerialKeys.h>
#include <algorithm>
#include <stdexcept>

namespace zide::detail {
std::string hid_token(IOHIDDeviceRef device) {
    uint64_t identity = 0;
    if (IORegistryEntryGetRegistryEntryID(IOHIDDeviceGetService(device), &identity) != KERN_SUCCESS)
        throw std::runtime_error("cannot read HID registry identity");
    return "darwin-hid:" + std::to_string(identity);
}

IOHIDManagerRef hid_manager() {
    auto manager = IOHIDManagerCreate(kCFAllocatorDefault, kIOHIDOptionsTypeNone);
    if (!manager) throw std::runtime_error("cannot create HID manager");
    const int vendor = 0x0483, product = 0x5750;
    auto vendor_number = CFNumberCreate(nullptr, kCFNumberIntType, &vendor);
    auto product_number = CFNumberCreate(nullptr, kCFNumberIntType, &product);
    const void* keys[] = {CFSTR(kIOHIDVendorIDKey), CFSTR(kIOHIDProductIDKey)};
    const void* values[] = {vendor_number, product_number};
    auto matching = CFDictionaryCreate(nullptr, keys, values, 2,
                                      &kCFTypeDictionaryKeyCallBacks, &kCFTypeDictionaryValueCallBacks);
    IOHIDManagerSetDeviceMatching(manager, matching);
    CFRelease(matching);
    CFRelease(vendor_number);
    CFRelease(product_number);
    // CopyDevices performs matching without opening any output handle during discovery.
    return manager;
}

std::vector<SerialDevice> serial_devices() {
    io_iterator_t iterator = IO_OBJECT_NULL;
    if (IOServiceGetMatchingServices(kIOMainPortDefault, IOServiceMatching(kIOSerialBSDServiceValue),
                                    &iterator) != KERN_SUCCESS)
        throw std::runtime_error("cannot enumerate serial callout devices");
    std::vector<SerialDevice> devices;
    for (io_object_t service = IOIteratorNext(iterator); service; service = IOIteratorNext(iterator)) {
        uint64_t identity = 0;
        const auto status = IORegistryEntryGetRegistryEntryID(service, &identity);
        auto property = IORegistryEntryCreateCFProperty(service, CFSTR(kIOCalloutDeviceKey), nullptr, 0);
        IOObjectRelease(service);
        char path[4096]{};
        if (status == KERN_SUCCESS && property && CFGetTypeID(property) == CFStringGetTypeID() &&
            CFStringGetCString(static_cast<CFStringRef>(property), path, sizeof(path), kCFStringEncodingUTF8) &&
            std::string(path).rfind("/dev/cu.", 0) == 0) {
            devices.push_back({"darwin-serial:" + std::to_string(identity), path});
        }
        if (property) CFRelease(property);
    }
    IOObjectRelease(iterator);
    return devices;
}

std::vector<Device> darwin_devices() {
    std::vector<Device> result;
    auto manager = hid_manager();
    auto devices = IOHIDManagerCopyDevices(manager);
    try {
        if (devices) {
            std::vector<const void*> values(static_cast<std::size_t>(CFSetGetCount(devices)));
            CFSetGetValues(devices, values.data());
            for (auto value : values) {
                auto device = static_cast<IOHIDDeviceRef>(const_cast<void*>(value));
                result.push_back({"hid", hid_token(device), "ZIDE USB HID (0483:5750)"});
            }
        }
        for (const auto& device : serial_devices()) result.push_back({"serial", device.token, device.callout});
    } catch (...) {
        if (devices) CFRelease(devices);
        CFRelease(manager);
        throw;
    }
    if (devices) CFRelease(devices);
    CFRelease(manager);
    std::sort(result.begin(), result.end(), [](const Device& a, const Device& b) { return a.path < b.path; });
    return result;
}
}
#endif
