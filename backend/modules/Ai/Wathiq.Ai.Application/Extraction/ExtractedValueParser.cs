using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Wathiq.Ai.Extraction;

/// <summary>
/// FR-AI-003's teeth: pure, deterministic re-validation of everything the model says. The model
/// is treated like an untrusted API client - the same posture as input DTO validation, applied
/// to output. Static and side-effect free so 3.8's evals can hammer it directly.
/// </summary>
public static partial class ExtractedValueParser
{
    [GeneratedRegex(@"^[A-Za-z0-9][A-Za-z0-9 /\-\.]{0,63}$")]
    private static partial Regex DocumentNumberPattern();

    /// <summary>
    /// Arabic-Indic (٠١٢...) and Eastern Arabic-Indic (۰۱۲...) digits to ASCII, so one strict
    /// date/number grammar covers both scripts. Everything else passes through untouched.
    /// </summary>
    public static string NormalizeDigits(string value)
    {
        var sb = new StringBuilder(value.Length);
        foreach (var ch in value)
        {
            sb.Append(ch switch
            {
                >= '٠' and <= '٩' => (char)('0' + (ch - '٠')),   // ٠-٩
                >= '۰' and <= '۹' => (char)('0' + (ch - '۰')),   // ۰-۹
                _ => ch
            });
        }

        return sb.ToString();
    }

    /// <summary>
    /// Strict ISO date only: TryParseExact rejects impossible calendar dates (2027-02-30, month
    /// 13) that a plain regex would wave through - the checkpoint answer lives on this line.
    /// </summary>
    public static DateOnly? TryParseDate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return DateOnly.TryParseExact(
            NormalizeDigits(value.Trim()), "yyyy-MM-dd", CultureInfo.InvariantCulture,
            DateTimeStyles.None, out var date)
            ? date
            : null;
    }

    /// <summary>Allow-list, not deny-list: a document number is short and boring, or it is dropped.</summary>
    public static string? TryParseDocumentNumber(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = NormalizeDigits(value.Trim());
        return DocumentNumberPattern().IsMatch(normalized) ? normalized : null;
    }

    /// <summary>Free text, but bounded and stripped of control chars (a prompt-injection foothold).</summary>
    public static string? SanitizeText(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var cleaned = new string(value.Trim().Where(c => !char.IsControl(c)).ToArray());
        return cleaned.Length == 0 ? null
            : cleaned.Length <= maxLength ? cleaned
            : cleaned[..maxLength];
    }

    /// <summary>Model output often arrives fenced (```json ... ```); the payload is the outermost {...}.</summary>
    public static string? ExtractJsonObject(string raw)
    {
        var start = raw.IndexOf('{');
        var end = raw.LastIndexOf('}');
        return start >= 0 && end > start ? raw[start..(end + 1)] : null;
    }
}
