using Trippie.TW.Core.Base;
using Trippie.TW.Core.Interfaces;

namespace Trippie.TW.Tweaks.Services;

/// <summary>
/// Disables Remote Registry service for security and performance.
/// </summary>
public class DisableRemoteRegistryTweak : ServiceTweakBase
{
    public override string Id => "disable-remote-registry";
    public override string Name => "Disable Remote Registry";
    public override string Description => "Disable remote registry access (security & performance)";
    public override TweakRiskLevel RiskLevel => TweakRiskLevel.Safe;

    protected override string[] ServiceNames => new[] { "RemoteRegistry" };
    protected override string DefaultStartType => "manual";
}
