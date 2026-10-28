#!/usr/bin/env bash
set -euo pipefail
root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
if [[ "$(uname -s)" != Darwin ]]; then echo 'This launcher requires macOS.' >&2; exit 1; fi
if [[ "${1:-}" == --skip-build ]]; then shift; else bash "$root/scripts/build-desktop.sh" --skip-install; fi
if [[ $# -ne 0 ]]; then echo 'Usage: bash scripts/start-desktop.sh [--skip-build]' >&2; exit 1; fi
go_command=go
if [[ -x "$root/.tools/go/bin/go" ]]; then go_command="$root/.tools/go/bin/go"; fi
arch="$("$go_command" env GOHOSTARCH)"
bundle="$root/app/bin/darwin-$arch/School Bot.app"
if [[ ! -d "$bundle" ]]; then echo 'Build first: bash scripts/build-desktop.sh' >&2; exit 1; fi
open "$bundle"
