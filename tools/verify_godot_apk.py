#!/usr/bin/env python3
"""Validate a real exported Godot player, not a renamed ZIP or an empty launcher."""
import hashlib
from pathlib import Path
import sys
import zipfile

apk = Path(sys.argv[1])
if not apk.is_file():
    raise SystemExit('No APK was produced')
with zipfile.ZipFile(apk) as bundle:
    assert bundle.testzip() is None, 'Corrupt APK archive'
    names = bundle.namelist()
    for item in ['AndroidManifest.xml', 'classes.dex', 'lib/arm64-v8a/libgodot_android.so']:
        assert item in names, 'Missing player component: ' + item
    assert any('Rektor.dll' in name for name in names), 'Missing compiled game assembly'
    assert any('Main.tscn' in name for name in names), 'Missing native main scene'
print('PASS Godot ARM64 APK:', apk.stat().st_size, 'bytes')
print('SHA-256:', hashlib.sha256(apk.read_bytes()).hexdigest())
