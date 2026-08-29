using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Wathiq.Shared.Extraction;

namespace Wathiq.Ai.Evals;

/// <summary>One labeled example: OCR text in, the proposal a correct pipeline should emit out.</summary>
public class EvalCase
{
    public string Id { get; set; } = string.Empty;
    public string Lang { get; set; } = string.Empty;
    public string OcrText { get; set; } = string.Empty;
    public EvalExpected Expected { get; set; } = new();
}

public class EvalExpected
{
    public string? Number { get; set; }
    public DateOnly? IssueDate { get; set; }
    public DateOnly? ExpiryDate { get; set; }
}

public static class EvalSet
{
    /// <summary>Loaded from the copied content file - editing cases never touches C#.</summary>
    public static IReadOnlyList<EvalCase> Load()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Ai", "Evals", "cases.json");
        using var doc = JsonDocument.Parse(File.ReadAllText(path));
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        return doc.RootElement.GetProperty("cases").Deserialize<List<EvalCase>>(options)!;
    }
}

/// <summary>
/// Field accuracy, the 3.8 metric: each case contributes three graded fields (number, issue,
/// expiry) and a null ground truth must be MET with null - proposing a value where the truth is
/// "nothing readable" is a miss, so hallucinations cost exactly what they should.
/// </summary>
public static class EvalScorer
{
    public static (int Matched, int Total) Score(EvalCase @case, DocumentDataProposal proposal)
    {
        var matched = 0;
        if (proposal.Number == @case.Expected.Number) matched++;
        if (proposal.IssueDate == @case.Expected.IssueDate) matched++;
        if (proposal.ExpiryDate == @case.Expected.ExpiryDate) matched++;
        return (matched, 3);
    }
}
