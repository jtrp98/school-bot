# Zide engine notices

The servo/action-group and HID protocol logic is extracted/adapted from the local
School Bright `sb-app-zide` C++ project. This extraction does not change the
ownership or license of that project or grant a new license to its application code.

HIDAPI 0.7.0 source was copied from `sb-app-zide/hidapi-0.7.0`. We select its
BSD license option for this engine. The complete notice and terms are retained in
`third_party/hidapi/LICENSE-bsd.txt`; the original alternative licenses are also retained.
Local changes to its Windows implementation correct the HidP_GetCaps status check,
bound writes, cancel/drain outstanding operations before closing/freeing memory,
clamp input copy lengths, and validate board identity and 64-byte report sizes.
The header also supports HIDAPI_STATIC so its API is not exported from the engine executable.

JSON for Modern C++ (nlohmann/json) v3.12.0 is distributed under the MIT license,
Copyright (c) 2013–2025 Niels Lohmann. Its complete license is retained in
`third_party/nlohmann/LICENSE.MIT` and the single header. Header SHA256:
`aaf127c04cb31c406e5b04a63f1ae89369fccde6d8fa7cdda1ed4f32dfc5de63`.

## Vendored file provenance

| File | Upstream | Vendored SHA256 (line endings normalized to LF) |
| --- | --- | --- |
| `third_party/hidapi/windows/hid.c` | HIDAPI 0.7.0, modified | `c2cc80b692f43c265a4d46444f784618bdb080b38cd7c2f68fb5ebea59927186` |
| `third_party/hidapi/hidapi/hidapi.h` | HIDAPI 0.7.0, modified | `7ecebb018b98cc5ee34e858c8c0424e4564d8f918cc652526aa3b83936e92938` |
| `third_party/nlohmann/json.hpp` | nlohmann/json 3.12.0 (version declared in header) | `aaf127c04cb31c406e5b04a63f1ae89369fccde6d8fa7cdda1ed4f32dfc5de63` |

HIDAPI: the files were copied from `sb-app-zide/hidapi-0.7.0`. In that copy `windows/hid.c` has SHA256
`f7bc54d9fdee1deb9fe381da74a421a84e1c7d9aa6e31f0903a8a34f2f2cedf4` and `hidapi/hidapi.h` has SHA256
`f1efb5cea95be628d343628d7fde9d463c7e27a2630bf20252e5310662dfd976`; the four license files are byte-identical to it.
The upstream repository commit or tag and the hashes of the upstream 0.7.0 release files were not recorded and have
not been compared with that copy, so they are unknown. The differences from the `sb-app-zide` copy can be regenerated
with a text diff against it.

nlohmann/json: the header declares version 3.12.0. No upstream release hash or commit was recorded, and the file has
not been compared with the upstream release, so those are unknown. The file contains the SPDX copyright lines that
upstream ships in it, including those of other authors for code embedded in the header.

When distributing the engine binary, include this notice and both dependency
license files. The portable build compiler in `.tools` is a development tool and
is not required on the end user's machine.
