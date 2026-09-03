using System;
using Shouldly;
using Wathiq.Guides.Guides;
using Xunit;

namespace Wathiq.Guides;

public class VectorMathTests
{
    [Fact]
    public void Identical_Direction_Scores_One_Regardless_Of_Magnitude()
    {
        // Cosine is direction-only: a long chunk and a short question about the same thing match.
        VectorMath.CosineSimilarity([1f, 2f, 3f], [2f, 4f, 6f]).ShouldBe(1.0, tolerance: 1e-9);
    }

    [Fact]
    public void Orthogonal_Scores_Zero_And_Opposite_Scores_Minus_One()
    {
        VectorMath.CosineSimilarity([1f, 0f], [0f, 1f]).ShouldBe(0.0, tolerance: 1e-9);
        VectorMath.CosineSimilarity([1f, 2f], [-1f, -2f]).ShouldBe(-1.0, tolerance: 1e-9);
    }

    [Fact]
    public void Zero_Vector_Is_Similar_To_Nothing()
    {
        // Not NaN: a degenerate embedding must sink below any floor, not poison the ranking.
        VectorMath.CosineSimilarity([0f, 0f], [1f, 2f]).ShouldBe(0.0);
    }

    [Fact]
    public void Mismatched_Dimensions_Throw()
    {
        // Mixed models = mixed spaces - upstream bug, never a quiet low score.
        Should.Throw<ArgumentException>(() => VectorMath.CosineSimilarity([1f], [1f, 2f]));
    }

    [Fact]
    public void Survives_The_Real_Dimensionality()
    {
        var a = new float[1024];
        var b = new float[1024];
        for (var i = 0; i < 1024; i++) { a[i] = MathF.Sin(i); b[i] = MathF.Sin(i + 0.1f); }

        var score = VectorMath.CosineSimilarity(a, b);

        score.ShouldBeGreaterThan(0.9);   // nearly-aligned vectors stay nearly-aligned at scale
        score.ShouldBeLessThan(1.0);
    }
}
