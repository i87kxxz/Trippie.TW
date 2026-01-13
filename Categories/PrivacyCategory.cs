using Trippie.TW.Core.Base;

namespace Trippie.TW.Categories;

/// <summary>
/// Privacy-related tweaks category.
/// </summary>
public class PrivacyCategory : TweakCategoryBase
{
    public override string Id => "privacy";
    public override string Name => "Privacy";
    public override string Description => "Telemetry, tracking, and data collection settings";
    public override ConsoleColor AccentColor => ConsoleColor.Magenta;

    public PrivacyCategory()
    {
        // Tweaks will be registered here
    }
}
