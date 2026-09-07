using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Domain.Repositories;
using Wathiq.Guides.Guides;
using Wathiq.Guides.Retrieval;
using Wathiq.Shared.GuideChat;

namespace Wathiq.Guides.Chat;

/// <summary>
/// The grounded loop: retrieve → ask → VALIDATE → answer or refuse. [Authorize] without a
/// permission: reading guides is anonymous, but chat spends model time under the per-user cap -
/// the cap needs an identity, so chat needs a sign-in (any user, no special grant).
/// </summary>
[Authorize]
public class ChatAppService : GuidesAppServiceBase, IChatAppService
{
    private const int SnippetLength = 200;

    private readonly IGuideRetriever _retriever;
    private readonly IGuideAnswerer _answerer;
    private readonly IRepository<Guide, Guid> _guides;
    private readonly IRepository<GuideVersion, Guid> _versions;

    public ChatAppService(
        IGuideRetriever retriever,
        IGuideAnswerer answerer,
        IRepository<Guide, Guid> guides,
        IRepository<GuideVersion, Guid> versions)
    {
        _retriever = retriever;
        _answerer = answerer;
        _guides = guides;
        _versions = versions;
    }

    public async Task<GuideChatResponseDto> AskAsync(GuideChatRequestDto input)
    {
        var matches = await _retriever.RetrieveAsync(input.Question);
        if (matches.Count == 0)
        {
            // Below the similarity floor everywhere: the corpus does not speak to this question.
            return Refusal();
        }

        // Labels C1..Cn: the model cites labels, never real ids - and only THESE labels exist.
        var byLabel = matches
            .Select((match, index) => (Label: $"C{index + 1}", Match: match))
            .ToDictionary(x => x.Label, x => x.Match);

        var modelAnswer = await _answerer.AnswerAsync(
            input.Question,
            byLabel.Select(p => new GuideExcerpt(p.Key, p.Value.Text)).ToList());

        if (modelAnswer.AnswerText is null)
        {
            return Refusal();   // the model (or its parser) declined - same honest outcome
        }

        // FR-AI-003 for RAG: a citation is only real if it maps to something we retrieved.
        var cited = modelAnswer.CitedLabels.Distinct()
            .Where(byLabel.ContainsKey)
            .Select(label => byLabel[label])
            .ToList();
        var dropped = modelAnswer.CitedLabels.Distinct().Count() != cited.Count;

        if (cited.Count == 0)
        {
            // An answer with no surviving citation is ungrounded by definition - refuse it
            // whole rather than serve prose we cannot source (dropped flag tells the story).
            return Refusal(dropped);
        }

        var citations = await BuildCitationsAsync(cited);

        return new GuideChatResponseDto
        {
            Answered = true,
            Answer = modelAnswer.AnswerText,
            Citations = citations,
            LastVerifiedAt = citations.Min(c => c.LastVerifiedAt),   // oldest source = honest freshness
            HallucinatedCitationsDropped = dropped
        };
    }

    private GuideChatResponseDto Refusal(bool dropped = false) => new()
    {
        Answered = false,
        Message = L["Wathiq.Guides:ChatNoAnswer"],
        HallucinatedCitationsDropped = dropped
    };

    private async Task<List<GuideChatCitationDto>> BuildCitationsAsync(List<GuideChunkMatch> cited)
    {
        var versionIds = cited.Select(c => c.GuideVersionId).Distinct().ToList();
        var versions = (await _versions.GetListAsync(v => versionIds.Contains(v.Id))).ToDictionary(v => v.Id);
        var guideIds = versions.Values.Select(v => v.GuideId).Distinct().ToList();
        var guides = (await _guides.GetListAsync(g => guideIds.Contains(g.Id))).ToDictionary(g => g.Id);

        return cited.Select(match =>
        {
            var version = versions[match.GuideVersionId];
            var guide = guides[version.GuideId];
            return new GuideChatCitationDto
            {
                ChunkId = match.ChunkId,
                GuideVersionId = match.GuideVersionId,
                GuideSlug = guide.Slug,
                TitleAr = guide.TitleAr,
                TitleEn = guide.TitleEn,
                Snippet = match.Text.Length <= SnippetLength ? match.Text : match.Text[..SnippetLength] + "…",
                LastVerifiedAt = version.LastVerifiedAt
            };
        }).ToList();
    }
}
