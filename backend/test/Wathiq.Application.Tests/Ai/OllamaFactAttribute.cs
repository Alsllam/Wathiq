using System;
using Xunit;

namespace Wathiq.Ai;

/// <summary>
/// A live-model test: skipped everywhere by default, run by setting WATHIQ_OLLAMA_SMOKE=1 on a
/// machine with Ollama serving (the dev box). The suite stays green and fast without a model;
/// the smoke stays one env var away, not commented out.
/// </summary>
public sealed class OllamaFactAttribute : FactAttribute
{
    public OllamaFactAttribute()
    {
        if (Environment.GetEnvironmentVariable("WATHIQ_OLLAMA_SMOKE") != "1")
        {
            Skip = "Live Ollama round-trip. Set WATHIQ_OLLAMA_SMOKE=1 with Ollama running (see backend/README.md).";
        }
    }
}
