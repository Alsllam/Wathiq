using System;
using System.Collections.Generic;
using System.Linq;

namespace Wathiq.Guides.Guides;

/// <summary>A chunk before embedding: what the chunker emits, what the embed job persists.</summary>
public record GuideChunkDraft(int ChunkNo, string Text, int TokenCount);

/// <summary>
/// Pure, deterministic chunking (FR-GDE-003) - the part of RAG that is engineering, not model.
/// Strategy: split the markdown body on headings (a heading is a topic boundary - the best
/// free semantic signal in authored content), pack sections into ~300-token chunks, and carry
/// the previous chunk's tail forward as overlap so a fact straddling a boundary is findable
/// from both sides. Steps and the facts line (documents/fees/location) become their own chunks:
/// they answer the most common questions and must never be diluted mid-chunk.
/// </summary>
public static class GuideChunker
{
    public const int TargetTokensPerChunk = 300;
    public const int OverlapTokens = 40;

    public static IReadOnlyList<GuideChunkDraft> Chunk(
        string bodyMarkdown, IEnumerable<string> steps,
        string? requiredDocuments, string? fees, string? location)
    {
        var texts = new List<string>();

        // 1. The facts chunk: short, dense, and the likeliest retrieval hit for "how much / what
        //    do I need / where" questions.
        var facts = new List<string>();
        if (!string.IsNullOrWhiteSpace(requiredDocuments)) facts.Add(requiredDocuments!.Trim());
        if (!string.IsNullOrWhiteSpace(fees)) facts.Add(fees!.Trim());
        if (!string.IsNullOrWhiteSpace(location)) facts.Add(location!.Trim());
        if (facts.Count > 0)
        {
            texts.Add(string.Join("\n", facts));
        }

        // 2. Body sections, heading-aware then size-packed with overlap.
        texts.AddRange(PackSections(SplitOnHeadings(bodyMarkdown)));

        // 3. Steps as one ordered chunk (they are small; order carries meaning).
        var stepList = steps.Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
        if (stepList.Count > 0)
        {
            texts.Add(string.Join("\n", stepList.Select((s, i) => $"{i + 1}. {s.Trim()}")));
        }

        return texts
            .Select((t, i) => new GuideChunkDraft(i + 1, t, EstimateTokens(t)))
            .ToList();
    }

    /// <summary>
    /// Honest heuristic, not a tokenizer: ~1 token per word for Arabic-heavy text plus subword
    /// splits ≈ max(words, chars/4). Used as a packing budget - being 20% off costs nothing.
    /// </summary>
    public static int EstimateTokens(string text)
    {
        var words = text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length;
        return Math.Max(words, text.Length / 4);
    }

    private static List<string> SplitOnHeadings(string markdown)
    {
        var sections = new List<string>();
        var current = new List<string>();

        foreach (var line in markdown.Replace("\r\n", "\n").Split('\n'))
        {
            if (line.TrimStart().StartsWith('#') && current.Any(l => !string.IsNullOrWhiteSpace(l)))
            {
                sections.Add(string.Join("\n", current).Trim());
                current.Clear();
            }
            current.Add(line);
        }
        if (current.Any(l => !string.IsNullOrWhiteSpace(l)))
        {
            sections.Add(string.Join("\n", current).Trim());
        }
        return sections;
    }

    private static IEnumerable<string> PackSections(List<string> sections)
    {
        var chunks = new List<string>();
        var buffer = "";

        foreach (var section in sections)
        {
            var candidate = buffer.Length == 0 ? section : buffer + "\n\n" + section;
            if (buffer.Length > 0 && EstimateTokens(candidate) > TargetTokensPerChunk)
            {
                chunks.Add(buffer);
                // Overlap: the tail of the closed chunk opens the next, so boundary-straddling
                // facts are retrievable from either side.
                buffer = Tail(buffer, OverlapTokens) + "\n\n" + section;
            }
            else
            {
                buffer = candidate;
            }
        }
        if (buffer.Length > 0)
        {
            chunks.Add(buffer);
        }
        return chunks;
    }

    private static string Tail(string text, int tokens)
    {
        var words = text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        return string.Join(" ", words.Skip(Math.Max(0, words.Length - tokens)));
    }
}
