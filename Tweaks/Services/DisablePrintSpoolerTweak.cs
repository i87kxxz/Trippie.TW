using Trippie.TW.Core.Base;
using Trippie.TW.Core.Interfaces;

namespace Trippie.TW.Tweaks.Services;

/// <summary>
/// Disables the Print Spooler service for users without printers.
/// </summary>
public class DisablePrintSpoolerTweak : ServiceTweakBase
{
    public override string Id => "disable-print-spooler";
    public override string Name => "Disable Print Spooler";
    public override string Description => "Stop the print spooler service (for users without printers)";
    public override TweakRiskLevel RiskLevel => TweakRiskLevel.Safe;

    protected override string[] ServiceNames => new[] { "Spooler" };
    protected override string DefaultStartType => "auto";
}
