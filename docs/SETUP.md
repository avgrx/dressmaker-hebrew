# Install the Hebrew patch on macOS

This guide builds the patch locally from your own Steam copy of Dressmaker.
It supports **macOS Steam build 25508059 (game version 410)**. Windows and custom
Steam library locations are not supported by the current scripts.

Run the numbered steps in order. Stop if a command reports an error; do not
continue to installation with a failed build. Keep Dressmaker closed until step 6.

## 1. Check the game

Install Dressmaker through Steam, launch it once, then close it. The scripts expect:

```text
~/Library/Application Support/Steam/steamapps/common/Dressmaker/Dressmaker.app
```

Check the installed Steam build from Terminal:

```sh
grep '"buildid"' "$HOME/Library/Application Support/Steam/steamapps/appmanifest_4019220.acf"
```

It must report **25508059**. If it reports another build, this adapter needs to be
updated and tested for that version first. Do not install an old generated patch
on a newer game. The builder checks the original strings as an additional guard.

For a first installation, start with an unmodified game. If an earlier Hebrew
patch is already installed, restore it using that patch's original installer and
backups before switching to a fresh clone. Re-running the build is supported when
this clone already contains its own `adapter/backups/build-25508059/` originals.

## 2. Install the tools

You need Git, Python 3.11 or newer, GitHub CLI, and the **.NET 9 SDK** (not just the
.NET runtime). These are build tools; they do not replace your game.

If needed, install [Homebrew](https://brew.sh/) using its official instructions.
Then install the command-line tools:

```sh
brew install git python gh
```

Download and run the **macOS .NET 9 SDK installer** from
[Microsoft's .NET 9 download page](https://dotnet.microsoft.com/en-us/download/dotnet/9.0).
Choose **Arm64** for an Apple Silicon Mac or **x64** for an Intel Mac. You can check
which you have in Apple menu → About This Mac. Open a new Terminal after installation.

Verify the tools:

```sh
python3 --version
git --version
gh --version
dotnet --list-sdks
```

The last command must include a version starting with `9.`. If `dotnet` is not
found, follow [Microsoft's macOS installation/troubleshooting guide](https://learn.microsoft.com/en-us/dotnet/core/install/macos).

## 3. Download this repository

The repository is currently private. Your GitHub account needs access from its
owner before you can clone it. Sign in as an account with access:

```sh
gh auth login --hostname github.com --git-protocol https --web
```

Complete the browser sign-in, then choose a folder for the project:

```sh
mkdir -p "$HOME/Projects"
cd "$HOME/Projects"
gh repo clone avgrx/dressmaker-hebrew
cd dressmaker-hebrew
```

For all remaining commands, stay in this `dressmaker-hebrew` directory. If you
already cloned it, open Terminal in that directory instead of cloning it again.

## 4. Set up the build environment

Create a Python virtual environment so dependencies stay separate from other projects:

```sh
python3 -m venv .venv
source .venv/bin/activate
python -m pip install -r adapter/requirements.txt
```

Point the adapter to the installed .NET SDK root. This finds the directory containing
its actual `dotnet` executable, including when your shell finds it through a symlink:

```sh
export DRESSMAKER_DOTNET="$(python -c 'import pathlib,shutil; print(pathlib.Path(shutil.which("dotnet")).resolve().parent)')"
"$DRESSMAKER_DOTNET/dotnet" --list-sdks
```

The SDK root must contain both `dotnet` and an `sdk/` directory. This environment
variable and the virtual environment activation apply to the current Terminal
session. Repeat this step's `source` and `export` commands after reopening Terminal.

## 5. Build and install

Build the adapter's assembly patcher, then generate the Hebrew game files:

```sh
"$DRESSMAKER_DOTNET/dotnet" build adapter/Patcher/Patcher.csproj -o adapter/build/patcher
python adapter/build.py
```

The build should finish with:

```text
Patched TMP string ingestion; original game DLL untouched.
Built and read back 5886 Hebrew entries. Original game files untouched.
```

The builder writes generated files into `adapter/build/`; it has not installed
anything yet. With Dressmaker still closed, install:

```sh
python adapter/install.py
```

Success ends with:

```text
Hebrew prototype installed; all file hashes verified.
```

Original files are backed up to `adapter/backups/build-25508059/`. Keep this folder
and the repository: the uninstall command depends on them. Backups and generated
game files are ignored by Git. The installer does not modify saves.

## 6. Play in Hebrew

Launch Dressmaker normally through Steam. Keep its language set to **English**;
the patch replaces that language's text tables. Hebrew is not a new entry in the
language menu. Menus, tutorials, descriptions and dialogue should now use Hebrew.

![Hebrew sewing screen](images/hebrew-sewing-screen.png)

This remains a beta. Some long text, mixed numbers, punctuation and layouts need
more gameplay testing; image-embedded and hardcoded text may remain untranslated.

## Update the translation

Close the game. From this same clone, update and rebuild:

```sh
git pull --ff-only
source .venv/bin/activate
```

Repeat step 4's `export DRESSMAKER_DOTNET=...`, then the build and install commands
in step 5. The builder uses `translation/` by default. To use a different compatible
pack, set `DRESSMAKER_TRANSLATION_PACK` to that pack's directory before building.
The game does not load JSON changes directly; every translation update needs a rebuild.

If Steam has updated Dressmaker to a different build, stop and wait for a compatible
adapter. Old backups belong to the old build; do not restore them over an updated game.

## Uninstall and return to English

Close Dressmaker. From the same clone used to install it:

```sh
source .venv/bin/activate
python adapter/install.py restore
```

Success ends with `Originals restored.` This restores the original English game
files and removes the added Hebrew files. Save files remain untouched.

## Troubleshooting

| Message or symptom | What to do |
| --- | --- |
| Repository not found / permission denied | Sign in to GitHub with an account that has access to this private repository. |
| `No module named UnityPy` | Activate `.venv` and run the dependency installation in step 4. |
| SDK executable or `Patcher.dll` missing | Check `DRESSMAKER_DOTNET`, install the .NET 9 SDK, and run the first build command in step 5. |
| `Game text changed`, a missing translation ID, or a pack coverage mismatch | Check the Steam build and whether the game is already patched. Use the original files for the tested build; do not bypass validation. |
| `Close Dressmaker before installing or restoring` | Save, quit the game completely, and retry. |
| `Game file changed; refusing to overwrite` | The files differ from the recorded originals/patch, often after a Steam update. Do not force an overwrite. Check compatibility first. |
| Unknown Hebrew patch file already installed | Use the earlier patch's installer and backups to restore it before switching clones. |
| Game folder missing | The scripts currently expect Steam's default macOS library path from step 1. |
| Game remains English | Confirm the install success message, leave the game language on English, and fully restart it. |

For a rendering issue, include a screenshot, the Steam build ID, and the relevant
screen/dialogue when reporting it. Do not upload saves, credentials, or game binaries.
