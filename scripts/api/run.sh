#!/usr/bin/env bash
set -euo pipefail
root="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
cd "$root/web"
exec dotnet run --project src/ServiceSchoolBot.API --no-launch-profile
