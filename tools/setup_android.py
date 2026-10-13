#!/usr/bin/env python3
"""Install official Android SDK components needed by the stock Godot exporter."""
import argparse
import os
from pathlib import Path
import shutil
import subprocess
import zipfile
from setup_godot import download

def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('--directory', default='.toolchain/android-sdk')
    args = parser.parse_args()
    root = Path(args.directory).resolve(); root.mkdir(parents=True, exist_ok=True)
    archive = root.parent / 'commandlinetools-linux-13114758_latest.zip'
    download('https://dl.google.com/android/repository/commandlinetools-linux-13114758_latest.zip', archive)
    tools = root / 'cmdline-tools/latest'
    if not (tools / 'bin/sdkmanager').exists():
        with zipfile.ZipFile(archive) as bundle:
            for entry in bundle.infolist():
                if entry.is_dir():
                    continue
                relative = Path(entry.filename).relative_to('cmdline-tools')
                if relative.is_absolute() or '..' in relative.parts:
                    raise RuntimeError('Unsafe Android SDK archive path')
                target = tools / relative; target.parent.mkdir(parents=True, exist_ok=True)
                with bundle.open(entry) as source, target.open('wb') as output:
                    shutil.copyfileobj(source, output)
                mode = entry.external_attr >> 16
                if mode:
                    target.chmod(mode)
        for binary in (tools / 'bin').iterdir():
            binary.chmod(0o755)
    manager = str(tools / 'bin/sdkmanager')
    result = subprocess.run([manager, '--sdk_root=' + str(root), 'platform-tools', 'build-tools;35.0.1', 'platforms;android-35'], input='y\n' * 100, text=True, stdout=subprocess.PIPE, stderr=subprocess.STDOUT)
    (root / 'setup.log').write_text(result.stdout)
    if result.returncode:
        print(result.stdout[-4000:])
        raise RuntimeError('Android SDK installation failed')
    print('ANDROID_HOME:', root)

if __name__ == '__main__':
    main()
