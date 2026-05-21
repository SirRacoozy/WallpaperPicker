#!/bin/bash

# ==========================================
# 1. KONFIGURATION
# ==========================================
# Wie soll die fertige App heißen? (Das, was im Finder steht)
APP_NAME="WallpaperPicker"

# Wie heißt dein .NET Projekt? (Exakt der Name des Ordners / der .csproj Datei)
PROJECT_NAME="WallpaperPicker"

APP_BUNDLE="${APP_NAME}.app"
MACOS_DIR="${APP_BUNDLE}/Contents/MacOS"

# ==========================================
# 2. ARCHITEKTUR ERKENNEN
# ==========================================
if [ "$(uname -m)" = "arm64" ]; then
    RUNTIME="osx-arm64"
    echo "🍏 Apple Silicon (M-Chip) erkannt."
else
    RUNTIME="osx-x64"
    echo "💻 Intel Prozessor erkannt."
fi

echo "🚀 Baue $APP_NAME für $RUNTIME..."

# ==========================================
# 3. BAUEN UND VERPACKEN
# ==========================================
# Alten Ordner löschen, falls vorhanden
rm -rf "$APP_BUNDLE"

# .NET anweisen, alles direkt in den MacOS-Unterordner des Bundles zu kompilieren
dotnet publish -c Release -r $RUNTIME --self-contained true -p:PublishSingleFile=true -o "$MACOS_DIR"

# Debug-Dateien aufräumen
find "$MACOS_DIR" -name "*.pdb" -type f -delete

# ==========================================
# 4. INFO.PLIST ERSTELLEN (Für macOS)
# ==========================================
# Das sagt macOS, dass es sich um eine vollwertige App handelt
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

# ==========================================
# 5. RECHTE SETZEN
# ==========================================
# Macht die Datei ausführbar
chmod +x "$MACOS_DIR/$PROJECT_NAME"

echo "✅ Fertig! Deine App liegt hier: $(pwd)/$APP_BUNDLE"
