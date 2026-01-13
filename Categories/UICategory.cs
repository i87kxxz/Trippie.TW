using Trippie.TW.Core.Base;

namespace Trippie.TW.Categories;

/// <summary>
/// User Interface customization category.
/// </summary>
public class UICategory : TweakCategoryBase
{
    public override string Id => "ui";
    public override string Name => "User Interface";
    public override string Description => "Visual and UI customization options";
    public override ConsoleColor AccentColor => ConsoleColor.Cyan;

    public UICategory()
    {
        // Tweaks will be registered here
    }
}
