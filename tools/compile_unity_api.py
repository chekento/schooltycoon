#!/usr/bin/env python3
"""Compile against official installed Unity/UGUI assemblies, without claiming editor or player execution."""
from pathlib import Path
import subprocess,os
root=Path(__file__).resolve().parents[1]; sdk=root/'.toolchain/dotnet';unity=Path(os.environ.get('UNITY_EDITOR_DATA',str(root/'.toolchain/unity/Editor/Data')));out=root/'.toolchain/unity-api';out.mkdir(exist_ok=True)
refs=list((unity/'MonoBleedingEdge/lib/mono/4.8-api').glob('*.dll'))+list((unity/'MonoBleedingEdge/lib/mono/4.8-api/Facades').glob('*.dll'))+list((unity/'Managed/UnityEngine').glob('*.dll'))

ui=list(unity.rglob('UnityEngine.UI.dll'))
def compile(name,sources,extra=(),defines=''):
 args=['-nologo','-nostdlib+','-unsafe','-target:library',f'-out:{out}/{name}.dll']+[f'-r:{p}' for p in refs+list(extra)]+(['-define:'+defines] if defines else [])+[str(p) for p in sources]
 rsp=out/(name+'.rsp');rsp.write_text('\n'.join(args));subprocess.run([str(sdk/'dotnet'),str(sdk/'sdk/8.0.419/Roslyn/bincore/csc.dll'),'@'+str(rsp)],check=True);return out/(name+'.dll')
if ui:ui=ui[0]
else:
 sources=list((root/'.toolchain/ugui/package/Runtime/UGUI').rglob('*.cs'));assert sources,'UGUI 2.0 package required';ui=compile('UnityEngine.UI',sources,defines='UNITY_6000_0_OR_NEWER,UNITY_2022_3_OR_NEWER,UNITY_2021_2_OR_NEWER,UNITY_2020_1_OR_NEWER,UNITY_2019_1_OR_NEWER')
core=compile('KoSch.SchoolTycoon.Core',list((root/'Assets/SchoolTycoon/Core').glob('*.cs')))
runtime=compile('KoSch.SchoolTycoon.Runtime',list((root/'Assets/SchoolTycoon/Runtime').glob('*.cs')),[core,ui])
compile('KoSch.SchoolTycoon.Editor',list((root/'Assets/SchoolTycoon/Editor').glob('*.cs')),[core,runtime,ui],defines='UNITY_EDITOR')
print('PASS Core, Runtime/UGUI and Editor compile against official Unity assemblies. Rendering not executed.')
