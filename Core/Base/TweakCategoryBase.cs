using Trippie.TW.Core.Interfaces;

namespace Trippie.TW.Core.Base;

/// <summary>
/// Base class for tweak categories.
/// </summary>
public abstract class TweakCategoryBase : ITweakCategory
{
    private readonly List<ITweak> _tweaks = new();

    public abstract string Id { get; }
    public abstract string Name { get; }
    public abstract string Description { get; }
    public virtual ConsoleColor AccentColor => ConsoleColor.Cyan;
    public IReadOnlyList<ITweak> Tweaks => _tweaks.AsReadOnly();

    protected void RegisterTweak(ITweak tweak) => _tweaks.Add(tweak);
    protected void RegisterTweaks(params ITweak[] tweaks) => _tweaks.AddRange(tweaks);
}
