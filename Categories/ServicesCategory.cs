using Trippie.TW.Core.Base;
using Trippie.TW.Tweaks.Services;

namespace Trippie.TW.Categories;

/// <summary>
/// Windows Services management category.
/// </summary>
public class ServicesCategory : TweakCategoryBase
{
    public override string Id => "services";
    public override string Name => "Services";
    public override string Description => "Disable non-essential background services";
    public override ConsoleColor AccentColor => ConsoleColor.Yellow;

    public ServicesCategory()
    {
        RegisterTweaks(
            new DisablePrintSpoolerTweak(),
            new DisableBluetoothTweak(),
            new DisableRemoteRegistryTweak(),
            new DisableSysMainTweak(),
            new DisableWindowsInsiderTweak(),
            new DisableTouchKeyboardTweak(),
            new DisableFaxTweak(),
            new DisableErrorReportingTweak()
        );
    }
}
