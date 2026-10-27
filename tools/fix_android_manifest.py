#!/usr/bin/env python3
"""Handle system resource updates without recreating the owned Godot activity.

Only configChanges in compiled XML is changed. The output must be zipaligned
and signed again before installation; permissions and launcher stay unchanged.
"""
from pathlib import Path
import re
import struct
import sys
import zipfile

CONFIG_MASK = 0x80000000 | 0x40000000 | 0x10000000 | 0x00004000 | 0x00008000


def patch_manifest(data):
    xml = bytearray(data)
    if struct.unpack_from('<H', xml)[0] != 3:
        raise ValueError('Expected compiled Android XML')
    strings = []
    changed = []
    offset = struct.unpack_from('<H', xml, 2)[0]
    while offset < len(xml):
        kind, header, size = struct.unpack_from('<HHI', xml, offset)
        if size < header or offset + size > len(xml):
            raise ValueError('Invalid XML chunk boundary')
        if kind == 1:
            count, _, flags, start, _ = struct.unpack_from('<IIIII', xml, offset + 8)
            for index in range(count):
                cursor = offset + start + struct.unpack_from('<I', xml, offset + header + index * 4)[0]
                if flags & 0x100:
                    def length8(at):
                        first = xml[at]
                        return (((first & 0x7f) << 8) | xml[at+1], at+2) if first & 0x80 else (first, at+1)
                    _, cursor = length8(cursor)
                    n, cursor = length8(cursor)
                    strings.append(bytes(xml[cursor:cursor+n]).decode('utf-8'))
                else:
                    first = struct.unpack_from('<H', xml, cursor)[0]
                    cursor += 2
                    if first & 0x8000:
                        n = ((first & 0x7fff) << 16) | struct.unpack_from('<H', xml, cursor)[0]
                        cursor += 2
                    else:
                        n = first
                    strings.append(bytes(xml[cursor:cursor+n*2]).decode('utf-16-le'))
        elif kind == 0x102:
            _, name, attr_start, attr_size, attr_count = struct.unpack_from('<IIHHH', xml, offset + 16)
            if strings[name] == 'activity':
                attributes = {}
                for index in range(attr_count):
                    at = offset + 16 + attr_start + index * attr_size
                    _, key, raw, _, _, dtype, value = struct.unpack_from('<IIIHBBI', xml, at)
                    attributes[strings[key]] = (at, raw, dtype, value)
                named = attributes.get('name')
                if named:
                    activity = strings[named[1] if named[1] != 0xffffffff else named[3]]
                    if activity.endswith('.GodotApp'):
                        at, _, dtype, old = attributes['configChanges']
                        if dtype not in (0x10, 0x11):
                            raise ValueError('configChanges must be an integer')
                        new = old | CONFIG_MASK
                        struct.pack_into('<I', xml, at + 8, 0xffffffff)
                        struct.pack_into('<I', xml, at + 16, new)
                        changed.append((activity, old, new))
        offset += size
    if len(changed) != 1:
        raise ValueError('Expected exactly one Godot game activity')
    return bytes(xml), changed[0]


def main():
    source, output = map(Path, sys.argv[1:3])
    if source.resolve() == output.resolve():
        raise ValueError('Use a separate output APK')
    with zipfile.ZipFile(source) as src, zipfile.ZipFile(output, 'w') as dst:
        manifest, changed = patch_manifest(src.read('AndroidManifest.xml'))
        for item in src.infolist():
            if item.filename == 'META-INF/MANIFEST.MF' or re.fullmatch(r'META-INF/[^/]+\.(RSA|DSA|EC|SF)', item.filename, re.I):
                continue
            dst.writestr(item, manifest if item.filename == 'AndroidManifest.xml' else src.read(item.filename))
    print('Android lifecycle configuration:', changed[0], hex(changed[1]), '->', hex(changed[2]))


if __name__ == '__main__':
    main()
