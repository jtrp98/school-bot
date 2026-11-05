#!/usr/bin/env bash
set -euo pipefail
source_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
root="$(cd "$source_dir/../.." && pwd)"
output=''
run_tests=false
for argument in "$@"; do
  case "$argument" in
    --test) run_tests=true ;;
    --*) echo "Unknown option: $argument" >&2; exit 1 ;;
    *) if [[ -n "$output" ]]; then echo 'Only one output directory is allowed.' >&2; exit 1; fi; output="$argument" ;;
  esac
done
go_command=go
if [[ -x "$root/.tools/go/bin/go" ]]; then go_command="$root/.tools/go/bin/go"; fi
export GOWORK=off
cd "$source_dir"
platform="$("$go_command" env GOOS)"
arch="${ENGINE_ARCH:-$("$go_command" env GOARCH)}"
export GOARCH="$arch" CGO_ENABLED=0
if [[ "$platform" == darwin ]]; then
  # The board list's USB IDs come from IOKit, which go.bug.st/serial reaches only through cgo;
  # without it the enumerator does not build. -arch lets one Mac build both thin binaries.
  clang_arch=x86_64
  if [[ "$arch" == arm64 ]]; then clang_arch=arm64; fi
  export CGO_ENABLED=1 CGO_CFLAGS="${CGO_CFLAGS:-} -arch $clang_arch" CGO_LDFLAGS="${CGO_LDFLAGS:-} -arch $clang_arch"
fi
suffix=''
if [[ "$platform" == windows ]]; then suffix=.exe; fi
if [[ -z "$output" ]]; then output="$source_dir/bin/$platform-$arch"; fi
mkdir -p "$output/licenses"
output="$(cd "$output" && pwd)"
if [[ "$run_tests" == true ]]; then
  "$go_command" test -mod=readonly ./...
  "$go_command" vet -mod=readonly ./...
fi
"$go_command" build -mod=readonly -trimpath -buildvcs=false -ldflags '-s -w' -o "$output/mixly-engine$suffix" .
cp "$source_dir/engine.json" "$source_dir/THIRD-PARTY-NOTICES.md" "$output/"
cp "$source_dir"/licenses/*.txt "$output/licenses/"
# arduino-cli compiles these Arduino libraries from source, so they ship as they are vendored.
# Replace the copy so a library removed from the source tree does not linger in the payload.
rm -rf "$output/libraries"
cp -R "$source_dir/libraries" "$output/libraries"
if command -v sha256sum >/dev/null; then
  (cd "$output" && sha256sum "mixly-engine$suffix" > "mixly-engine$suffix.sha256")
else
  (cd "$output" && shasum -a 256 "mixly-engine$suffix" > "mixly-engine$suffix.sha256")
fi
if [[ "$run_tests" == true ]]; then
  MIXLY_ENGINE="$output/mixly-engine$suffix" node --test "$source_dir/tests/ipc.test.mjs"
fi
echo "Mixly engine ready: $output/mixly-engine$suffix"
