using Trippie.TW.Core.Base;

namespace Trippie.TW.Categories;

/// <summary>
/// Performance optimization tweaks category.
/// </summary>
public class PerformanceCategory : TweakCategoryBase
{
    public override string Id => "performance";
    public override string Name => "Performance";
    public override string Description => "System performance and optimization tweaks";
    public override ConsoleColor AccentColor => ConsoleColor.Green;

    public PerformanceCategory()
    {
        // Tweaks will be registered here
    }
}
