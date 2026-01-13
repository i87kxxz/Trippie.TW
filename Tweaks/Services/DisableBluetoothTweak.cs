using Trippie.TW.Core.Base;
using Trippie.TW.Core.Interfaces;

namespace Trippie.TW.Tweaks.Services;

/// <summary>
/// Disables Bluetooth services for desktop users without Bluetooth.
/// </summary>
public class DisableBluetoothTweak : ServiceTweakBase
{
    public override string Id => "disable-bluetooth";
    public override string Name => "Disable Bluetooth Support";
    public override string Description => "Stop Bluetooth services (for desktops without Bluetooth)";
    public override TweakRiskLevel RiskLevel => TweakRiskLevel.Safe;

    protected override string[] ServiceNames => new[] { "bthserv", "BTAGService" };
    protected override string DefaultStartType => "manual";
}
