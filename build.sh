#!/bin/bash
set -e

APP_NAME="WallpaperPicker"
PROJECT_NAME="WallpaperPicker"
OS=$(uname -s)

# ==========================================
# macOS → .app Bundle
# ==========================================
if [ "$OS" = "Darwin" ]; then
    APP_BUNDLE="${APP_NAME}.app"
    MACOS_DIR="${APP_BUNDLE}/Contents/MacOS"

    if [ "$(uname -m)" = "arm64" ]; then
        RUNTIME="osx-arm64"
        echo "🍏 Apple Silicon detected."
    else
        RUNTIME="osx-x64"
        echo "💻 Intel processor detected."
    fi

    echo "🚀 Building $APP_NAME for $RUNTIME..."
    rm -rf "$APP_BUNDLE"

    dotnet publish -c Release -r $RUNTIME --self-contained true -p:PublishSingleFile=true -o "$MACOS_DIR"
    find "$MACOS_DIR" -name "*.pdb" -type f -delete

    cat > "${APP_BUNDLE}/Contents/Info.plist" <<EOF
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>CFBundleExecutable</key>
    <string>$PROJECT_NAME</string>
    <key>CFBundleName</key>
    <string>$APP_NAME</string>
    <key>CFBundlePackageType</key>
    <string>APPL</string>
    <key>NSHighResolutionCapable</key>
    <true/>
</dict>
</plist>
EOF

    chmod +x "$MACOS_DIR/$PROJECT_NAME"
    echo "✅ Done! App bundle: $(pwd)/$APP_BUNDLE"

# ==========================================
# Linux → self-contained binary
# ==========================================
elif [ "$OS" = "Linux" ]; then
    if [ "$(uname -m)" = "aarch64" ]; then
        RUNTIME="linux-arm64"
        echo "🐧 Linux ARM64 detected."
    else
        RUNTIME="linux-x64"
        echo "🐧 Linux x64 detected."
    fi

    OUT_DIR="dist"
    echo "🚀 Building $APP_NAME for $RUNTIME..."
    rm -rf "$OUT_DIR"

    dotnet publish -c Release -r $RUNTIME --self-contained true -p:PublishSingleFile=true -o "$OUT_DIR"
    find "$OUT_DIR" -name "*.pdb" -type f -delete
    chmod +x "$OUT_DIR/$PROJECT_NAME"

    echo "✅ Done! Binary: $(pwd)/$OUT_DIR/$PROJECT_NAME"

else
    echo "❌ Unsupported OS: $OS. Use build.ps1 on Windows."
    exit 1
fi
