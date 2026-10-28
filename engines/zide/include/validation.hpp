#pragma once

#include "protocol.hpp"
#include <cstdint>
#include <optional>

namespace zide {
[[noreturn]] inline void invalid(const std::string& message) {
    throw std::runtime_error(message);
}

inline void require(bool condition, const std::string& message) {
    if (!condition) invalid(message);
}

inline int integer(const json& data, const char* key, int low, int high,
                   std::optional<int> fallback = std::nullopt) {
    if (!data.contains(key)) {
        require(fallback.has_value(), std::string(key) + " is required");
        return *fallback;
    }
    const auto& number = data[key];
    require(number.is_number_integer(), std::string(key) + " must be an integer");
    if (number.is_number_unsigned()) {
        require(number.get<std::uint64_t>() <= static_cast<std::uint64_t>(high),
                std::string(key) + " outside range");
    }
    const auto value = number.get<std::int64_t>();
    require(value >= low && value <= high,
            std::string(key) + " outside " + std::to_string(low) + ".." + std::to_string(high));
    return static_cast<int>(value);
}
}
