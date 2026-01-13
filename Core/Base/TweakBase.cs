using Trippie.TW.Core.Interfaces;

namespace Trippie.TW.Core.Base;

/// <summary>
/// Base class providing common functionality for tweaks.
/// </summary>
public abstract class TweakBase : ITweak
{
    public abstract string Id { get; }
    public abstract string Name { get; }
    public abstract string Description { get; }
    public virtual TweakRiskLevel RiskLevel => TweakRiskLevel.Safe;

    public abstract bool IsApplied();
    public abstract TweakResult Apply();
    public abstract TweakResult Revert();

    protected TweakResult Success(string message) => new(true, message);
    protected TweakResult Failure(string message) => new(false, message);
}
