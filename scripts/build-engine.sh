#!/usr/bin/env bash
set -euo pipefail
root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
engine="${1:-}"
if [[ ! "$engine" =~ ^[a-z][a-z0-9-]*$ ]]; then
  echo 'Usage: bash scripts/build-engine.sh <engine-id> [output-directory] [--test]' >&2
  exit 1
fi
shift
directory="$root/engines/$engine"
if [[ ! -f "$directory/engine.json" || ! -f "$directory/scripts/build.sh" ]]; then
  echo "Unknown engine or missing shell build script: $engine" >&2
  exit 1
fi
exec bash "$directory/scripts/build.sh" "$@"
