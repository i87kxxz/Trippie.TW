using Trippie.TW.Core.Base;

namespace Trippie.TW.Categories;

/// <summary>
/// Security-related tweaks category.
/// </summary>
public class SecurityCategory : TweakCategoryBase
{
    public override string Id => "security";
    public override string Name => "Security";
    public override string Description => "Security hardening and configuration";
    public override ConsoleColor AccentColor => ConsoleColor.Red;

    public SecurityCategory()
    {
        // Tweaks will be registered here
    }
}
