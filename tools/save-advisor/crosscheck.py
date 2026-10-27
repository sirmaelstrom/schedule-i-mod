"""Independent first-slice check. python -I -X utf8 crosscheck.py SAVE DIGEST OUTPUT.

Named checks: elapsed days, online balance, net worth, lifetime earnings, weekly
deposits, OG Kush price, Sour Diesel price, and the relationship, dependence,
and time since last completed deal of the first customer with nonzero dependence.
These are raw values, not rates.
"""
import json
import pathlib
import sys

save, digest_path, output = map(pathlib.Path, sys.argv[1:4])
def read(name):
    return json.loads((save / name).read_text(encoding='utf-8-sig'))
d = json.loads(digest_path.read_text(encoding='utf-8'))
t, m, p, n = (read(f) for f in ('Time.json', 'Money.json', 'Products.json', 'NPCs.json'))
first = next(x for x in n['NPCs'] if any(a['Name'] == 'CustomerData' and json.loads(a['Contents'])['Dependence'] != 0 for a in x['AdditionalDatas']))
parts = {a['Name']: json.loads(a['Contents']) for a in first['AdditionalDatas'] if a['Name'] in ('Relationship', 'CustomerData')}
prices = {x['String']: x['Int'] for x in p['ProductPrices']}
digest_prices = {x['id']['value']: x['price']['value'] for x in d['products']['prices']}
customer_id = json.loads(first['BaseData'])['ID']
customer = next(x for x in d['customers'] if x['id']['value'] == customer_id)
checks = [
    ('elapsed days', t['ElapsedDays'], d['day']['value']),
    ('online balance', m['OnlineBalance'], d['economy']['OnlineBalance']['value']),
    ('net worth', m['Networth'], d['economy']['Networth']['value']),
    ('lifetime earnings', m['LifetimeEarnings'], d['economy']['LifetimeEarnings']['value']),
    ('weekly deposits', m['WeeklyDepositSum'], d['economy']['WeeklyDepositSum']['value']),
    ('OG Kush price', prices['ogkush'], digest_prices['ogkush']),
    ('Sour Diesel price', prices['sourdiesel'], digest_prices['sourdiesel']),
    ('selected customer relationship', parts['Relationship']['RelationDelta'], customer['relationship']['value']),
    ('selected customer dependence', parts['CustomerData']['Dependence'], customer['dependence']['value']),
    ('selected customer time since last deal', parts['CustomerData']['TimeSinceLastDealCompleted'], customer['lastDeal']['value']),
]
rows = [{'name': name, 'raw': raw, 'digest': actual, 'match': raw == actual} for name, raw, actual in checks]
output.parent.mkdir(parents=True, exist_ok=True)
output.write_text(json.dumps(rows, indent=2) + '\n', encoding='utf-8')
for row in rows:
    print(f"{row['name']}: raw={row['raw']} digest={row['digest']} match={row['match']}")
sys.exit(0 if len(rows) == 10 and all(x['match'] for x in rows) else 1)
