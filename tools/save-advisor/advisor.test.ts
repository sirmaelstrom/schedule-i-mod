import { test } from 'node:test';
import * as assert from 'node:assert/strict';
import * as fs from 'node:fs';
import * as path from 'node:path';
import * as os from 'node:os';
import { spawnSync } from 'node:child_process';

const script=path.join(import.meta.dirname,'advisor.ts');
const lint=path.join(import.meta.dirname,'lint-brief.ts');
function setup() {
  const root=fs.mkdtempSync(path.join(os.tmpdir(),'advisor-test-')), save=path.join(root,'save'), out=path.join(root,'out'); fs.mkdirSync(save);
  const write=(name:string,data:any)=>fs.writeFileSync(path.join(save,name),JSON.stringify(data));
  write('Game.json',{GameVersion:'0.4.7f12'}); write('Time.json',{ElapsedDays:2,TimeOfDay:700});
  write('Money.json',{OnlineBalance:10,Networth:20,LifetimeEarnings:30,WeeklyDepositSum:40});
  write('Products.json',{DiscoveredProducts:['sample'],MixRecipes:[],ProductPrices:[{String:'sample',Int:17}]});
  write('NPCs.json',{NPCs:[{BaseData:JSON.stringify({ID:'customer'}),AdditionalDatas:[{Name:'Relationship',Contents:JSON.stringify({RelationDelta:2,Unlocked:true})},{Name:'CustomerData',Contents:JSON.stringify({Dependence:0.25,ProductAffinities:[0.3],TimeSinceLastDealCompleted:55})}]}]});
  const run=()=>spawnSync(process.execPath,[script,save,out],{encoding:'utf8'});
  const load=()=>JSON.parse(fs.readFileSync(path.join(out,'digest.json'),'utf8'));
  return {root,save,out,write,run,load};
}
function cleanup(root:string) { fs.rmSync(root,{recursive:true,force:true}); }
test('named product price mutation propagates',()=>{const x=setup();try {assert.equal(x.run().status,0);assert.equal(x.load().products.prices[0].price.value,17);x.write('Products.json',{DiscoveredProducts:['sample'],MixRecipes:[],ProductPrices:[{String:'sample',Int:23}]});assert.equal(x.run().status,0);assert.equal(x.load().products.prices[0].price.value,23);} finally {cleanup(x.root)}});
test('missing required file refuses with file and path',()=>{const x=setup();try {fs.unlinkSync(path.join(x.save,'Money.json'));const r=x.run();assert.notEqual(r.status,0);assert.match(r.stderr,/Money\.json \$/);}finally{cleanup(x.root)}});
test('missing required field refuses with file and path',()=>{const x=setup();try{x.write('Money.json',{OnlineBalance:10,Networth:20,WeeklyDepositSum:40});const r=x.run();assert.notEqual(r.status,0);assert.match(r.stderr,/Money\.json \$\.LifetimeEarnings/);}finally{cleanup(x.root)}});
test('malformed nested JSON refuses with file and path',()=>{const x=setup();try{x.write('NPCs.json',{NPCs:[{BaseData:'{broken',AdditionalDatas:[]}]});const r=x.run();assert.notEqual(r.status,0);assert.match(r.stderr,/NPCs\.json \$\.NPCs\[0\]\.BaseData: malformed nested JSON/);}finally{cleanup(x.root)}});
test('different and unknown versions warn in digest header',()=>{const x=setup();try{for(const v of ['0.4.6f5','unknown']){x.write('Game.json',{GameVersion:v});assert.equal(x.run().status,0);assert.match(fs.readFileSync(path.join(x.out,'digest.md'),'utf8'),/WARNING: save version/);}}finally{cleanup(x.root)}});
test('citation lint rejects altered value and fabricated rule',()=>{const x=setup();try{assert.equal(x.run().status,0);const play={action:'Consider',prediction:'Prediction: measure after play',citations:[{pointer:'/day/value',value:2}],rules:['M1']};const brief={plays:[play,play,play]};const f=path.join(x.root,'brief.json');const run=()=>spawnSync(process.execPath,[lint,f,path.join(x.out,'digest.json')],{encoding:'utf8'});fs.writeFileSync(f,JSON.stringify(brief));assert.equal(run().status,0);play.citations[0].value=3;fs.writeFileSync(f,JSON.stringify(brief));let r=run();assert.notEqual(r.status,0);assert.match(r.stderr,/value mismatch/);play.citations[0].value=2;play.rules=['M999'];fs.writeFileSync(f,JSON.stringify(brief));r=run();assert.notEqual(r.status,0);assert.match(r.stderr,/unknown mechanics rule/);}finally{cleanup(x.root)}});
