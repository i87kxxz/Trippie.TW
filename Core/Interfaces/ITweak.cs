namespace Trippie.TW.Core.Interfaces;

/// <summary>
/// Base interface for all tweaks in the application.
/// </summary>
public interface ITweak
{
    string Id { get; }
    string Name { get; }
    string Description { get; }
    TweakRiskLevel RiskLevel { get; }
    
    bool IsApplied();
    TweakResult Apply();
    TweakResult Revert();
}

public enum TweakRiskLevel
{
    Safe,
    Moderate,
    Advanced,
    Experimental
}

public record TweakResult(bool Success, string Message);
