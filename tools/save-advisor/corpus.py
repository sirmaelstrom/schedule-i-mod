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
discovered = {}
for host in ('flagg', 'roland'):
    tree = snapshot / host
    if not tree.is_dir():
        raise SystemExit(f'{host}: snapshot tree missing: {tree}')
    entries = [(p.name, p, False) for p in sorted(tree.glob('SaveGame_*')) if p.is_dir()]
    entries += [(p.name, p, True) for p in sorted((tree / 'Backups').glob('*.zip'))]
    discovered[host] = {'slots': sum(not e[2] for e in entries), 'zips': sum(e[2] for e in entries)}
    if not discovered[host]['slots']:
        raise SystemExit(f'{host}: no save slots discovered')
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
                presence = {}
                for filename, keys in {'Money.json': ['OnlineBalance','Networth','LifetimeEarnings','WeeklyDepositSum'], 'Time.json':['ElapsedDays','TimeOfDay'], 'Products.json':['DiscoveredProducts','MixRecipes','ProductPrices']}.items():
                    try:
                        doc = json.loads((save / filename).read_text(encoding='utf-8-sig'))
                        for key in keys:
                            presence[f'{filename}:$.{key}'] = key in doc
                    except (OSError, ValueError, TypeError):
                        for key in keys:
                            presence[f'{filename}:$.{key}'] = False
                try:
                    npc_doc = json.loads((save / 'NPCs.json').read_text(encoding='utf-8-sig'))
                    customers = [n for n in npc_doc['NPCs'] if any(a.get('Name') == 'CustomerData' for a in n.get('AdditionalDatas', []))]
                    relation = {'present': sum(any(a.get('Name') == 'Relationship' for a in n['AdditionalDatas']) for n in customers), 'absent': sum(not any(a.get('Name') == 'Relationship' for a in n['AdditionalDatas']) for n in customers)}
                    nested = {key: {'present': 0, 'absent': 0, 'malformed': 0} for key in ('Dependence','ProductAffinities','TimeSinceLastDealCompleted','TimeSinceInstantDealOffered','CompletedDeals')}
                    for npc in customers:
                        try:
                            customer_data = json.loads(next(a['Contents'] for a in npc['AdditionalDatas'] if a.get('Name') == 'CustomerData'))
                            for key in nested:
                                nested[key]['present' if key in customer_data else 'absent'] += 1
                        except (KeyError, StopIteration, TypeError, ValueError):
                            for value in nested.values(): value['malformed'] += 1
                except (OSError, ValueError, TypeError, KeyError):
                    relation, nested = None, {}
                result = subprocess.run(['node', str(reader), str(save), str(root / 'digest')], capture_output=True, text=True, encoding='utf-8')
                rows.append({'host': host, 'entry': name, 'version': version, 'status': 'parse' if result.returncode == 0 else 'fail', 'detail': result.stderr.strip(), 'exit': result.returncode, 'fieldPresence': presence, 'customerRelationships': relation, 'nestedCustomerFields': nested})
            except (OSError, ValueError, zipfile.BadZipFile) as e:
                rows.append({'host': host, 'entry': name, 'version': 'unknown', 'status': 'fail', 'detail': str(e), 'exit': 1})
if len(rows) != sum(v['slots'] + v['zips'] for v in discovered.values()):
    raise SystemExit('corpus accounting mismatch')
groups = defaultdict(lambda: {'parse': 0, 'fail': 0, 'errors': set(), 'fields': defaultdict(lambda: {'present': 0, 'absent': 0}), 'relationships': {'present': 0, 'absent': 0}, 'nested': defaultdict(lambda: {'present': 0, 'absent': 0, 'malformed': 0})})
for row in rows:
    g = groups[row['version']]
    g[row['status']] += 1
    if row['detail']:
        g['errors'].add(row['detail'])
    for key, present in row.get('fieldPresence', {}).items():
        g['fields'][key]['present' if present else 'absent'] += 1
    for key, value in (row.get('customerRelationships') or {}).items():
        g['relationships'][key] += value
    for field_name, counts in row.get('nestedCustomerFields', {}).items():
        for status, count in counts.items(): g['nested'][field_name][status] += count
table = [{'version': version, 'parse': group['parse'], 'fail': group['fail'], 'errors': sorted(group['errors']), 'fieldPresence': dict(group['fields']), 'customerRelationships': group['relationships'], 'nestedCustomerFields': dict(group['nested'])} for version, group in sorted(groups.items())]
(output / 'corpus.json').write_text(json.dumps({'discovered': discovered, 'rows': rows, 'byVersion': table}, indent=2) + '\n', encoding='utf-8')
print(f"accounted={len(rows)} parsed={sum(r['status']=='parse' for r in rows)} failed={sum(r['status']=='fail' for r in rows)}")
for row in table:
    print(f"{row['version']}: parse={row['parse']} fail={row['fail']} relationship-absent={row['customerRelationships']['absent']}; " + '; '.join(row['errors']))
