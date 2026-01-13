using Trippie.TW.Core.Base;
using Trippie.TW.Core.Interfaces;

namespace Trippie.TW.Tweaks.Services;

/// <summary>
/// Disables the legacy Fax service.
/// </summary>
public class DisableFaxTweak : ServiceTweakBase
{
    public override string Id => "disable-fax";
    public override string Name => "Disable Fax Service";
    public override string Description => "Deactivate the obsolete legacy faxing service";
    public override TweakRiskLevel RiskLevel => TweakRiskLevel.Safe;

    protected override string[] ServiceNames => new[] { "Fax" };
    protected override string DefaultStartType => "manual";
}
