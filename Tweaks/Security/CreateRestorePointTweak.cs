using Trippie.TW.Core.Base;
using Trippie.TW.Core.Interfaces;
using Trippie.TW.Core.Restore;
using Trippie.TW.Helpers;

namespace Trippie.TW.Tweaks.Security;

/// <summary>
/// Creates a System Restore Point before applying optimizations.
/// </summary>
public class CreateRestorePointTweak : TweakBase
{
    public override string Id => "create-restore-point";
    public override string Name => "Create System Restore Point";
    public override string Description => "Create a restore point before applying any optimizations (CRITICAL)";
    public override TweakRiskLevel RiskLevel => TweakRiskLevel.Safe;

    public override bool IsApplied()
    {
        // This is a one-time action, always show as available
        return false;
    }

    public override TweakResult Apply()
    {
        SecurityTweakLogger.LogAction("Creating System Restore Point...");

        bool success = RestorePointManager.CreateRestorePoint("TrippieTW_PreOptimization");

        if (success)
        {
            SecurityTweakLogger.Log("Creating System Restore Point", SecStatus.Success, 
                "TrippieTW_PreOptimization");
            return Success("System Restore Point created successfully");
        }
        else
        {
            SecurityTweakLogger.Log("Creating System Restore Point", SecStatus.Failed,
                "System Protection may be disabled");
            return Failure("Could not create restore point. Enable System Protection in System Properties.");
        }
    }

    public override TweakResult Revert()
    {
        SecurityTweakLogger.Log("Create Restore Point", SecStatus.Skipped, "Nothing to revert");
        return Success("Restore point creation is a one-time action");
    }
}
