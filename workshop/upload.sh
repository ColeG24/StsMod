#!/usr/bin/env bash
# Publish the mod, copy the fresh build into workshop/content, and upload it to the Steam Workshop
# with Mega Crit's uploader (downloaded into tools/ModUploader on first run).
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
WS="$ROOT/workshop"
PROJ="$ROOT/DrunkenMaster"
TOOLS="$ROOT/tools/ModUploader"
UPLOADER_VERSION="v0.2.0"
ARCH="$(uname -m)"; [[ "$ARCH" == "arm64" ]] && RID="osx-arm64" || RID="osx-x64"
UPLOADER_URL="https://github.com/megacrit/sts2-mod-uploader/releases/download/$UPLOADER_VERSION/ModUploader-$RID.zip"
MODS_DIR="$HOME/Library/Application Support/Steam/steamapps/common/Slay the Spire 2/SlayTheSpire2.app/Contents/MacOS/mods/DrunkenMaster"

DO_PUBLISH=1; DO_UPLOAD=1
for a in "$@"; do
  case "$a" in
    --no-publish) DO_PUBLISH=0 ;;
    --dry-run) DO_UPLOAD=0 ;;
    *) echo "usage: $0 [--no-publish] [--dry-run]"; exit 2 ;;
  esac
done

export DOTNET_ROOT="$HOME/.dotnet"; export PATH="$HOME/.dotnet:$PATH"

if [[ -n "$(git -C "$ROOT" status --porcelain --untracked-files=no)" ]]; then
  echo "!! Working tree has uncommitted changes; the deployed version will be stamped -dirty." >&2
  echo "   Commit first, or press Enter to continue anyway." >&2; read -r
fi

if (( DO_PUBLISH )); then
  echo ">> dotnet publish"
  (cd "$PROJ" && dotnet publish)
fi

# Sync the build from the game's mods folder. With --no-publish and no local copy (deleted after
# subscribing to the Workshop item), reuse whatever is already staged in content/.
if [[ -f "$MODS_DIR/DrunkenMaster.pck" ]]; then
  rm -rf "$WS/content"; mkdir -p "$WS/content/DrunkenMaster"
  cp "$MODS_DIR"/DrunkenMaster.{json,dll,pck} "$WS/content/DrunkenMaster/"
elif (( DO_PUBLISH )); then
  echo "publish did not produce $MODS_DIR/DrunkenMaster.pck" >&2; exit 1
else
  echo ">> no local mods copy; reusing the build already staged in content/"
fi
for f in DrunkenMaster.json DrunkenMaster.dll DrunkenMaster.pck; do
  [[ -f "$WS/content/DrunkenMaster/$f" ]] || { echo "missing $WS/content/DrunkenMaster/$f" >&2; exit 1; }
done
echo ">> content:"; ls -la "$WS/content/DrunkenMaster"
grep -o '"version": *"[^"]*"' "$WS/content/DrunkenMaster/DrunkenMaster.json"

IMG_BYTES=$(stat -f%z "$WS/image.png")
(( IMG_BYTES < 1000000 )) || { echo "image.png is $IMG_BYTES bytes; Steam requires < 1 MB" >&2; exit 1; }

if [[ ! -x "$TOOLS/ModUploader" ]]; then
  echo ">> downloading uploader $UPLOADER_VERSION ($RID) to $TOOLS"
  mkdir -p "$TOOLS"
  curl -fL "$UPLOADER_URL" -o "$TOOLS/ModUploader.zip"
  (cd "$TOOLS" && unzip -oq ModUploader.zip && rm ModUploader.zip)
  BIN="$(find "$TOOLS" -type f -name ModUploader | head -1)"
  [[ -n "$BIN" ]] || { echo "ModUploader binary not found in the zip" >&2; exit 1; }
  if [[ "$(dirname "$BIN")" != "$TOOLS" ]]; then mv "$(dirname "$BIN")"/* "$TOOLS/"; fi
  chmod +x "$TOOLS/ModUploader"
  xattr -dr com.apple.quarantine "$TOOLS" 2>/dev/null || true
fi

pgrep -x steam_osx >/dev/null || { echo "Steam is not running; start it and log in first." >&2; exit 1; }

if (( DO_UPLOAD )); then
  echo ">> uploading"
  (cd "$TOOLS" && ./ModUploader upload -w "$WS")
  [[ -f "$WS/mod_id.txt" ]] && echo ">> Workshop item: https://steamcommunity.com/sharedfiles/filedetails/?id=$(cat "$WS/mod_id.txt")"
else
  echo ">> dry run: skipping upload. Would run: (cd $TOOLS && ./ModUploader upload -w $WS)"
fi
