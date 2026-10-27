"""Run the reader on every slot and backup in two already-copied save trees.

Usage: python -I -X utf8 corpus.py SNAPSHOT_DIR PRIVATE_OUTPUT_DIR
Never operates on the live trees. Reports failures rather than counting them as parses.
"""
import json
import pathlib
import subprocess
import sys
import tempfile
import zipfile
from collections import defaultdict

snapshot, output = map(pathlib.Path, sys.argv[1:3])
output.mkdir(parents=True, exist_ok=True)
reader = pathlib.Path(__file__).with_name('advisor.ts')
rows = []
for host in ('flagg', 'roland'):
    tree = snapshot / host
    entries = [(p.name, p, False) for p in sorted(tree.glob('SaveGame_*')) if p.is_dir()]
    entries += [(p.name, p, True) for p in sorted((tree / 'Backups').glob('*.zip'))]
    for name, source, is_zip in entries:
        with tempfile.TemporaryDirectory(prefix='advisor-corpus-') as tmp:
            root = pathlib.Path(tmp)
            try:
                if is_zip:
                    with zipfile.ZipFile(source) as z:
                        for info in z.infolist():
                            target = (root / info.filename).resolve()
                            if not target.is_relative_to(root.resolve()):
                                raise ValueError('zip entry escapes destination')
                        z.extractall(root)
                    candidates = list(root.rglob('Game.json'))
                    if len(candidates) != 1:
                        raise ValueError('Game.json $: expected exactly one save root')
                    save = candidates[0].parent
                else:
                    save = source
                try:
                    version = json.loads((save / 'Game.json').read_text(encoding='utf-8-sig')).get('GameVersion', 'unknown')
                except (OSError, ValueError):
                    version = 'unknown'
                result = subprocess.run(['node', str(reader), str(save), str(root / 'digest')], capture_output=True, text=True, encoding='utf-8')
                rows.append({'host': host, 'entry': name, 'version': version, 'status': 'parse' if result.returncode == 0 else 'fail', 'detail': result.stderr.strip(), 'exit': result.returncode})
            except (OSError, ValueError, zipfile.BadZipFile) as e:
                rows.append({'host': host, 'entry': name, 'version': 'unknown', 'status': 'fail', 'detail': str(e), 'exit': 1})
groups = defaultdict(lambda: {'parse': 0, 'fail': 0, 'errors': set()})
for row in rows:
    g = groups[row['version']]
    g[row['status']] += 1
    if row['detail']:
        g['errors'].add(row['detail'])
table = [{'version': version, 'parse': group['parse'], 'fail': group['fail'], 'errors': sorted(group['errors'])} for version, group in sorted(groups.items())]
(output / 'corpus.json').write_text(json.dumps({'rows': rows, 'byVersion': table}, indent=2) + '\n', encoding='utf-8')
print(f"accounted={len(rows)} parsed={sum(r['status']=='parse' for r in rows)} failed={sum(r['status']=='fail' for r in rows)}")
for row in table:
    print(f"{row['version']}: parse={row['parse']} fail={row['fail']}; " + '; '.join(row['errors']))
