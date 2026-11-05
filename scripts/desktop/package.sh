#!/usr/bin/env bash
set -euo pipefail
root="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
if [[ "$(uname -s)" != Darwin ]]; then
  echo 'Build the macOS .pkg on a Mac with Xcode Command Line Tools.' >&2
  exit 1
fi
node "$root/scripts/checks/third-party-notices.mjs"
skip_build=false
require_signed=false
build_flags=(--universal)
while [[ $# -gt 0 ]]; do
  case "$1" in
    --skip-build) skip_build=true ;;
    --skip-install) build_flags+=(--skip-install) ;;
    --require-signed) require_signed=true ;;
    *) echo 'Usage: bash scripts/desktop/package.sh [--skip-build] [--skip-install] [--require-signed]' >&2; exit 1 ;;
  esac
  shift
done
if [[ "$require_signed" == true ]]; then
  if [[ -z "${SCHOOL_BOT_BUILD_API_URL:-}" || ! "${SCHOOL_BOT_BUILD_API_URL}" =~ ^https:// ]]; then
    echo 'Release signing requires SCHOOL_BOT_BUILD_API_URL to be set to an HTTPS Gateway endpoint (SEC001-008).' >&2
    exit 1
  fi
  if [[ -z "${APPLE_DEVELOPER_ID_APP:-}" || -z "${APPLE_DEVELOPER_ID_INSTALLER:-}" || -z "${APPLE_TEAM_ID:-}" ]]; then
    echo 'Release signing requires APPLE_DEVELOPER_ID_APP, APPLE_DEVELOPER_ID_INSTALLER, and APPLE_TEAM_ID (BE-022 / SEC001-003).' >&2
    exit 1
  fi
fi
if [[ "$skip_build" == false ]]; then bash "$root/scripts/desktop/build.sh" "${build_flags[@]}"; fi
bundle="$root/app/bin/darwin-universal/School Bot.app"
for executable in school-bot demo-engine engines/zide/zide-engine; do
  test -x "$bundle/Contents/MacOS/$executable"
  lipo -verify_arch x86_64 arm64 "$bundle/Contents/MacOS/$executable"
done
for required_asset in \
  "Contents/Resources/THIRD-PARTY-NOTICES.md" \
  "Contents/MacOS/engines/zide/engine.json" \
  "Contents/MacOS/engines/zide/THIRD-PARTY-NOTICES.md" \
  "Contents/MacOS/engines/zide/HIDAPI-LICENSE.txt" \
  "Contents/MacOS/engines/zide/JSON-LICENSE.txt"; do
  test -f "$bundle/$required_asset"
done
if [[ -n "${APPLE_DEVELOPER_ID_APP:-}" ]]; then
  for executable in engines/zide/zide-engine demo-engine school-bot; do
    codesign --force --options runtime --timestamp --sign "$APPLE_DEVELOPER_ID_APP" "$bundle/Contents/MacOS/$executable"
  done
  codesign --force --options runtime --timestamp --sign "$APPLE_DEVELOPER_ID_APP" "$bundle"
fi
codesign --verify --deep --strict "$bundle"
version="$(/usr/libexec/PlistBuddy -c 'Print :CFBundleShortVersionString' "$bundle/Contents/Info.plist")"
output_dir="$root/dist/installers"
mkdir -p "$output_dir"
installer="$output_dir/school-bot-$version-macos-universal.pkg"
pkg_sign_args=()
if [[ -n "${APPLE_DEVELOPER_ID_INSTALLER:-}" ]]; then
  pkg_sign_args=(--sign "$APPLE_DEVELOPER_ID_INSTALLER" --timestamp)
fi
productbuild --component "$bundle" /Applications \
  --product "$root/app/build/macos/installer-requirements.plist" \
  --identifier local.schoolbot.poc.installer --version "$version" \
  "${pkg_sign_args[@]}" "$installer"
if [[ -n "${APPLE_NOTARY_PROFILE:-}" ]]; then
  xcrun notarytool submit "$installer" --keychain-profile "$APPLE_NOTARY_PROFILE" --wait
  xcrun stapler staple "$installer"
  xcrun stapler validate "$installer"
fi
cp "$root/THIRD-PARTY-NOTICES.md" "$installer.NOTICES.txt"
(cd "$output_dir" && shasum -a 256 "$(basename "$installer")" > "$(basename "$installer").sha256")
echo "Desktop installer ready: $installer"
if [[ -z "${APPLE_DEVELOPER_ID_INSTALLER:-}" ]]; then
  echo 'Development .pkg is unsigned and contains an ad-hoc signed app. Developer ID signing and notarization are required before external distribution.'
fi
