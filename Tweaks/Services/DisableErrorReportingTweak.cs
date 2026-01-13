using Trippie.TW.Core.Base;
using Trippie.TW.Core.Interfaces;

namespace Trippie.TW.Tweaks.Services;

/// <summary>
/// Disables Windows Error Reporting service.
/// </summary>
public class DisableErrorReportingTweak : ServiceTweakBase
{
    public override string Id => "disable-error-reporting";
    public override string Name => "Disable Windows Error Reporting";
    public override string Description => "Stop collection and sending of error logs to Microsoft";
    public override TweakRiskLevel RiskLevel => TweakRiskLevel.Safe;

    protected override string[] ServiceNames => new[] { "WerSvc" };
    protected override string DefaultStartType => "manual";
}
