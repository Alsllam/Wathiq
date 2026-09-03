using System;
using System.Buffers.Binary;

namespace Wathiq.Guides.Guides;

/// <summary>
/// float[] ↔ varbinary, explicitly little-endian. BitConverter would silently follow the CPU's
/// endianness - fine until a backup restores onto different hardware; an explicit byte order
/// makes the column a portable contract (and the round-trip test meaningful).
/// </summary>
public static class EmbeddingConverter
{
    public static byte[] ToBytes(ReadOnlySpan<float> vector)
    {
        var bytes = new byte[vector.Length * sizeof(float)];
        for (var i = 0; i < vector.Length; i++)
        {
            BinaryPrimitives.WriteSingleLittleEndian(bytes.AsSpan(i * sizeof(float)), vector[i]);
        }
        return bytes;
    }

    public static float[] ToFloats(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length % sizeof(float) != 0)
        {
            throw new ArgumentException($"Embedding blob length {bytes.Length} is not a multiple of 4.", nameof(bytes));
        }

        var vector = new float[bytes.Length / sizeof(float)];
        for (var i = 0; i < vector.Length; i++)
        {
            vector[i] = BinaryPrimitives.ReadSingleLittleEndian(bytes.Slice(i * sizeof(float)));
        }
        return vector;
    }
}
