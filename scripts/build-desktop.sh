#!/usr/bin/env bash
set -euo pipefail
root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
if [[ "$(uname -s)" != Darwin ]]; then
  echo 'Build macOS packages on a Mac with Xcode Command Line Tools.' >&2
  exit 1
fi
skip_install=false
universal=false
while [[ $# -gt 0 ]]; do
  case "$1" in
    --skip-install) skip_install=true ;;
    --universal) universal=true ;;
    *) echo 'Usage: bash scripts/build-desktop.sh [--skip-install] [--universal]' >&2; exit 1 ;;
  esac
  shift
done
link_flags=(-ldflags '-s -w')
if [[ -n "${SCHOOL_BOT_BUILD_API_URL:-}" ]]; then
  api_pattern="$(tr -d '\r\n' < "$root/scripts/api-url-pattern.txt")"
  if [[ ! "$SCHOOL_BOT_BUILD_API_URL" =~ $api_pattern ]]; then
    echo 'SCHOOL_BOT_BUILD_API_URL must be HTTPS (HTTP only for localhost),' \
      'without credentials, query, fragment or spaces.' >&2
    exit 1
  fi
  link_flags=(-ldflags "-s -w -X main.defaultAPIURL=$SCHOOL_BOT_BUILD_API_URL")
  echo "Built-in API endpoint: $SCHOOL_BOT_BUILD_API_URL"
fi
go_command=go
if [[ -x "$root/.tools/go/bin/go" ]]; then go_command="$root/.tools/go/bin/go"; fi
command -v "$go_command" >/dev/null
command -v node >/dev/null
xcrun --find clang >/dev/null
arch="$("$go_command" env GOHOSTARCH)"
case "$arch" in amd64|arm64) ;; *) echo "Unsupported Mac architecture: $arch" >&2; exit 1 ;; esac
export GOOS=darwin GOARCH="$arch" CGO_ENABLED=1 MACOSX_DEPLOYMENT_TARGET=15.0
target="darwin-$arch"
if [[ "$universal" == true ]]; then target=darwin-universal; fi
cd "$root/app/frontend"
if [[ "$skip_install" == false ]]; then npm ci --no-audit --no-fund; fi
npm run build
version="$(node -p 'JSON.parse(require("node:fs").readFileSync("package.json", "utf8")).version')"
if [[ ! "$version" =~ ^[0-9]+\.[0-9]+\.[0-9]+$ ]]; then echo 'package.json version must be x.y.z.' >&2; exit 1; fi
link_flags[1]+=" -X main.appVersion=$version"
bundle="$root/app/bin/$target/School Bot.app"
mkdir -p "$bundle/Contents/MacOS" "$bundle/Contents/Resources"
cp "$root/app/build/macos/Info.plist" "$bundle/Contents/Info.plist"
plutil -replace CFBundleShortVersionString -string "$version" "$bundle/Contents/Info.plist"
plutil -replace CFBundleVersion -string "$version" "$bundle/Contents/Info.plist"
cd "$root/app"
if [[ "$universal" == true ]]; then
  for thin_arch in amd64 arm64; do
    clang_arch=x86_64
    if [[ "$thin_arch" == arm64 ]]; then clang_arch=arm64; fi
    thin_dir="$root/app/bin/$target/thin/$thin_arch"
    mkdir -p "$thin_dir"
    ENGINE_ARCH="$thin_arch" bash "$root/scripts/build-engine.sh" zide "$thin_dir/engines/zide"
    GOARCH="$thin_arch" \
      CGO_CFLAGS="${CGO_CFLAGS:-} -arch $clang_arch" \
      CGO_LDFLAGS="${CGO_LDFLAGS:-} -arch $clang_arch" \
      "$go_command" build -trimpath -buildvcs=false -ldflags '-s -w' -o "$thin_dir/demo-engine" ./cmd/demo-engine
    GOARCH="$thin_arch" \
      CGO_CFLAGS="${CGO_CFLAGS:-} -arch $clang_arch" \
      CGO_LDFLAGS="${CGO_LDFLAGS:-} -arch $clang_arch" \
      "$go_command" build -trimpath -buildvcs=false -tags production "${link_flags[@]}" -o "$thin_dir/school-bot" .
  done
  for executable in demo-engine school-bot; do
    lipo -create \
      "$root/app/bin/$target/thin/amd64/$executable" \
      "$root/app/bin/$target/thin/arm64/$executable" \
      -output "$bundle/Contents/MacOS/$executable"
    lipo -verify_arch x86_64 arm64 "$bundle/Contents/MacOS/$executable"
  done
  zide_bundle="$bundle/Contents/MacOS/engines/zide"
  mkdir -p "$zide_bundle"
  cp -R "$root/app/bin/$target/thin/amd64/engines/zide/." "$zide_bundle/"
  lipo -create \
    "$root/app/bin/$target/thin/amd64/engines/zide/zide-engine" \
    "$root/app/bin/$target/thin/arm64/engines/zide/zide-engine" \
    -output "$zide_bundle/zide-engine"
  lipo -verify_arch x86_64 arm64 "$zide_bundle/zide-engine"
else
  ENGINE_ARCH="$arch" bash "$root/scripts/build-engine.sh" zide "$bundle/Contents/MacOS/engines/zide"
  "$go_command" build -trimpath -buildvcs=false -ldflags '-s -w' \
    -o "$bundle/Contents/MacOS/demo-engine" ./cmd/demo-engine
  "$go_command" build -trimpath -buildvcs=false -tags production "${link_flags[@]}" \
    -o "$bundle/Contents/MacOS/school-bot" .
fi
cp "$root/THIRD-PARTY-NOTICES.md" "$bundle/Contents/Resources/THIRD-PARTY-NOTICES.md"
chmod 755 \
  "$bundle/Contents/MacOS/demo-engine" \
  "$bundle/Contents/MacOS/school-bot" \
  "$bundle/Contents/MacOS/engines/zide/zide-engine"
plutil -lint "$bundle/Contents/Info.plist"
codesign --force --sign - "$bundle/Contents/MacOS/engines/zide/zide-engine"
codesign --force --sign - "$bundle/Contents/MacOS/demo-engine"
codesign --force --sign - "$bundle/Contents/MacOS/school-bot"
codesign --force --sign - "$bundle"
codesign --verify --deep --strict "$bundle"
# ditto preserves executable permissions and the complete app bundle in the ZIP.
ditto -c -k --sequesterRsrc --keepParent "$bundle" "$root/app/bin/$target/school-bot-$target.zip"
echo "Ready: $bundle"
echo "Local development build ($target)." \
  'Developer ID signing and notarization are required before external distribution.'
