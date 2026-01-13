using Trippie.TW.Core.Interfaces;

namespace Trippie.TW.Core.Registry;

/// <summary>
/// Central registry for all tweak categories.
/// </summary>
public sealed class CategoryRegistry
{
    private readonly List<ITweakCategory> _categories = new();
    
    public IReadOnlyList<ITweakCategory> Categories => _categories.AsReadOnly();

    public void Register(ITweakCategory category) => _categories.Add(category);
    
    public void RegisterRange(IEnumerable<ITweakCategory> categories)
    {
        foreach (var category in categories)
            Register(category);
    }

    public ITweakCategory? GetById(string id) => 
        _categories.FirstOrDefault(c => c.Id.Equals(id, StringComparison.OrdinalIgnoreCase));

    public ITweak? FindTweak(string categoryId, string tweakId)
    {
        var category = GetById(categoryId);
        return category?.Tweaks.FirstOrDefault(t => t.Id.Equals(tweakId, StringComparison.OrdinalIgnoreCase));
    }
}
