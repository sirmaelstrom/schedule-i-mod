import * as fs from 'node:fs';
import * as path from 'node:path';

// Brief format: each play is a JSON object with title, action, prediction,
// citations [{pointer,value}], and rules ["M1"]. JSON pointers address digest.json.
try {
  const [briefPath,digestPath] = process.argv.slice(2);
  if (!briefPath || !digestPath) throw new Error('Usage: node lint-brief.ts brief.json digest.json');
  const brief=JSON.parse(fs.readFileSync(briefPath,'utf8')), digest=JSON.parse(fs.readFileSync(digestPath,'utf8'));
  const mechanics=fs.readFileSync(path.join(import.meta.dirname,'mechanics.md'),'utf8');
  const rules=new Set([...mechanics.matchAll(/^## (M\d+):/gm)].map(x=>x[1]));
  if (!Array.isArray(brief.plays) || brief.plays.length < 3 || brief.plays.length > 5) throw new Error('brief $.plays: expected 3-5 plays');
  for (const [i,play] of brief.plays.entries()) {
    if (typeof play.action !== 'string' || typeof play.prediction !== 'string' || !play.prediction.startsWith('Prediction:')) throw new Error(`brief $.plays[${i}]: action and labeled prediction required`);
    if (!Array.isArray(play.citations) || !play.citations.length || !Array.isArray(play.rules) || !play.rules.length) throw new Error(`brief $.plays[${i}]: citations and rules required`);
    for (const c of play.citations) {
      if (typeof c.pointer !== 'string' || !c.pointer.startsWith('/')) throw new Error(`brief $.plays[${i}]: invalid pointer`);
      if (c.pointer.startsWith('/customers/') || c.pointer.startsWith('/products/prices/')) throw new Error(`brief $.plays[${i}]: use stable ID-keyed citation ${c.pointer}`);
      let v:any=digest;
      for (const part of c.pointer.slice(1).split('/').map((x:string)=>x.replace(/~1/g,'/').replace(/~0/g,'~'))) {
        if (v === null || typeof v !== 'object' || !Object.hasOwn(v,part)) throw new Error(`brief $.plays[${i}]: missing digest field ${c.pointer}`);
        v=v[part];
      }
      if (v === 'unknown') throw new Error(`brief $.plays[${i}]: cannot cite unknown ${c.pointer}`);
      if (JSON.stringify(v) !== JSON.stringify(c.value)) throw new Error(`brief $.plays[${i}]: value mismatch ${c.pointer}`);
    }
    for (const rule of play.rules) if (!rules.has(rule)) throw new Error(`brief $.plays[${i}]: unknown mechanics rule ${rule}`);
  }
  console.log('PASS: brief citations and rule IDs match');
} catch(e) { console.error(e instanceof Error ? e.message : String(e)); process.exitCode=1; }
