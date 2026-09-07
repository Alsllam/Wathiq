using System.Collections.Generic;
using System.Text.Json;
using Wathiq.Ai.Extraction;

namespace Wathiq.Ai.GuideChat;

/// <summary>
/// Pure parsing of the guides-chat JSON contract - the 3.6 discipline: the model's output is
/// UNTRUSTED TEXT until a parser says otherwise, and every malformed shape degrades to the safe
/// outcome (a refusal), never to an exception or a fabricated answer.
/// </summary>
public static class GuideAnswerParser
{
    public static (string? Answer, IReadOnlyList<string> Citations) Parse(string raw)
    {
        // Fence-strip + first-object extraction, shared with extraction (models love ```json).
        var json = ExtractedValueParser.ExtractJsonObject(raw);
        if (json is null)
        {
            return (null, []);   // no JSON at all → refuse
        }

        try
        {
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;

            string? answer = null;
            if (root.TryGetProperty("answer", out var answerElement) && answerElement.ValueKind == JsonValueKind.String)
            {
                var text = answerElement.GetString();
                answer = string.IsNullOrWhiteSpace(text) ? null : text!.Trim();
            }

            var citations = new List<string>();
            if (root.TryGetProperty("citations", out var citationsElement) && citationsElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var entry in citationsElement.EnumerateArray())
                {
                    // Non-string entries are model noise, skipped - validation against the
                    // RETRIEVED set happens on the Guides side, where that set is known.
                    if (entry.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(entry.GetString()))
                    {
                        citations.Add(entry.GetString()!.Trim());
                    }
                }
            }

            return (answer, citations);
        }
        catch (JsonException)
        {
            return (null, []);   // torn JSON → refuse, never throw
        }
    }
}
