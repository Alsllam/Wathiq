using Volo.Abp.Localization;

namespace Wathiq.Shared.Localization;

// Its own resource, separate from the host's "Wathiq" resource — Shared is a peer module,
// not part of the app-template's shared kernel (same boundary rule as ADR-001, applied to code).
[LocalizationResourceName("WathiqShared")]
public class WathiqSharedResource
{
}
