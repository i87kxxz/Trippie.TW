using Trippie.TW.Core.Base;
using Trippie.TW.Tweaks.UI;

namespace Trippie.TW.Categories;

/// <summary>
/// User Interface customization category.
/// </summary>
public class UICategory : TweakCategoryBase
{
    public override string Id => "ui";
    public override string Name => "User Interface";
    public override string Description => "Visual optimizations and UI customization";
    public override ConsoleColor AccentColor => ConsoleColor.Cyan;

    public UICategory()
    {
        RegisterTweaks(
            new MenuShowDelayTweak(),
            new DisableTransparencyTweak(),
            new DisableStartupSoundTweak(),
            new DisableAnimationsTweak(),
            new AutoEndTasksTweak(),
            new ClassicContextMenuTweak(),
            new DisableAeroShakeTweak(),
            new TaskbarAlignmentTweak()
        );
    }
}
