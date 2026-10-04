"""Install/restore only the patch files; preserve originals and check hashes."""
import hashlib
import json
import shutil
import subprocess
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parent
DATA = Path.home() / 'Library/Application Support/Steam/steamapps/common/Dressmaker/Dressmaker.app/Contents/Resources/Data'
FILES = ['Managed/Unity.TextMeshPro.dll', 'Managed/DressmakerHebrew.dll',
         'StreamingAssets/aa/StandaloneOSX/localization-string-tables-english(en)_assets_all.bundle',
         'StreamingAssets/aa/catalog.bin']
FILES += ['Managed/DressmakerHebrew-' + font.name for font in sorted((ROOT / 'fonts').glob('*.ttf'))]
FILES += ['Managed/DressmakerHebrew-FONT-LICENSES.txt']
BACKUP = ROOT / 'backups/build-25508059'
MANIFEST = BACKUP / 'manifest.json'
digest = lambda p: hashlib.sha256(p.read_bytes()).hexdigest() if p.exists() else None
process_status = subprocess.run(['pgrep', '-f', '/Dressmaker.app/Contents/MacOS/Dressmaker'], capture_output=True).returncode
if process_status == 0:
    raise SystemExit('Close Dressmaker before installing or restoring.')
if process_status != 1:
    raise SystemExit('Cannot check whether Dressmaker is running; no files changed.')
restore = sys.argv[1:] == ['restore']
if sys.argv[1:] not in ([], ['restore']):
    raise SystemExit('Usage: install.py [restore]')
state = json.loads(MANIFEST.read_text()) if MANIFEST.exists() else {}
if restore:
    if not state:
        raise SystemExit('No complete backup available.')
    FILES = list(state)
for rel in FILES:
    destination = DATA / rel
    if rel in state:
        if digest(destination) not in (state[rel]['original'], state[rel]['installed']):
            raise SystemExit('Game file changed; refusing to overwrite: ' + rel)
    elif restore:
        raise SystemExit('No complete backup available.')
    elif Path(rel).name.startswith('DressmakerHebrew') and destination.exists():
        raise SystemExit('An unknown Hebrew patch file is already installed: ' + rel)
    if not restore and not (ROOT / 'build' / Path(rel).name).is_file():
        raise SystemExit('Missing build artifact: ' + rel)
for rel in FILES:
    if rel not in state:
        original = DATA / rel
        saved = BACKUP / rel
        saved.parent.mkdir(parents=True, exist_ok=True)
        if original.exists():
            shutil.copy2(original, saved)
        state[rel] = {'original': digest(original), 'installed': digest(ROOT / 'build' / Path(rel).name)}
MANIFEST.write_text(json.dumps(state, indent=2) + '\n')
for rel in FILES:
    target = DATA / rel
    source = BACKUP / rel if restore else ROOT / 'build' / Path(rel).name
    if restore and state[rel]['original'] is None:
        target.unlink(missing_ok=True)
    else:
        temporary = target.with_name(target.name + '.hebrew-tmp')
        shutil.copy2(source, temporary)
        temporary.replace(target)
    expected = state[rel]['original'] if restore else digest(source)
    assert digest(target) == expected
    if not restore:
        state[rel]['installed'] = expected
MANIFEST.write_text(json.dumps(state, indent=2) + '\n')
print('Originals restored.' if restore else 'Hebrew prototype installed; all file hashes verified.')
