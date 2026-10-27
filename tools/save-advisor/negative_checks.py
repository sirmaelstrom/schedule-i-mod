"""Produce explicit red/refusal evidence from a copied save and a private brief.

python -I -X utf8 negative_checks.py SNAPSHOT_SAVE BRIEF OUTPUT_DIR
"""
import json
import pathlib
import shutil
import subprocess
import sys
import tempfile

source, brief_path, output = map(pathlib.Path, sys.argv[1:4])
output.mkdir(parents=True, exist_ok=True)
reader = pathlib.Path(__file__).with_name('advisor.ts')
lint = pathlib.Path(__file__).with_name('lint-brief.ts')
results = []
with tempfile.TemporaryDirectory(prefix='advisor-red-') as tmp:
    root = pathlib.Path(tmp)
    save, out = root / 'save', root / 'out'
    shutil.copytree(source, save)
    def call(*args):
        r = subprocess.run(args, text=True, encoding='utf-8', capture_output=True)
        return {'exit': r.returncode, 'stdout': r.stdout.strip(), 'stderr': r.stderr.strip()}
    def record(name, run, expected):
        run.update(name=name, expected=expected)
        results.append(run)
    def write(name, obj):
        (save / name).write_text(json.dumps(obj), encoding='utf-8')
    def read(name):
        return json.loads((save / name).read_text(encoding='utf-8-sig'))
    def run():
        return call('node', str(reader), str(save), str(out))
    assert run()['exit'] == 0
    original = json.loads((out / 'digest.json').read_text(encoding='utf-8'))
    product = read('Products.json')
    price = next(x for x in product['ProductPrices'] if x['String'] == 'ogkush')
    old = price['Int']; price['Int'] = old + 1; write('Products.json', product)
    r = run(); assert r['exit'] == 0
    changed = json.loads((out / 'digest.json').read_text(encoding='utf-8'))
    before = next(x['price']['value'] for x in original['products']['prices'] if x['id']['value'] == 'ogkush')
    after = next(x['price']['value'] for x in changed['products']['prices'] if x['id']['value'] == 'ogkush')
    red = call(sys.executable, '-I', '-X', 'utf8', '-c', 'import sys; assert sys.argv[1] == sys.argv[2], "old price assertion failed"', str(before), str(after))
    red['stdout'] = f'original={before} mutated={after}; ' + red['stdout']
    record('product price mutation: old-value assertion', red, 'exit 1 / FAIL')
    price['Int'] = old; write('Products.json', product)
    money = read('Money.json'); (save / 'Money.json').unlink()
    record('missing file', run(), 'nonzero refusal naming Money.json $')
    write('Money.json', money); del money['LifetimeEarnings']; write('Money.json', money)
    record('missing field', run(), 'nonzero refusal naming Money.json $.LifetimeEarnings')
    money['LifetimeEarnings'] = original['economy']['LifetimeEarnings']['value']; write('Money.json', money)
    npcs = read('NPCs.json'); base = npcs['NPCs'][0]['BaseData']; npcs['NPCs'][0]['BaseData'] = '{broken'; write('NPCs.json', npcs)
    record('malformed nested JSON', run(), 'nonzero refusal naming NPCs.json $.NPCs[0].BaseData')
    npcs['NPCs'][0]['BaseData'] = base; write('NPCs.json', npcs)
    game = read('Game.json'); version = game['GameVersion']
    for v in ('0.0.0', 'unknown'):
        game['GameVersion'] = v; write('Game.json', game); r = run()
        r['header'] = (out / 'digest.md').read_text(encoding='utf-8').splitlines()[2]
        record('version ' + v, r, 'exit 0 and WARNING header')
    game['GameVersion'] = version; write('Game.json', game); assert run()['exit'] == 0
    brief = json.loads(brief_path.read_text(encoding='utf-8'))
    local_brief = root / 'brief.json'
    def lint_run():
        local_brief.write_text(json.dumps(brief), encoding='utf-8')
        return call('node', str(lint), str(local_brief), str(out / 'digest.json'))
    record('valid brief', lint_run(), 'exit 0')
    old = brief['plays'][0]['citations'][0]['value']; brief['plays'][0]['citations'][0]['value'] = 'wrong'
    record('wrong cited value', lint_run(), 'nonzero value mismatch')
    brief['plays'][0]['citations'][0]['value'] = old; brief['plays'][0]['rules'] = ['M999']
    record('fabricated rule', lint_run(), 'nonzero unknown mechanics rule')
(output / 'negative-checks.json').write_text(json.dumps(results, indent=2) + '\n', encoding='utf-8')
for r in results:
    print(f"{r['name']}: exit={r['exit']} {r.get('header', '')} {r['stderr']}")
ok = (results[0]['exit'] != 0 and all(results[i]['exit'] != 0 for i in (1,2,3,7,8))
      and all(results[i]['exit'] == 0 and 'WARNING' in results[i]['header'] for i in (4,5))
      and results[6]['exit'] == 0 and after == before + 1
      and 'Money.json $:' in results[1]['stderr']
      and 'Money.json $.LifetimeEarnings:' in results[2]['stderr']
      and 'NPCs.json $.NPCs[0].BaseData:' in results[3]['stderr']
      and 'value mismatch' in results[7]['stderr']
      and 'unknown mechanics rule' in results[8]['stderr'])
sys.exit(0 if ok else 1)
