// Adapt the shared SaveArchive's SHA256 type only in this Godot compilation.
// Unity keeps its original System.Security.Cryptography implementation intact.
global using SHA256 = RektorSaveHash;

using Godot;
using System;
using System.Text;

/// <summary>Portable SHA-256 through Godot, without the .NET Android crypto/JNI bridge.</summary>
public sealed class RektorSaveHash : IDisposable
{
    public static RektorSaveHash Create() => new();
    public byte[] ComputeHash(byte[] data)
    {
        using var context = new HashingContext();
        if (context.Start(HashingContext.HashType.Sha256) != Error.Ok || (data.Length > 0 && context.Update(data) != Error.Ok))
            throw new InvalidOperationException("Could not calculate save checksum");
        return context.Finish();
    }
    public static string Hex(string text)
    {
        using var hash = Create();
        return Convert.ToHexString(hash.ComputeHash(Encoding.UTF8.GetBytes(text))).ToLowerInvariant();
    }
    public void Dispose() { }
}
