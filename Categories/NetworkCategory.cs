using Trippie.TW.Core.Base;
using Trippie.TW.Tweaks.Network;

namespace Trippie.TW.Categories;

/// <summary>
/// Network optimization and latency reduction category.
/// </summary>
public class NetworkCategory : TweakCategoryBase
{
    public override string Id => "network";
    public override string Name => "Network";
    public override string Description => "Network optimization and latency reduction";
    public override ConsoleColor AccentColor => ConsoleColor.Blue;

    public NetworkCategory()
    {
        RegisterTweaks(
            new DisableNagleTweak(),
            new NetworkThrottlingTweak(),
            new FlushDnsTweak(),
            new OptimizeTcpWindowTweak(),
            new DisableNetBiosTweak(),
            new SetDnsTweak(),
            new DisableLsoTweak()
        );
    }
}
