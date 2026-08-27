using Volo.Abp.Application.Services;
using Wathiq.Documents.Localization;

namespace Wathiq.Documents;

/// <summary>Common base so every service localizes (L[...]) from the module's own resource.</summary>
public abstract class DocumentsAppServiceBase : ApplicationService
{
    protected DocumentsAppServiceBase()
    {
        LocalizationResource = typeof(WathiqDocumentsResource);
    }
}
