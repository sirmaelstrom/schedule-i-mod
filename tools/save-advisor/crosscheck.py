"""Independent first-slice check. python -I -X utf8 crosscheck.py SAVE DIGEST OUTPUT.

Named checks: elapsed days, online balance, lifetime earnings, OG Kush price,
Sour Diesel price, relationship, dependence and time since last completed deal
of the first customer with nonzero dependence, another customer's dependence,
and the motel's placed-object count. These are raw values or direct counts, not rates.
The day-2 motel has no employees and there are no active deliveries, so object
count and a second customer's dependence replace those empty examples.
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
second = next(x for x in n['NPCs'] if json.loads(x['BaseData'])['ID'] != customer_id and any(a['Name'] == 'CustomerData' and json.loads(a['Contents'])['Dependence'] != 0 for a in x['AdditionalDatas']))
second_id = json.loads(second['BaseData'])['ID']
second_raw = json.loads(next(a['Contents'] for a in second['AdditionalDatas'] if a['Name'] == 'CustomerData'))['Dependence']
second_digest = next(x for x in d['customers'] if x['id']['value'] == second_id)['dependence']['value']
property_raw = read('Properties/Motel Room.json')
property_digest = next(x for x in d['properties'] if x['code']['value'] == property_raw['PropertyCode'])
checks = [
    ('elapsed days', t['ElapsedDays'], d['day']['value']),
    ('online balance', m['OnlineBalance'], d['economy']['OnlineBalance']['value']),
    ('lifetime earnings', m['LifetimeEarnings'], d['economy']['LifetimeEarnings']['value']),
    ('OG Kush price', prices['ogkush'], digest_prices['ogkush']),
    ('Sour Diesel price', prices['sourdiesel'], digest_prices['sourdiesel']),
    ('selected customer relationship', parts['Relationship']['RelationDelta'], customer['relationship']['value']),
    ('selected customer dependence', parts['CustomerData']['Dependence'], customer['dependence']['value']),
    ('selected customer time since last deal', parts['CustomerData']['TimeSinceLastDealCompleted'], customer['lastDeal']['value']),
    ('second customer dependence', second_raw, second_digest),
    ('motel placed-object count', len(property_raw['Objects']), len(property_digest['objects'])),
]
rows = [{'name': name, 'raw': raw, 'digest': actual, 'match': raw == actual} for name, raw, actual in checks]
output.parent.mkdir(parents=True, exist_ok=True)
output.write_text(json.dumps(rows, indent=2) + '\n', encoding='utf-8')
for row in rows:
    print(f"{row['name']}: raw={row['raw']} digest={row['digest']} match={row['match']}")
sys.exit(0 if len(rows) == 10 and all(x['match'] for x in rows) else 1)
