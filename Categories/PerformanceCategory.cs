using Trippie.TW.Core.Base;
using Trippie.TW.Tweaks.Performance;

namespace Trippie.TW.Categories;

/// <summary>
/// Performance optimization tweaks category - Gaming focused.
/// </summary>
public class PerformanceCategory : TweakCategoryBase
{
    public override string Id => "performance";
    public override string Name => "Performance";
    public override string Description => "Gaming and system performance optimizations";
    public override ConsoleColor AccentColor => ConsoleColor.Green;

    public PerformanceCategory()
    {
        RegisterTweaks(
            new UltimatePerformanceTweak(),
            new DisablePowerThrottlingTweak(),
            new DisableGameDVRTweak(),
            new EnableHAGSTweak(),
            new DisableIndexingTweak(),
            new SystemResponsivenessTweak(),
            new DisableHibernationTweak(),
            new OptimizeNTFSTweak(),
            new DisableSSDIndexingTweak(),
            // New gaming optimizations
            new GpuPriorityTweak(),
            new MemoryOptimizationTweak(),
            new TimerResolutionTweak(),
            new DisableCoreParkingTweak(),
            new MouseOptimizationTweak(),
            new FullscreenOptimizationTweak(),
            new MsiModeTweak()
        );
    }
}
