using Trippie.TW.Core.Base;
using Trippie.TW.Core.Interfaces;

namespace Trippie.TW.Tweaks.Services;

/// <summary>
/// Disables Touch Keyboard service for non-touchscreen users.
/// </summary>
public class DisableTouchKeyboardTweak : ServiceTweakBase
{
    public override string Id => "disable-touch-keyboard";
    public override string Name => "Disable Touch Keyboard";
    public override string Description => "Stop touch keyboard service (for non-touchscreen PCs)";
    public override TweakRiskLevel RiskLevel => TweakRiskLevel.Safe;

    protected override string[] ServiceNames => new[] { "TabletInputService" };
    protected override string DefaultStartType => "manual";
}
