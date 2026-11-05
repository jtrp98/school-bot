#!/usr/bin/env bash
set -euo pipefail

# 1. Create wrapper
cat > /usr/local/bin/zcc-darwin-arm64 << 'EOF'
#!/bin/bash
echo "[CC] $@" >> /tmp/cc.log
exec zig cc -target aarch64-macos-none -isysroot /opt/macos-sdk -L/opt/macos-sdk/usr/lib -F/opt/macos-sdk/System/Library/Frameworks -Wno-elaborated-enum-base -Wl,-headerpad,1000 -w "$@"
EOF
chmod +x /usr/local/bin/zcc-darwin-arm64

cat > /usr/local/bin/zcc-darwin-amd64 << 'EOF'
#!/bin/bash
exec zig cc -target x86_64-macos-none -isysroot /opt/macos-sdk -L/opt/macos-sdk/usr/lib -F/opt/macos-sdk/System/Library/Frameworks -Wno-elaborated-enum-base -Wl,-headerpad,1000 -w "$@"
EOF
chmod +x /usr/local/bin/zcc-darwin-amd64

cat > /usr/local/bin/zcxx-darwin-arm64 << 'EOF'
#!/bin/bash
exec zig c++ -target aarch64-macos-none -isysroot /opt/macos-sdk -L/opt/macos-sdk/usr/lib -F/opt/macos-sdk/System/Library/Frameworks -Wno-elaborated-enum-base -Wl,-headerpad,1000 -w "$@"
EOF
chmod +x /usr/local/bin/zcxx-darwin-arm64

cat > /usr/local/bin/zcxx-darwin-amd64 << 'EOF'
#!/bin/bash
exec zig c++ -target x86_64-macos-none -isysroot /opt/macos-sdk -L/opt/macos-sdk/usr/lib -F/opt/macos-sdk/System/Library/Frameworks -Wno-elaborated-enum-base -Wl,-headerpad,1000 -w "$@"
EOF
chmod +x /usr/local/bin/zcxx-darwin-amd64

ln -sf /usr/bin/llvm-lipo-14 /usr/local/bin/lipo
ln -sf /usr/bin/llvm-lipo-14 /usr/local/bin/llvm-lipo

export GOTOOLCHAIN=auto
export GOPATH=/tmp/gopath
export GOCACHE=/tmp/gocache

cd /workspace/app
echo "Building demo-engine..."
CGO_ENABLED=0 GOOS=darwin GOARCH=arm64 go build -trimpath -buildvcs=false -ldflags '-s -w' -o /tmp/demo-arm64 ./cmd/demo-engine
CGO_ENABLED=0 GOOS=darwin GOARCH=amd64 go build -trimpath -buildvcs=false -ldflags '-s -w' -o /tmp/demo-amd64 ./cmd/demo-engine
lipo -create /tmp/demo-arm64 /tmp/demo-amd64 -output /tmp/demo-universal
lipo -info /tmp/demo-universal
echo "demo-engine universal: OK"

echo "Building zide-engine..."
zide_sources="/workspace/engines/zide/src/protocol.cpp /workspace/engines/zide/src/transport.cpp /workspace/engines/zide/src/transport_hid.cpp /workspace/engines/zide/src/transport_serial.cpp /workspace/engines/zide/src/discovery.cpp /workspace/engines/zide/src/actions.cpp /workspace/engines/zide/src/request.cpp /workspace/engines/zide/src/line_reader.cpp /workspace/engines/zide/src/transport_io.cpp /workspace/engines/zide/src/discovery_darwin.cpp /workspace/engines/zide/src/transport_darwin_hid.cpp /workspace/engines/zide/src/transport_darwin_serial.cpp /workspace/engines/zide/src/main.cpp"

zcxx-darwin-arm64 -std=c++17 -O2 -DNDEBUG -g0 -fvisibility=hidden -I/workspace/engines/zide/include -I/workspace/engines/zide/third_party -framework IOKit -framework CoreFoundation $zide_sources -o /tmp/zide-arm64
zcxx-darwin-amd64 -std=c++17 -O2 -DNDEBUG -g0 -fvisibility=hidden -I/workspace/engines/zide/include -I/workspace/engines/zide/third_party -framework IOKit -framework CoreFoundation $zide_sources -o /tmp/zide-amd64
lipo -create /tmp/zide-arm64 /tmp/zide-amd64 -output /tmp/zide-universal
lipo -info /tmp/zide-universal
echo "zide-engine universal: OK"
