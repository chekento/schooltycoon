#!/usr/bin/env python3
"""Prepare deterministic text tree chunks for an authorized GitHub checkpoint."""
from pathlib import Path
import subprocess,json,hashlib
root=Path(__file__).resolve().parents[1]
for p in (root/'Assets').rglob('*'):
 m=Path(str(p)+'.meta')
 if p.suffix=='.meta' or m.exists():continue
 guid=hashlib.md5(('kosch-school-v0.2/'+str(p.relative_to(root))).encode()).hexdigest()
 s='fileFormatVersion: 2\nguid: '+guid+'\n'
 if p.is_dir():s+='folderAsset: yes\nDefaultImporter:\n  externalObjects: {}\n'
 elif p.suffix=='.shader':s+='ShaderImporter:\n  externalObjects: {}\n  defaultTextures: []\n  nonModifiableTextures: []\n'
 elif p.suffix=='.png':s+='TextureImporter:\n  externalObjects: {}\n  serializedVersion: 13\n  mipmaps:\n    enableMipMap: 0\n  isReadable: 0\n  textureType: 0\n  maxTextureSize: 2048\n  textureCompression: 1\n  alphaIsTransparency: 0\n  nPOTScale: 0\n'
 else:s+='MonoImporter:\n  externalObjects: {}\n  serializedVersion: 2\n  defaultReferences: []\n  executionOrder: 0\n  icon: {instanceID: 0}\n'
 m.write_text(s)
paths=set(subprocess.check_output(['git','diff','--name-only'],cwd=root,text=True).splitlines()+subprocess.check_output(['git','ls-files','--others','--exclude-standard'],cwd=root,text=True).splitlines())
entries=[]
for p in sorted(paths):
 f=root/p
 if not f.is_file():entries.append(dict(path=p,mode='100644',type='blob',sha=None));continue
 if f.suffix in ['.png','.jpg','.jpeg','.webp','.zip','.apk','.blend','.pyc','.keystore','.jks']:continue
 entries.append(dict(path=p,mode='100644',type='blob',content=f.read_text()))
chunks=[];chunk=[];size=0
for e in entries:
 n=len(json.dumps(e).encode())
 if size+n>45000 and chunk:chunks.append(chunk);chunk=[];size=0
 chunk.append(e);size+=n
if chunk:chunks.append(chunk)
folder=root/'.toolchain/checkpoint';folder.mkdir(parents=True,exist_ok=True)
for i,c in enumerate(chunks):(folder/f'tree-{i}.json').write_text(json.dumps(c))
print(json.dumps({'files':len(entries),'chunks':len(chunks)}))
