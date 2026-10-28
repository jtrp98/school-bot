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
if command -v sha256sum >/dev/null; then
  (cd "$output" && sha256sum "mixly-engine$suffix" > "mixly-engine$suffix.sha256")
else
  (cd "$output" && shasum -a 256 "mixly-engine$suffix" > "mixly-engine$suffix.sha256")
fi
if [[ "$run_tests" == true ]]; then
  MIXLY_ENGINE="$output/mixly-engine$suffix" node --test "$source_dir/tests/ipc.test.mjs"
fi
echo "Mixly engine ready: $output/mixly-engine$suffix"
