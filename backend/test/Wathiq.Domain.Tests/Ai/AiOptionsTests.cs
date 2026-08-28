using System;
using Shouldly;
using Xunit;

namespace Wathiq.Ai;

public class AiOptionsTests
{
    [Fact]
    public void Extraction_Must_Be_Self_Hosted()
    {
        new AiOptions().Validate();   // default: ollama - fine

        new AiOptions { Extraction = { Provider = "OLLAMA" } }.Validate();   // case-insensitive

        // The privacy wall (FR-AI-002/C1): pointing extraction at a cloud tier must kill the boot.
        Should.Throw<InvalidOperationException>(() =>
                new AiOptions { Extraction = { Provider = "groq" } }.Validate())
            .Message.ShouldContain("FR-AI-002");
    }

    [Fact]
    public void Guides_May_Use_Any_Provider()
    {
        // Public guides content is not personal data - the cloud free tier stays an option there.
        new AiOptions { Guides = { Provider = "groq" } }.Validate();
    }
}
