import * as fs from 'node:fs';
import * as path from 'node:path';
import * as os from 'node:os';

type Json = any;
type Field = { value: Json; source: string; known: boolean };
const field = (value: Json, source: string): Field => ({ value, source, known:true });
const unknown = (source: string): Field => ({value:'unknown',source,known:false});
function fail(file: string, p: string, why: string): never { throw new Error(`${file} ${p}: ${why}`); }
function read(root: string, file: string): Json {
  let text: string;
  try { text = fs.readFileSync(path.join(root, file), 'utf8'); }
  catch { return fail(file, '$', 'required file missing or unreadable'); }
  try { return JSON.parse(text.replace(/^\uFEFF/, '')); }
  catch { return fail(file, '$', 'malformed JSON'); }
}
function required(o: Json, key: string, type: 'string'|'number'|'boolean'|'array', file: string, p = '$'): Json {
  const v = o?.[key];
  if (type === 'array' ? !Array.isArray(v) : typeof v !== type || (type === 'number' && !Number.isFinite(v)))
    return fail(file, `${p}.${key}`, `required ${type}`);
  return v;
}
function nested(s: Json, file: string, p: string): Json {
  if (typeof s !== 'string') return fail(file, p, 'required JSON string');
  try { return JSON.parse(s); } catch { return fail(file, p, 'malformed nested JSON'); }
}
function optional(o: Json, key: string, source: string): Field {
  return o != null && Object.hasOwn(o, key) && o[key] != null ? field(o[key], `${source}.${key}`) : unknown(`${source}.${key}`);
}
function optionalFile(root: string, name: string): Json | undefined {
  if (!fs.existsSync(path.join(root, name))) return undefined;
  return read(root, name);
}
function extract(root: string, mechanicsPath: string): Json {
  const game = read(root, 'Game.json'), time = read(root, 'Time.json'), money = read(root, 'Money.json');
  const products = read(root, 'Products.json'), npcs = read(root, 'NPCs.json');
  const version = required(game, 'GameVersion', 'string', 'Game.json');
  const mech = fs.readFileSync(mechanicsPath, 'utf8').match(/^Save version: `([^`]+)`/m)?.[1];
  if (!mech) throw new Error('mechanics.md $.Save version: missing version label');
  const economy: Json = {};
  for (const k of ['OnlineBalance','Networth','LifetimeEarnings','WeeklyDepositSum']) economy[k] = field(required(money,k,'number','Money.json'),`Money.json:$.${k}`);
  const invName = 'Players/Player_0/Inventory.json', inv = optionalFile(root,invName);
  if (inv && Array.isArray(inv.Items)) {
    economy.inventory = inv.Items.map((s: Json,i: number) => field(nested(s,invName,`$.Items[${i}]`),`${invName}:$.Items[${i}]::$`));
  } else economy.inventory = unknown(`${invName}:$.Items`);
  const day = field(required(time,'ElapsedDays','number','Time.json'),'Time.json:$.ElapsedDays');
  const timeOfDay = field(required(time,'TimeOfDay','number','Time.json'),'Time.json:$.TimeOfDay');
  const discovered = required(products,'DiscoveredProducts','array','Products.json');
  const recipes = required(products,'MixRecipes','array','Products.json');
  const prices = required(products,'ProductPrices','array','Products.json').map((v: Json, i: number) => {
    const p = `$.ProductPrices[${i}]`;
    return { id: field(required(v,'String','string','Products.json',p),`Products.json:${p}.String`), price: field(required(v,'Int','number','Products.json',p),`Products.json:${p}.Int`) };
  });
  const pricesById: Record<string, Json> = Object.create(null);
  for (const product of prices) {
    const id = product.id.value;
    if (Object.hasOwn(pricesById,id)) fail('Products.json',product.id.source,'duplicate product ID');
    pricesById[id] = product;
  }
  const receiptsById: Record<string, Json> | Field = Array.isArray(products.ContractReceipts) ? Object.create(null) : unknown('Products.json:$.ContractReceipts');
  if (!Object.hasOwn(receiptsById,'known')) products.ContractReceipts.forEach((receipt: Json,i: number) => {
    const p=`Products.json:$.ContractReceipts[${i}]`;
    if (typeof receipt?.ReceiptId !== 'number' || !Number.isFinite(receipt.ReceiptId)) return;
    const id=String(receipt.ReceiptId);
    if (Object.hasOwn(receiptsById,id)) fail('Products.json',`${p}.ReceiptId`,'duplicate receipt ID');
    (receiptsById as Record<string, Json>)[id]={id:field(receipt.ReceiptId,`${p}.ReceiptId`),customer:optional(receipt,'CustomerId',p),amountPaid:optional(receipt,'AmountPaid',p)};
  });
  const customers: Json[] = [];
  required(npcs,'NPCs','array','NPCs.json').forEach((v: Json, i: number) => {
    const p = `$.NPCs[${i}]`, base = nested(v?.BaseData,'NPCs.json',`${p}.BaseData`);
    const id = required(base,'ID','string','NPCs.json',`${p}.BaseData::$`);
    const additional = required(v,'AdditionalDatas','array','NPCs.json',p);
    const ci = additional.findIndex((x: Json) => x?.Name === 'CustomerData');
    if (ci < 0) return;
    const ri = additional.findIndex((x: Json) => x?.Name === 'Relationship');
    const cp = `${p}.AdditionalDatas[${ci}].Contents`, rp = `${p}.AdditionalDatas[${ri}].Contents`;
    const c = nested(additional[ci].Contents,'NPCs.json',cp), rel = ri < 0 ? undefined : nested(additional[ri].Contents,'NPCs.json',rp);
    const cf = (key: string, type: 'number'|'array') => field(required(c,key,type,'NPCs.json',`${cp}::$`),`NPCs.json:${cp}::$.${key}`);
    const rf = (key: string, type: 'number'|'boolean') => ri < 0 ? unknown(`NPCs.json:${p}.AdditionalDatas[Relationship]::$.${key}`) : field(required(rel,key,type,'NPCs.json',`${rp}::$`),`NPCs.json:${rp}::$.${key}`);
    customers.push({ id:field(id,`NPCs.json:${p}.BaseData::$.ID`), relationship:rf('RelationDelta','number'), unlocked:rf('Unlocked','boolean'), dependence:cf('Dependence','number'), affinities:cf('ProductAffinities','array'), lastDeal:cf('TimeSinceLastDealCompleted','number'), sinceInstantOffer:optional(c,'TimeSinceInstantDealOffered',`NPCs.json:${cp}::$`), contractOffered:optional(c,'IsContractOffered',`NPCs.json:${cp}::$`), completedDeals:optional(c,'CompletedDeals',`NPCs.json:${cp}::$`), region:unknown(`NPCs.json:${p}::region (not stored in supported data)`) });
  });
  const propertiesDir = path.join(root,'Properties');
  const properties = fs.existsSync(propertiesDir) ? fs.readdirSync(propertiesDir).filter(x => x.endsWith('.json')).sort().map(file => {
    const name = `Properties/${file}`, v = read(root,name), source = `${name}:$`;
    const objects = Array.isArray(v.Objects) ? v.Objects.map((obj: Json,i: number) => {
      const p = `$.Objects[${i}].BaseData`, sourcePath=`${name}:${p}::$.ItemString::$.ID`;
      let item: Field;
      try {
        const base = nested(obj?.BaseData,name,p);
        const decoded = nested(base?.ItemString,name,`${p}::$.ItemString`);
        item = optional(decoded,'ID',`${name}:${p}::$.ItemString::$`);
      } catch { item = unknown(sourcePath); }
      return { type:optional(obj,'DataType',`${name}:$.Objects[${i}]`), item };
    }) : unknown(`${source}.Objects`);
    const canCount = Array.isArray(objects) && objects.every((o: Json)=>o.item.known && typeof o.item.value === 'string');
    const itemCounts: Record<string, Field> | Field = canCount ? Object.create(null) : unknown(`${source}.Objects[*].BaseData::$.ItemString::$.ID (count)`);
    if (canCount) {
      for (const obj of objects) { const id=obj.item.value; itemCounts[id]=field((itemCounts[id]?.value ?? 0)+1,`${source}.Objects[*].BaseData::$.ItemString::$.ID (count)`); }
    }
    return { code:optional(v,'PropertyCode',source), owned:optional(v,'IsOwned',source), employees:optional(v,'Employees',source), employeeCount:Array.isArray(v.Employees) ? field(v.Employees.length,`${source}.Employees.length`) : unknown(`${source}.Employees`), objects, itemCounts };
  }) : unknown('Properties/:$');
  const delivery = optionalFile(root,'Deliveries.json'), law = optionalFile(root,'Law.json'), cartel = optionalFile(root,'Cartel.json');
  const propertiesByCode: Record<string, Json> = Object.create(null);
  if (Array.isArray(properties)) for (const prop of properties) {
    if (!prop.code.known || typeof prop.code.value !== 'string') continue;
    if (Object.hasOwn(propertiesByCode,prop.code.value)) fail('Properties/',prop.code.source,'duplicate property code');
    propertiesByCode[prop.code.value] = prop;
  }
  const customersById: Record<string, Json> = Object.create(null);
  for (const customer of customers) {
    const id = customer.id.value;
    if (Object.hasOwn(customersById,id)) fail('NPCs.json',customer.id.source,'duplicate customer ID');
    customersById[id] = customer;
  }
  return { version:field(version,'Game.json:$.GameVersion'), mechanicsVersion:mech, warning:version === mech ? null : `WARNING: save version ${version} differs from mechanics version ${mech}; rules may not apply.`, day, timeOfDay, economy, products:{ discovered:field(discovered,'Products.json:$.DiscoveredProducts'), recipes:field(recipes,'Products.json:$.MixRecipes'), prices, pricesById, receiptsById }, customers, customersById, properties, propertiesByCode, deliveries:{ active:optional(delivery,'ActiveDeliveries','Deliveries.json:$') }, law:{ intensity:optional(law,'InternalLawIntensity','Law.json:$') }, cartel:{ status:optional(cartel,'Status','Cartel.json:$'), influence:optional(cartel,'RegionInfluence','Cartel.json:$') } };
}
function main(): void {
  const [input, output] = process.argv.slice(2);
  if (!input || !output) throw new Error('Usage: node advisor.ts <save-directory> <output-directory>');
  const src = fs.realpathSync(input), dst = path.resolve(output);
  const rel = path.relative(src,dst);
  if (!rel || (!rel.startsWith('..'+path.sep) && rel !== '..' && !path.isAbsolute(rel))) throw new Error('output directory must be outside the save');
  const temp = fs.mkdtempSync(path.join(os.tmpdir(),'save-advisor-'));
  try {
    fs.cpSync(src,path.join(temp,'save'),{recursive:true,errorOnExist:true,force:false});
    const digest = extract(path.join(temp,'save'),path.join(import.meta.dirname,'mechanics.md'));
    fs.mkdirSync(dst,{recursive:true});
    fs.writeFileSync(path.join(dst,'digest.json'),JSON.stringify(digest,null,2)+'\n');
    fs.writeFileSync(path.join(dst,'digest.md'),`# Save digest\n\n${digest.warning ? `**${digest.warning}**\n\n` : ''}Save version: ${digest.version.value}. Day: ${digest.day.value}.\n\nValues and their source paths are in digest.json.\n\n\`\`\`json\n${JSON.stringify(digest,null,2)}\n\`\`\`\n`);
  } finally { fs.rmSync(temp,{recursive:true,force:true}); }
}
try { main(); } catch (e) { console.error(e instanceof Error ? e.message : String(e)); process.exitCode = 1; }
