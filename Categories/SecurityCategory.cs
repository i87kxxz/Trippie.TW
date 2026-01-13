using Trippie.TW.Core.Base;
using Trippie.TW.Tweaks.Security;

namespace Trippie.TW.Categories;

/// <summary>
/// Security hardening and system integrity category.
/// </summary>
public class SecurityCategory : TweakCategoryBase
{
    public override string Id => "security";
    public override string Name => "Security";
    public override string Description => "Security hardening and system protection";
    public override ConsoleColor AccentColor => ConsoleColor.Red;

    public SecurityCategory()
    {
        RegisterTweaks(
            new CreateRestorePointTweak(),
            new DisableAutoRunTweak(),
            new OptimizeUacTweak(),
            new DisableRemoteAssistanceTweak(),
            new SpectreMeltdownTweak(),
            new DisableScriptHostTweak(),
            new DisableStickyKeysTweak(),
            new DefenderToggleTweak(),
            new ClearPagefileTweak(),
            new DisableCortanaTweak()
        );
    }
}
