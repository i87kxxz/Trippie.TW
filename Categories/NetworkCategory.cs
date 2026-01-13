using Trippie.TW.Core.Base;

namespace Trippie.TW.Categories;

/// <summary>
/// Network-related tweaks category.
/// </summary>
public class NetworkCategory : TweakCategoryBase
{
    public override string Id => "network";
    public override string Name => "Network";
    public override string Description => "Network and connectivity optimizations";
    public override ConsoleColor AccentColor => ConsoleColor.Blue;

    public NetworkCategory()
    {
        // Tweaks will be registered here
    }
}
