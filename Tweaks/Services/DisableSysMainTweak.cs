using Trippie.TW.Core.Base;
using Trippie.TW.Core.Interfaces;

namespace Trippie.TW.Tweaks.Services;

/// <summary>
/// Disables SysMain (Superfetch) to reduce disk spikes.
/// </summary>
public class DisableSysMainTweak : ServiceTweakBase
{
    public override string Id => "disable-sysmain";
    public override string Name => "Disable SysMain (Superfetch)";
    public override string Description => "Stop Superfetch to reduce disk spikes (especially on HDDs)";
    public override TweakRiskLevel RiskLevel => TweakRiskLevel.Moderate;

    protected override string[] ServiceNames => new[] { "SysMain" };
    protected override string DefaultStartType => "auto";
}
