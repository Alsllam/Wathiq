using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.DependencyInjection;
using Wathiq.Shared.Extraction;

namespace Wathiq.Ai.Extraction;

/// <summary>
/// The Ai module's implementation of the Shared seam. Takes the KEYED extraction client, so the
/// FR-AI-002 privacy wall (ollama-only, enforced at boot) and the 3.3 cap/ledger decorator are
/// structurally in every call's path - there is no way to extract around them.
/// </summary>
public class DocumentDataExtractor : IDocumentDataExtractor, ITransientDependency
{
    public const string PromptVersion = "extract-document@v1";

    private static readonly string PromptText = LoadPrompt();

    private readonly IChatClient _chatClient;
    private readonly AiOptions _options;

    public DocumentDataExtractor(
        [FromKeyedServices(AiConsts.ExtractionClientKey)] IChatClient chatClient,
        AiOptions options)
    {
        _chatClient = chatClient;
        _options = options;
    }

    public async Task<DocumentDataProposal> ExtractAsync(string ocrText, CancellationToken cancellationToken = default)
    {
        var proposal = new DocumentDataProposal
        {
            PromptVersion = PromptVersion,
            Provider = _options.Extraction.Provider,
            Model = _options.Extraction.Model
        };

        var chatOptions = new ChatOptions
        {
            // Ollama-side JSON mode; the parser below still assumes nothing.
            ResponseFormat = ChatResponseFormat.Json,
            // Flows into the 3.3 decorator: every ledger row names the prompt that caused it.
            AdditionalProperties = new AdditionalPropertiesDictionary
            {
                [AiConsts.PromptVersionOptionKey] = PromptVersion
            }
        };

        var stopwatch = Stopwatch.StartNew();
        var response = await _chatClient.GetResponseAsync(
        [
            new ChatMessage(ChatRole.System, PromptText),
            new ChatMessage(ChatRole.User, ocrText)
        ], chatOptions, cancellationToken);
        stopwatch.Stop();
        proposal.DurationMs = (int)Math.Min(stopwatch.ElapsedMilliseconds, int.MaxValue);

        ParseAndValidate(response.Text, proposal);
        return proposal;
    }

    /// <summary>Everything below this line treats the model as an untrusted client (FR-AI-003).</summary>
    private static void ParseAndValidate(string raw, DocumentDataProposal proposal)
    {
        var json = ExtractedValueParser.ExtractJsonObject(raw);
        if (json == null)
        {
            proposal.RawJson = raw;   // keep the evidence for the review UI / evals
            proposal.Warnings.Add("The model did not return JSON; no fields were extracted.");
            return;
        }

        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(json);
        }
        catch (JsonException)
        {
            proposal.RawJson = raw;
            proposal.Warnings.Add("The model returned malformed JSON; no fields were extracted.");
            return;
        }

        using (document)
        {
            proposal.RawJson = json;
            var root = document.RootElement;

            proposal.Number = TakeString(root, "number", out var rawNumber) is { } n
                ? ExtractedValueParser.TryParseDocumentNumber(n)
                : null;
            Warn(proposal, proposal.Number == null && rawNumber != null, $"Number '{rawNumber}' failed validation - dropped.");

            proposal.IssueDate = ExtractedValueParser.TryParseDate(TakeString(root, "issue_date", out var rawIssue));
            Warn(proposal, proposal.IssueDate == null && rawIssue != null, $"Issue date '{rawIssue}' is not a valid YYYY-MM-DD date - dropped.");

            proposal.ExpiryDate = ExtractedValueParser.TryParseDate(TakeString(root, "expiry_date", out var rawExpiry));
            Warn(proposal, proposal.ExpiryDate == null && rawExpiry != null, $"Expiry date '{rawExpiry}' is not a valid YYYY-MM-DD date - dropped.");

            // Cross-field sanity: an inverted pair means at least one date is wrong and we cannot
            // tell which - dropping both beats presenting a confidently wrong one (FR-AI-003).
            if (proposal.IssueDate is { } issue && proposal.ExpiryDate is { } expiry && expiry < issue)
            {
                proposal.Warnings.Add($"Expiry {expiry:yyyy-MM-dd} is before issue {issue:yyyy-MM-dd} - both dates dropped.");
                proposal.IssueDate = null;
                proposal.ExpiryDate = null;
            }

            proposal.HolderName = ExtractedValueParser.SanitizeText(TakeString(root, "holder_name", out _), maxLength: 128);
            proposal.DocumentKind = ExtractedValueParser.SanitizeText(TakeString(root, "document_kind", out _), maxLength: 64);

            if (root.TryGetProperty("confidence", out var conf) && conf.ValueKind == JsonValueKind.Number
                && conf.TryGetDecimal(out var value) && value is >= 0m and <= 1m)
            {
                proposal.Confidence = value;
            }
        }
    }

    private static string? TakeString(JsonElement root, string name, out string? raw)
    {
        raw = root.TryGetProperty(name, out var el) && el.ValueKind == JsonValueKind.String
            ? el.GetString()
            : null;
        return raw;
    }

    private static void Warn(DocumentDataProposal proposal, bool condition, string message)
    {
        if (condition)
        {
            proposal.Warnings.Add(message);
        }
    }

    private static string LoadPrompt()
    {
        // Embedded resource = the prompt version that shipped WITH this binary; a file on disk
        // could drift from the PromptVersion const that the ledger records.
        var assembly = typeof(DocumentDataExtractor).Assembly;
        const string name = "Wathiq.Ai.Prompts.extract-document.v1.txt";   // pinned in the csproj
        using var stream = assembly.GetManifestResourceStream(name)
                           ?? throw new InvalidOperationException($"Embedded prompt '{name}' not found.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
