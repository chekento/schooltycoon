#!/usr/bin/env python3
"""Check a produced APK. Does not claim device installation, frame rate or gameplay verification."""
import sys,zipfile,hashlib
from pathlib import Path
apk=Path(sys.argv[1]);assert apk.is_file(),'Unity did not produce the requested APK'
with zipfile.ZipFile(apk) as z:
    assert z.testzip() is None,'APK has corrupt ZIP entries'
    names=set(z.namelist())
    for required in ['AndroidManifest.xml','classes.dex','lib/arm64-v8a/libunity.so','lib/arm64-v8a/libil2cpp.so']:
        assert required in names,f'Missing native Android player part: {required}'
    assert any(n.startswith('assets/bin/Data/') for n in names),'No Unity player data'
print('PASS Unity ARM64 APK structure',apk.stat().st_size,'bytes; SHA-256',hashlib.sha256(apk.read_bytes()).hexdigest())
