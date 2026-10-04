"""Compile against the installed Mac game; write patch artifacts only here."""
import json
import hashlib
import os
import re
import subprocess
import struct
import shutil
import zlib
from collections import Counter
from pathlib import Path
import UnityPy

ROOT = Path(__file__).resolve().parent
PACK = Path(os.environ.get('DRESSMAKER_TRANSLATION_PACK', ROOT.parent / 'translation'))
metadata = json.loads((PACK / 'pack.json').read_text())
assert metadata['format_version'] == 1 and metadata['language'] == 'he'
assert metadata['tested_build'] == '25508059'
assert set(metadata['tables']) == {'UI', 'Tutorial', 'Content', 'Dialogue'}
for info in metadata['tables'].values():
    assert (PACK / info['file']).is_file(), 'Translation table missing: ' + info['file']
DATA = Path.home() / 'Library/Application Support/Steam/steamapps/common/Dressmaker/Dressmaker.app/Contents/Resources/Data'
BUNDLE = Path('StreamingAssets/aa/StandaloneOSX/localization-string-tables-english(en)_assets_all.bundle')
OUT = ROOT / 'build'
OUT.mkdir(exist_ok=True)
for font in (ROOT / 'fonts').glob('*.ttf'):
    shutil.copy2(font, OUT / ('DressmakerHebrew-' + font.name))
shutil.copy2(ROOT / 'fonts/FONT-LICENSES.txt', OUT / 'DressmakerHebrew-FONT-LICENSES.txt')
BACKUP = ROOT / 'backups/build-25508059'
def original(relative):
    saved = BACKUP / relative
    return saved if saved.exists() else DATA / relative
SDK = Path(os.environ.get('DRESSMAKER_DOTNET', '/tmp/dressmaker-dotnet'))
refs = ['mscorlib', 'netstandard', 'System', 'System.Core', 'UnityEngine.CoreModule',
        'UnityEngine.TextRenderingModule', 'UnityEngine.TextCoreFontEngineModule', 'Unity.TextMeshPro', 'UnityEngine.UI']
subprocess.run([str(SDK / 'dotnet'), str(next((SDK / 'sdk').glob('*/Roslyn/bincore/csc.dll'))),
                '-nologo', '-target:library', '-nostdlib+', '-out:' + str(OUT / 'DressmakerHebrew.dll')]
               + ['-r:' + str(DATA / 'Managed' / (name + '.dll')) for name in refs]
               + [str(ROOT / 'HebrewRuntime.cs'), str(ROOT / 'HebrewText.cs')]
               + [str(p) for p in (ROOT / 'vendor/RTLTMPro').glob('*.cs')], check=True)
subprocess.run([str(SDK / 'dotnet'), str(OUT / 'patcher/Patcher.dll'), str(DATA / 'Managed'),
                str(OUT / 'DressmakerHebrew.dll'), str(OUT / 'Unity.TextMeshPro.dll'),
                str(original('Managed/Unity.TextMeshPro.dll'))], check=True)

environment = UnityPy.load(str(original(BUNDLE)))
assert len(environment.file.files) == 1, 'CRC calculation expects one serialized file'
old_crc = zlib.crc32(next(iter(environment.file.files.values())).reader.bytes)
count = 0
expected = {}
for obj in environment.objects:
    if obj.type.name != 'MonoBehaviour':
        continue
    table = obj.read_typetree()
    name = table['m_Name'].removesuffix('_en')
    translated = PACK / (name + '_he.json')
    if not translated.exists():
        continue
    rows = json.loads(translated.read_text())
    mapping = {row['id']: row for row in rows}
    assert len(mapping) == len(rows), 'Duplicate IDs'
    found = set()
    for row in table['m_TableData']:
        key = str(row['m_Id'])
        if key not in mapping:
            continue
        tr = mapping[key]
        source = row['m_Localized']
        assert hashlib.sha256(source.encode()).hexdigest() == tr['source_sha256'], 'Game text changed: ' + key
        assert tr['he'].strip(), 'Empty translation: ' + key
        tokens = lambda s: Counter(re.findall(r'<[^>]+>|\{[^{}]+\}', s))
        assert tokens(source) == tokens(tr['he']), 'Markup/placeholder mismatch: ' + key
        if source.startswith('QuestGiver:'):
            assert tr['he'].startswith('QuestGiver:'), 'Yarn speaker marker changed: ' + key
        row['m_Localized'] = tr['he']
        expected[(table['m_Name'], key)] = tr['he']
        found.add(key)
        count += 1
    assert found == set(mapping), 'Translation ID missing from game'
    obj.save_typetree(table)
assert count == sum(t['entries'] for t in metadata['tables'].values()), 'Pack coverage mismatch'
output = OUT / BUNDLE.name
output.write_bytes(environment.file.save())
actual = {}
for obj in UnityPy.load(str(output)).objects:
    if obj.type.name == 'MonoBehaviour':
        table = obj.read_typetree()
        actual.update({(table['m_Name'], str(r['m_Id'])): r['m_Localized'] for r in table['m_TableData']})
assert all(actual[key] == value for key, value in expected.items())
readback = UnityPy.load(str(output))
new_crc = zlib.crc32(next(iter(readback.file.files.values())).reader.bytes)
catalog = original('StreamingAssets/aa/catalog.bin').read_bytes()
old_bytes = struct.pack('<I', old_crc)
assert catalog.count(old_bytes) == 1, 'Cannot uniquely locate English bundle CRC'
(OUT / 'catalog.bin').write_bytes(catalog.replace(old_bytes, struct.pack('<I', new_crc)))
print(f'Built and read back {count} Hebrew entries. Original game files untouched.')
