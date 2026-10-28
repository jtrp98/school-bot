#!/usr/bin/env bash
set -euo pipefail
source_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
case "$(uname -s)" in Darwin) platform=darwin ;; Linux) platform=linux ;; *) echo 'Use build.ps1 on Windows.' >&2; exit 1 ;; esac
case "${ENGINE_ARCH:-${ZIDE_CLANG_ARCH:-$(uname -m)}}" in
  x86_64|amd64) arch=amd64 ;;
  aarch64|arm64) arch=arm64 ;;
  *)
    echo 'Unsupported engine architecture.' >&2
    exit 1
    ;;
esac
output="$source_dir/bin/$platform-$arch"
test_requested=false
output_set=false
for argument in "$@"; do
  case "$argument" in
    --test) test_requested=true ;;
    --*)
      echo "Unknown engine build option: $argument" >&2
      exit 1
      ;;
    *)
      if [[ "$output_set" == true ]]; then
        echo 'Usage: build.sh [output-directory] [--test]' >&2
        exit 1
      fi
      output="$argument"
      output_set=true
      ;;
  esac
done
mkdir -p "$output"
build_dir="$source_dir/.build/$platform-$arch"
mkdir -p "$build_dir"
cxx="${CXX:-c++}"
flags=(-std=c++17 -O2 -DNDEBUG -g0 -fvisibility=hidden -I"$source_dir/include" -I"$source_dir/third_party")
if [[ "$platform" == darwin ]]; then link_flags=(-Wl,-S,-x); else link_flags=(-s); fi
if [[ "$platform" == darwin ]]; then
  clang_arch=x86_64
  if [[ "$arch" == arm64 ]]; then
    clang_arch=arm64
  fi
  flags+=(-arch "$clang_arch")
fi
sources=(protocol transport transport_hid transport_serial discovery actions request line_reader main)
source_files=()
for source in "${sources[@]}"; do
  source_files+=("$source_dir/src/$source.cpp")
done
"$cxx" "${flags[@]}" "${link_flags[@]}" "${source_files[@]}" -o "$output/zide-engine"
if [[ "$test_requested" == true ]]; then
  "$cxx" "${flags[@]}" "$source_dir/src/protocol.cpp" "$source_dir/tests/protocol_test.cpp" -o "$build_dir/zide-protocol-tests"
  "$build_dir/zide-protocol-tests"
fi
cp "$source_dir/THIRD-PARTY-NOTICES.md" "$output/THIRD-PARTY-NOTICES.md"
cp "$source_dir/third_party/hidapi/LICENSE-bsd.txt" "$output/HIDAPI-LICENSE.txt"
cp "$source_dir/third_party/nlohmann/LICENSE.MIT" "$output/JSON-LICENSE.txt"
cp "$source_dir/engine.json" "$output/engine.json"
