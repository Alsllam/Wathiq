using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Wathiq.Guides.Evals;

/// <summary>One grounded-Q&amp;A example. expect=refuse cases carry no slug: their ground truth is silence.</summary>
public class RagEvalCase
{
    public string Id { get; set; } = string.Empty;
    public string Lang { get; set; } = string.Empty;
    public string Question { get; set; } = string.Empty;
    /// <summary>"answer" or "refuse".</summary>
    public string Expect { get; set; } = string.Empty;
    public string? MustCiteSlug { get; set; }
}

public static class RagEvalSet
{
    public static IReadOnlyList<RagEvalCase> Load()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Guides", "Evals", "rag-cases.json");
        using var doc = JsonDocument.Parse(File.ReadAllText(path));
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        return doc.RootElement.GetProperty("cases").Deserialize<List<RagEvalCase>>(options)!;
    }
}
