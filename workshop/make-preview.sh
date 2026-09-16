#!/usr/bin/env bash
# Rebuild image.png (Workshop preview, < 1 MB) from the character select backdrop and figure.
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
IMG="$ROOT/DrunkenMaster/DrunkenMaster/images/character"
OUT="$ROOT/workshop/image.png"
TMP="$(mktemp -d)"
cp "$ROOT/workshop/composite.swift" "$TMP/"
(cd "$TMP" && swiftc -O composite.swift -o composite 2>/dev/null)
for size in "1280 720" "1024 576" "960 540" "800 450"; do
  set -- $size
  "$TMP/composite" "$IMG/select_bg.png" "$IMG/select_figure.png" "$OUT" "$1" "$2"
  bytes=$(stat -f%z "$OUT")
  echo "${1}x${2}: $bytes bytes"
  (( bytes < 1000000 )) && break
done
rm -rf "$TMP"
