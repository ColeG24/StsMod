# Steam Workshop workspace

Layout expected by Mega Crit's [sts2-mod-uploader](https://github.com/megacrit/sts2-mod-uploader):

- `workshop.json` — title, description (Steam BBCode), visibility, tags, `changeNote`, Workshop dependencies (BaseLib = 3737335127).
- `image.png` — preview shown on the Workshop page, must be under 1 MB. Regenerate with `make-preview.sh`.
- `previews/` — additional store-page images (each under 1 MB). The uploader mirrors this folder: files here are added or updated by name, and any additional preview on Steam not present here is removed. Edit previews here, not on the Steam website, or the next upload undoes it. Steam's page shows these in the order they were added, and shows `image.png` last as the square thumbnail, so the splash art is duplicated here as `01_splash.png` to lead the gallery.
- `content/DrunkenMaster/` — the `.json`, `.dll` and `.pck` to upload. Gitignored; `upload.sh` fills it from a fresh publish.
- `mod_id.txt` — written by the uploader after the first upload. Commit it: later uploads update the same item.

## Releasing

```bash
workshop/upload.sh              # publish, sync content, upload
workshop/upload.sh --dry-run    # everything except the upload
workshop/upload.sh --no-publish # reuse the build already in the game's mods folder
```

Before uploading: commit everything (the deployed version is `v0.1.<commit count>` and gets `-dirty` otherwise), and set `changeNote` in `workshop.json`. Steam must be running and logged in; the uploader talks to the local Steam client.
