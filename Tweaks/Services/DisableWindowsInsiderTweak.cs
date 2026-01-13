using Trippie.TW.Core.Base;
using Trippie.TW.Core.Interfaces;

namespace Trippie.TW.Tweaks.Services;

/// <summary>
/// Disables Windows Insider Service to stop beta update checks.
/// </summary>
public class DisableWindowsInsiderTweak : ServiceTweakBase
{
    public override string Id => "disable-windows-insider";
    public override string Name => "Disable Windows Insider Service";
    public override string Description => "Stop background beta-update checks";
    public override TweakRiskLevel RiskLevel => TweakRiskLevel.Safe;

    protected override string[] ServiceNames => new[] { "wisvc" };
    protected override string DefaultStartType => "manual";
}
