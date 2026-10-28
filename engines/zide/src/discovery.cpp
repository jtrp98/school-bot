#include "transport.hpp"
#include "transport_windows.hpp"
#include <stdexcept>

#ifdef _WIN32
#include <setupapi.h>
#include <array>
#include <cctype>
#include <memory>
#include <vector>
#include <type_traits>

namespace zide::detail {
namespace {
struct DeviceSet {
    HDEVINFO value;
    ~DeviceSet() { if (value != INVALID_HANDLE_VALUE) SetupDiDestroyDeviceInfoList(value); }
};

bool matches_board(HDEVINFO devices, SP_DEVINFO_DATA& info) {
    std::array<char, 4096> hardware_ids{};
    DWORD type = 0;
    if (!SetupDiGetDeviceRegistryPropertyA(devices, &info, SPDRP_HARDWAREID, &type,
            reinterpret_cast<BYTE*>(hardware_ids.data()), static_cast<DWORD>(hardware_ids.size()), nullptr)) {
        if (GetLastError() == ERROR_INVALID_DATA) return false;
        throw win_error("read HID hardware IDs");
    }
    if (type != REG_MULTI_SZ) return false;
    std::string ids(hardware_ids.data(), hardware_ids.size());
    std::transform(ids.begin(), ids.end(), ids.begin(), [](unsigned char c) {
        return static_cast<char>(std::tolower(c));
    });
    const std::string target = "hid\\vid_0483&pid_5750";
    std::size_t start = 0;
    while (start < ids.size()) {
        const auto end = ids.find('\0', start);
        const auto length = end == std::string::npos ? ids.size() - start : end - start;
        if (length >= target.size() && ids.compare(start, target.size(), target) == 0 &&
            (length == target.size() || ids[start + target.size()] == '&')) return true;
        if (end == std::string::npos || length == 0) break;
        start = end + 1;
    }
    return false;
}
}

std::vector<std::string> hid_paths() {
    const GUID hid_class = {0x4d1e55b2, 0xf16f, 0x11cf, {0x88, 0xcb, 0x00, 0x11, 0x11, 0x00, 0x00, 0x30}};
    DeviceSet devices{SetupDiGetClassDevsA(&hid_class, nullptr, nullptr, DIGCF_PRESENT | DIGCF_DEVICEINTERFACE)};
    if (devices.value == INVALID_HANDLE_VALUE) throw win_error("enumerate HID interfaces");
    std::vector<std::string> paths;
    for (DWORD index = 0; ; ++index) {
        SP_DEVICE_INTERFACE_DATA interface_data{};
        interface_data.cbSize = sizeof(interface_data);
        if (!SetupDiEnumDeviceInterfaces(devices.value, nullptr, &hid_class, index, &interface_data)) {
            if (GetLastError() == ERROR_NO_MORE_ITEMS) break;
            throw win_error("enumerate HID interface");
        }
        DWORD size = 0;
        SetupDiGetDeviceInterfaceDetailA(devices.value, &interface_data, nullptr, 0, &size, nullptr);
        if (GetLastError() != ERROR_INSUFFICIENT_BUFFER || size < sizeof(SP_DEVICE_INTERFACE_DETAIL_DATA_A)) {
            throw win_error("size HID interface path");
        }
        std::vector<unsigned char> storage(size);
        auto* detail = reinterpret_cast<SP_DEVICE_INTERFACE_DETAIL_DATA_A*>(storage.data());
        detail->cbSize = sizeof(SP_DEVICE_INTERFACE_DETAIL_DATA_A);
        SP_DEVINFO_DATA info{};
        info.cbSize = sizeof(info);
        if (!SetupDiGetDeviceInterfaceDetailA(devices.value, &interface_data, detail, size, nullptr, &info)) {
            throw win_error("read HID interface path");
        }
        // SetupAPI exposes identity and paths without opening output handles on every HID interface.
        if (matches_board(devices.value, info)) paths.emplace_back(detail->DevicePath);
    }
    std::sort(paths.begin(), paths.end());
    paths.erase(std::unique(paths.begin(), paths.end()), paths.end());
    return paths;
}
}

namespace zide {
std::vector<Device> list_devices() {
    std::vector<Device> devices;
    for (const auto& path : detail::hid_paths()) {
        devices.push_back({"hid", path, "ZIDE USB HID (0483:5750)"});
    }
    HKEY raw_key = nullptr;
    const LONG opened = RegOpenKeyExA(HKEY_LOCAL_MACHINE, "HARDWARE\\DEVICEMAP\\SERIALCOMM",
                                     0, KEY_QUERY_VALUE, &raw_key);
    if (opened == ERROR_SUCCESS) {
        const auto close_key = [](HKEY key) { RegCloseKey(key); };
        std::unique_ptr<std::remove_pointer_t<HKEY>, decltype(close_key)> key(raw_key, close_key);
        for (DWORD index = 0; ; ++index) {
            std::array<char, 16384> name{};
            std::array<char, 16384> value{};
            DWORD name_size = static_cast<DWORD>(name.size());
            DWORD value_size = static_cast<DWORD>(value.size());
            DWORD type = 0;
            const LONG status = RegEnumValueA(key.get(), index, name.data(), &name_size, nullptr,
                                              &type, reinterpret_cast<BYTE*>(value.data()), &value_size);
            if (status == ERROR_NO_MORE_ITEMS) break;
            if (status != ERROR_SUCCESS) {
                throw std::runtime_error("serial discovery failed (Windows error " + std::to_string(status) + ")");
            }
            if (type == REG_SZ && value_size > 0 && value_size <= value.size()) {
                value.back() = 0;
                std::string port(value.data());
                if (detail::valid_port(port)) devices.push_back({"serial", port, port});
            }
        }
    } else if (opened != ERROR_FILE_NOT_FOUND && opened != ERROR_PATH_NOT_FOUND) {
        throw std::runtime_error("serial discovery failed (Windows error " + std::to_string(opened) + ")");
    }
    std::sort(devices.begin(), devices.end(), [](const Device& a, const Device& b) { return a.path < b.path; });
    const auto same = [](const Device& a, const Device& b) {
        return a.transport == b.transport && a.path == b.path;
    };
    devices.erase(std::unique(devices.begin(), devices.end(), same), devices.end());
    return devices;
}
}
#else
namespace zide {
std::vector<Device> list_devices() {
    throw std::runtime_error("native ZIDE hardware discovery is supported only on Windows");
}
}
#endif
