using System;
using System.Linq;
using Shouldly;
using Wathiq.Guides.Guides;
using Xunit;

namespace Wathiq.Guides;

public class EmbeddingConverterTests
{
    [Fact]
    public void Round_Trip_Preserves_Every_Float_Exactly()
    {
        var vector = Enumerable.Range(0, 1024).Select(i => (float)Math.Sin(i) * i).ToArray();

        var bytes = EmbeddingConverter.ToBytes(vector);

        bytes.Length.ShouldBe(GuideConsts.EmbeddingByteLength);   // 1024 × 4 = the column width
        EmbeddingConverter.ToFloats(bytes).ShouldBe(vector);      // bit-exact, not approximately
    }

    [Fact]
    public void Layout_Is_Little_Endian_By_Contract()
    {
        // 1.0f = 0x3F800000 → little-endian bytes 00 00 80 3F. If this test fails on some
        // machine, the STORED data would have been unportable - that is the bug it guards.
        EmbeddingConverter.ToBytes(new[] { 1.0f }).ShouldBe([0x00, 0x00, 0x80, 0x3F]);
    }

    [Fact]
    public void Torn_Blob_Is_Rejected()
    {
        Should.Throw<ArgumentException>(() => EmbeddingConverter.ToFloats(new byte[6]));
    }
}
