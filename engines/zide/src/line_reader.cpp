#include "line_reader.hpp"
#include "request.hpp"
#include <iostream>

namespace zide {
namespace {
constexpr std::size_t kMaxLineBytes = 65536;

void respond(const json& response) {
    std::cout << response.dump(-1, ' ', false, json::error_handler_t::replace) << '\n' << std::flush;
}

void handle_line(const std::string& line) {
    try {
        json request;
        try {
            request = json::parse(line);
        } catch (const json::exception&) {
            respond(error_response("", "INVALID_REQUEST", "invalid request JSON"));
            return;
        }
        respond(handle_request(request));
    } catch (const std::exception&) {
        respond(error_response("", "INVALID_REQUEST", "request could not be processed"));
    }
}
}
int serve_lines() {
    // Read bounded lines ourselves: std::getline alone permits unbounded allocation.
    std::string line;
    bool discarding = false;
    char ch;
    while (std::cin.get(ch)) {
        if (ch == '\n') {
            if (!discarding) handle_line(line);
            line.clear();
            discarding = false;
            if (!std::cout) return 1;
            continue;
        }
        if (discarding) continue;
        if (line.size() >= kMaxLineBytes) {
            // Answer now and skip the rest of the line so one oversize request cannot end the session.
            line.clear();
            discarding = true;
            respond(error_response("", "INVALID_REQUEST", "request line exceeds 65536 bytes"));
            if (!std::cout) return 1;
            continue;
        }
        line.push_back(ch);
    }
    if (!discarding && !line.empty()) handle_line(line);
    return std::cin.bad() || !std::cout ? 1 : 0;
}
}
