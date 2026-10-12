#!/usr/bin/env python3
"""Roslyn fallback for hosts where the dotnet CLI cannot inspect its process. CI uses the csproj."""
from pathlib import Path
import json,subprocess
root=Path(__file__).resolve().parents[1]; sdk=root/'.toolchain/dotnet'; out=root/'.toolchain/tests';out.mkdir(parents=True,exist_ok=True)
refs=list((sdk/'packs/Microsoft.NETCore.App.Ref/8.0.25/ref/net8.0').glob('*.dll'))+[root/'.toolchain/nunit/lib/netstandard2.0/nunit.framework.dll']
sources=list((root/'Assets/SchoolTycoon/Core').glob('*.cs'))+list((root/'Assets/SchoolTycoon/Tests/EditMode').glob('*.cs'))+[root/'tools/HeadlessTestRunner.cs']
rsp=out/'tests.rsp';rsp.write_text('\n'.join(['-nologo','-nostdlib+','-target:exe',f'-out:{out}/Tests.dll']+[f'-r:{x}' for x in refs]+[str(x) for x in sources]))
subprocess.run([str(sdk/'dotnet'),str(sdk/'sdk/8.0.419/Roslyn/bincore/csc.dll'),'@'+str(rsp)],check=True)
(out/'Tests.runtimeconfig.json').write_text(json.dumps({'runtimeOptions':{'tfm':'net8.0','framework':{'name':'Microsoft.NETCore.App','version':'8.0.25'}}}))
(out/'nunit.framework.dll').write_bytes(refs[-1].read_bytes())
subprocess.run([str(sdk/'dotnet'),str(out/'Tests.dll')],check=True)
