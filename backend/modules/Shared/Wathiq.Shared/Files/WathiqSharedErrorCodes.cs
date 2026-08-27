namespace Wathiq.Shared.Files;

public static class WathiqSharedErrorCodes
{
    // Prefix must match the namespace registered via AbpExceptionLocalizationOptions.MapCodeNamespace
    // in WathiqSharedModule — ABP uses the code itself as the localization resource key.
    public const string FileTooLarge = "Wathiq.Shared:FileTooLarge";
    public const string FileNotFound = "Wathiq.Shared:FileNotFound";
}
