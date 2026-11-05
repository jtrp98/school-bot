import test from 'node:test';
import assert from 'node:assert/strict';
import { createServer } from 'vite';
import Blockly from 'blockly/core';
import { fileURLToPath } from 'node:url';
globalThis.location = { search: '?preview=1&simulator=1' };
const vite = await createServer({root:fileURLToPath(new URL("../",import.meta.url)),configFile:false,server:{middlewareMode:true,hmr:false},appType:'custom'});
let RobotSimulator, ZideTransport, poseGroup, nominalProfile, runPreview, compile;
try {
  ({RobotSimulator}=await vite.ssrLoadModule('/src/core/robotSimulator.ts'));
  ({ZideTransport,poseGroup,nominalProfile}=await vite.ssrLoadModule('/src/modes/robot-control/engineAdapter.ts'));
  ({compile}=await vite.ssrLoadModule('/src/modes/block-programming/blockCompiler.ts'));
  ({runPreview}=await vite.ssrLoadModule('/src/core/preview.ts'));
} finally { await vite.close(); }
const setup=()=>{const board=new RobotSimulator();const failures=[];return {board,failures,driver:new ZideTransport(board,false,e=>failures.push(e))};};
for(const transport of ['hid','serial']) {
  test(`${transport}: discovery/probe/pose/read/upload/run/stop through production adapter`,async()=>{
    const {board,driver}=setup();
    const device=(await driver.discover()).find(d=>d.transport===transport);
    await driver.probe(device);
    assert.deepEqual(board.history,['board.info']);
    const group=poseGroup(nominalProfile(),[0,0,0,0,0,50],[100,100,100,100,100,100]);
    await driver.command(device,'group.execute',{group});
    const read=await driver.command(device,'servo.read',{id:0});
    assert.equal(read.simulated,true);
    assert.equal(read.delivery,'simulator');
    assert.equal(read.exchange.received,'#0PRAD=1500!');
    const groups=Array.from({length:5},(_,index)=>({...group,index}));
    await driver.command(device,'group.download',{groups:groups.slice(0,4)});
    await driver.command(device,'group.download',{groups:groups.slice(4)});
    await driver.command(device,'group.run',{start:0,end:4,times:2});
    await driver.stop(device);
    assert.equal(board.history.at(-1),'robot.stop');
  });
}
test('invalid later pose leaves board unchanged; batches above four and missing groups fail',async()=>{
 const {board,driver}=setup();const [d]=await driver.discover();
 const g=poseGroup(nominalProfile(),[0,0,0,0,0,50],[1,1,1,1,1,1]);
 const bad=structuredClone(g);bad.index=1;bad.items[5].pwm=2501;
 await assert.rejects(driver.command(d,'group.download',{groups:[g,bad]}));
 await assert.rejects(driver.command(d,'group.run',{start:0,end:0,times:1}));
 await assert.rejects(driver.command(d,'group.download',{groups:Array.from({length:5},(_,index)=>({...g,index}))}));
 assert.equal(board.history.includes('group.download'),false);
});
test('emergency cutoff cancels in-flight motion and rejects further motion until explicit rearm',async()=>{
 const {board,driver}=setup();const [d]=await driver.discover();
 const pending=driver.command(d,'servo.set',{id:0,pwm:2000});
 const rejected=assert.rejects(pending,e=>e.code==='ENGINE_CANCELLED');
 await board.emergencyStop();await rejected;
 await driver.stop(d);
 await assert.rejects(driver.command(d,'servo.set',{id:0,pwm:2000}),e=>e.code==='TORQUE_OFF');
 const read=await driver.command(d,'servo.read',{id:0});assert.equal(read.exchange.received,'#0PRAD=1500!');
 board.rearm();await driver.command(d,'servo.set',{id:0,pwm:2000});
});
for(const fault of ['timeout','disconnect'])test(`${fault}: failsafe cuts torque without retrying`,async()=>{
 const {board,driver,failures}=setup();const [d]=await driver.discover();board.fault=fault;
 await assert.rejects(driver.command(d,'servo.set',{id:0,pwm:2000}));
 assert.equal(failures.length,1);assert.equal(board.history.includes('servo.set'),false);
 board.fault='none';await assert.rejects(driver.command(d,'servo.set',{id:0,pwm:2000}),e=>e.code==='TORQUE_OFF');
});
test('DSL and block trace enforce bytes, steps, operations and command count without evaluating code',()=>{
 const run=(source)=>runPreview('coding-adventure','program.run',{source});
 assert.equal(run('hero.move_right(2)\nhero.jump()').data.steps,3);
 for(const source of ['import os','while True: pass','hero.jump(1)','#'+'ก'.repeat(6000),'hero.jump()\n'.repeat(65),'hero.move_right(10)\n'.repeat(11)])assert.throws(()=>run(source));
 assert.throws(()=>runPreview('coding-adventure','shell.exec',{source:'hero.jump()'}));
 assert.throws(()=>runPreview('block-programming','program.run',{commands:[{op:'exec',value:1}]}));
 assert.equal(runPreview('block-programming','program.run',{commands:[{op:'move_right',value:2},{op:'jump'}]}).data.steps,3);
});

Blockly.defineBlocksWithJsonArray([
 {type:'arm_jump',message0:'jump',previousStatement:null,nextStatement:null},
 {type:'arm_repeat',message0:'repeat %1 %2',args0:[{type:'field_number',name:'TIMES',value:2},{type:'input_statement',name:'DO'}],previousStatement:null,nextStatement:null}
]);
test('Blockly repeat trace survives workspace roundtrip; multiple roots, deep nests and expanded overflow fail',()=>{
 const w=new Blockly.Workspace(); const copy=new Blockly.Workspace();
 try {
  const repeat=w.newBlock('arm_repeat');const jump=w.newBlock('arm_jump');repeat.getInput('DO').connection.connect(jump.previousConnection);
  assert.deepEqual(compile(w),[{op:'jump'},{op:'jump'}]);
  Blockly.serialization.workspaces.load(Blockly.serialization.workspaces.save(w),copy);
  assert.deepEqual(compile(copy),compile(w));
  const extra=w.newBlock('arm_jump');assert.throws(()=>compile(w),/one sequence/);extra.dispose();
  repeat.setFieldValue(100,'TIMES');assert.throws(()=>compile(w),/1–10/);repeat.setFieldValue(10,'TIMES');
  let parent=repeat;
  jump.dispose();
  for(let i=0;i<9;i++){const child=w.newBlock('arm_repeat');parent.getInput('DO').connection.connect(child.previousConnection);parent=child;}
  parent.getInput('DO').connection.connect(w.newBlock('arm_jump').previousConnection);
  assert.throws(()=>compile(w),/8 nested/);
 } finally {w.dispose();copy.dispose();}
});
test('cable loss between submission and completion never commits the command',async()=>{
 const {board,driver}=setup();const [d]=await driver.discover();
 const pending=driver.command(d,'servo.set',{id:0,pwm:2200});board.fault='disconnect';
 await assert.rejects(pending,e=>e.code==='DEVICE_NOT_FOUND');
 assert.equal(board.history.includes('servo.set'),false);
});
