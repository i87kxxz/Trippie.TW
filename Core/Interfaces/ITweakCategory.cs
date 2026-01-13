namespace Trippie.TW.Core.Interfaces;

/// <summary>
/// Represents a category of related tweaks.
/// </summary>
public interface ITweakCategory
{
    string Id { get; }
    string Name { get; }
    string Description { get; }
    ConsoleColor AccentColor { get; }
    IReadOnlyList<ITweak> Tweaks { get; }
}
