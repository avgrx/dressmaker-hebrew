# Dressmaker Hebrew

Unofficial Hebrew localization for the macOS Steam edition of Dressmaker, tested
against build **25508059** (game version 410). This is a beta with all 5,886
extracted strings translated and reviewed. Full gameplay QA remains pending.

## Two separate parts

- `translation/`: standalone UTF-8 Hebrew translation pack, keyed by game string
  IDs. `pack.json` identifies the language, tested build, files and entry counts.
  Each row contains `id`, `he`, and a SHA-256 hash of the original English string.
  The hash lets the adapter reject changed source text without redistributing it.
- `adapter/`: Dressmaker-specific installer, build tools, Hebrew font selection,
  RTL handling, and vendored RTLTMPro source with its license.

The translation pack contains no Unity assemblies, game bundles, original English
text tables, saves, or backups. The adapter builds patched files from the player's
own installed game. Generated game files and local backups are excluded from Git.
Translation updates currently require rebuilding and reinstalling; the game does
not load this pack directly at runtime.

## Developer build on macOS

The existing scripts target the default Steam installation directory. Python 3
with `adapter/requirements.txt` and the .NET 9 SDK are required. Set
`DRESSMAKER_DOTNET` to your .NET SDK root (the directory containing `dotnet` and
`sdk/`); the development fallback is `/tmp/dressmaker-dotnet`.

Use a clean installation of Steam build 25508059 for the first build. Never apply
these generated files to another game build. If already patched, the build needs
this adapter's local `backups/build-25508059/` originals.

```sh
python3 -m pip install -r adapter/requirements.txt
dotnet build adapter/Patcher/Patcher.csproj -o adapter/build/patcher
python3 adapter/build.py
```

To build another compatible translation pack, set `DRESSMAKER_TRANSLATION_PACK`
to its directory. The builder validates source hashes, IDs, markup, placeholders,
and reads all translated values back from the generated bundle.

Close Dressmaker before installation or removal:

```sh
python3 adapter/install.py
python3 adapter/install.py restore
```

Run only the command for the action you want. Keep the game's language set to
**English** after installation. The installer verifies backups and installed
hashes and does not modify saves. Restore returns the original English files.
Steam updates may replace the patch; a new build requires adapter validation.

## Status and third-party licenses

All 208 UI, 71 tutorial, 1,930 content and 3,677 dialogue entries have a Hebrew
translation and received a second language pass. Instructions generally address
one female player. Fonts follow the game's original roles. Hebrew kerning is
disabled to avoid a reproduced RTL punctuation collision; italics remain active.

Still to check: long dialogue, typewriter reveal, settings layout, sewing screens,
measurements, currency sprites and mixed punctuation. Text embedded in images and
hardcoded strings are outside the extracted tables. Windows is not supported yet.

Font licenses are in `adapter/fonts/FONT-LICENSES.txt`. RTLTMPro's MIT license is
in `adapter/vendor/RTLTMPro/LICENSE`. No original game binaries are distributed.
