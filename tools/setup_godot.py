#!/usr/bin/env python3
"""Install pinned official Godot .NET tools without any account activation."""
import argparse
import hashlib
import os
from pathlib import Path
import tarfile
import urllib.request
import zipfile

VERSION = '4.7.2'
SDK_VERSIONS = ('8.0.419', '10.0.403')
# Official asset digests from godotengine/godot-builds 4.7.2-stable.
SHA256 = {
    'mono_linux_x86_64.zip': '129f82db7bafd54ae14bb5bb284041c73860e8c7a009a3a026ca5e946cbff247',
    'mono_export_templates.tpz': '92f8681e349ef1f90891b792da95e3b2b0bd1ed610b78018c58feb2d87e15a9d',
}

def download(url, destination, expected=None):
    if destination.exists() and (not expected or hashlib.sha256(destination.read_bytes()).hexdigest() == expected):
        return
    request = urllib.request.Request(url, headers={'User-Agent': 'Rektor-build/0.7'})
    temp = destination.with_suffix(destination.suffix + '.partial')
    with urllib.request.urlopen(request, timeout=60) as response, temp.open('wb') as stream:
        size = 0
        while chunk := response.read(8 * 1024 * 1024):
            stream.write(chunk)
            size += len(chunk)
            if size % (128 * 1024 * 1024) < len(chunk):
                print(f'{destination.name}: {size // (1024 * 1024)} MiB', flush=True)
    if expected and hashlib.sha256(temp.read_bytes()).hexdigest() != expected:
        raise RuntimeError('Official release checksum mismatch: ' + destination.name)
    temp.replace(destination)

def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('--directory', default='.toolchain/godot')
    parser.add_argument('--templates', action='store_true')
    parser.add_argument('--dotnet', action='store_true')
    args = parser.parse_args()
    root = Path(args.directory).resolve()
    root.mkdir(parents=True, exist_ok=True)
    names = [f'Godot_v{VERSION}-stable_mono_linux_x86_64.zip']
    if args.templates:
        names.append(f'Godot_v{VERSION}-stable_mono_export_templates.tpz')
    for name in names:
        suffix = name.split('-stable_', 1)[1]
        archive = root / name
        download(f'https://github.com/godotengine/godot-builds/releases/download/{VERSION}-stable/{name}', archive, SHA256[suffix])
        with zipfile.ZipFile(archive) as bundle:
            if name.endswith('.tpz'):
                target = Path(os.environ.get('XDG_DATA_HOME', str(Path.home() / '.local/share'))) / 'godot/export_templates' / f'{VERSION}.stable.mono'
                target.mkdir(parents=True, exist_ok=True)
                for entry in bundle.infolist():
                    if entry.is_dir():
                        continue
                    relative = Path(entry.filename).relative_to('templates')
                    if relative.is_absolute() or '..' in relative.parts:
                        raise RuntimeError('Unsafe template archive path')
                    destination = target / relative
                    destination.parent.mkdir(parents=True, exist_ok=True)
                    with bundle.open(entry) as source, destination.open('wb') as output:
                        import shutil
                        shutil.copyfileobj(source, output)
                print('Templates:', target, flush=True)
            else:
                bundle.extractall(root)
                for entry in bundle.infolist():
                    path = root / entry.filename
                    if path.is_file() and entry.external_attr >> 16:
                        path.chmod(entry.external_attr >> 16)
    engine = next(p for p in root.glob(f'Godot_v{VERSION}-stable_mono_linux_x86_64/Godot*') if p.is_file() and p.name.endswith('.x86_64'))
    engine.chmod(0o755)
    if args.dotnet:
        sdk_root = root.parent / 'dotnet'
        sdk_root.mkdir(parents=True, exist_ok=True)
        for version in SDK_VERSIONS:
            archive = root / f'dotnet-sdk-{version}.tar.gz'
            download(f'https://dotnetcli.blob.core.windows.net/dotnet/Sdk/{version}/dotnet-sdk-{version}-linux-x64.tar.gz', archive)
            with tarfile.open(archive) as bundle:
                bundle.extractall(sdk_root, filter='data')
        print('DOTNET_ROOT:', sdk_root, flush=True)
    print('GODOT:', engine, flush=True)

if __name__ == '__main__':
    main()
