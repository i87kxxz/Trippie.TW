using Trippie.TW.Core.Base;

namespace Trippie.TW.Categories;

/// <summary>
/// Windows Services management category.
/// </summary>
public class ServicesCategory : TweakCategoryBase
{
    public override string Id => "services";
    public override string Name => "Services";
    public override string Description => "Manage Windows background services";
    public override ConsoleColor AccentColor => ConsoleColor.Yellow;

    public ServicesCategory()
    {
        // Tweaks will be registered here
    }
}
