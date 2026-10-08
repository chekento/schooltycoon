#!/usr/bin/env python3
"""Structural validation of a source-only Unity project. Does not claim to run Unity."""
import json
import re
from pathlib import Path

root = Path(__file__).resolve().parents[1]
assets = root / 'Assets'
required = ['Assets/Scenes/Campus.unity', 'Packages/manifest.json',
            'ProjectSettings/ProjectVersion.txt', 'ProjectSettings/EditorBuildSettings.asset',
            'ProjectSettings/ProjectSettings.asset', 'ProjectSettings/InputManager.asset']
for name in required:
    assert (root / name).is_file(), f'Missing required file: {name}'
guids = {}
for path in assets.rglob('*'):
    if path.suffix == '.meta':
        m = re.search(r'^guid: ([0-9a-f]{32})$', path.read_text(), re.M)
        assert m, f'Invalid GUID: {path}'
        assert m[1] not in guids, f'Duplicate GUID: {path}'
        guids[m[1]] = path
    else:
        assert Path(str(path) + '.meta').is_file(), f'Missing meta: {path}'
for path in list(assets.rglob('*.asmdef')) + [root / 'Packages/manifest.json']:
    json.loads(path.read_text())
scene_guid = re.search(r'^guid: (\w+)$', (root / 'Assets/Scenes/Campus.unity.meta').read_text(), re.M)[1]
assert scene_guid in (root / 'ProjectSettings/EditorBuildSettings.asset').read_text()
assert 'activeInputHandler: 0' in (root / 'ProjectSettings/ProjectSettings.asset').read_text()
inputs = (root / 'ProjectSettings/InputManager.asset').read_text()
for axis in ['Horizontal', 'Vertical', 'Mouse X', 'Mouse Y', 'Submit', 'Cancel']:
    assert f'm_Name: {axis}' in inputs
assert (root / 'Assets/SchoolTycoon/Resources/CampusSurface.shader').is_file()
print(f'PASS project structure: {len(guids)} unique asset GUIDs, scene, dependencies, input axes and runtime shader.')
