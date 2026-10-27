#!/usr/bin/env python3
"""Keep the single Godot engine DSO mapped for the lifetime of the app process.

Set the standard ELF DF_1_NODELETE flag, equivalent to linking with -z nodelete.
No instructions, permissions, exported symbols or game resources are changed.
The resulting APK must be aligned and signed again before installation.
"""
from pathlib import Path
import struct
import sys
import zipfile


def patch_library(data):
    elf = bytearray(data)
    if elf[:6] != b'\x7fELF\x02\x01':
        raise ValueError('Expected little-endian ELF64 Godot library')
    phoff = struct.unpack_from('<Q', elf, 32)[0]
    entry_size, count = struct.unpack_from('<HH', elf, 54)
    changed = []
    for index in range(count):
        at = phoff + index * entry_size
        kind, _, offset, _, _, size, _, _ = struct.unpack_from('<IIQQQQQQ', elf, at)
        if kind != 2:
            continue
        for at in range(offset, offset + size, 16):
            tag, value = struct.unpack_from('<QQ', elf, at)
            if tag == 0:
                break
            if tag == 0x6ffffffb:
                struct.pack_into('<Q', elf, at + 8, value | 8)
                changed.append((value, value | 8))
    if len(changed) != 1:
        raise ValueError('Expected one existing DT_FLAGS_1 entry')
    return bytes(elf), changed[0]


def main():
    source, output = map(Path, sys.argv[1:3])
    if source.resolve() == output.resolve():
        raise ValueError('Use a separate output APK')
    with zipfile.ZipFile(source) as src, zipfile.ZipFile(output, 'w') as dst:
        libraries = []
        for item in src.infolist():
            data = src.read(item.filename)
            if item.filename.startswith('lib/') and item.filename.endswith('/libgodot_android.so'):
                data, values = patch_library(data)
                libraries.append(item.filename)
                print('Godot native lifetime:', item.filename, hex(values[0]), '->', hex(values[1]))
            dst.writestr(item, data)
        if set(libraries) != {'lib/arm64-v8a/libgodot_android.so', 'lib/x86_64/libgodot_android.so'}:
            raise ValueError('Expected exactly the ARM64 and x86-64 Godot engines')


if __name__ == '__main__':
    main()
