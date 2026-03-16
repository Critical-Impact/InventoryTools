using System;
using InventoryTools.Compendium.Interfaces;

namespace InventoryTools.Compendium.Models;

public record CompendiumGrouping<TData> : ICompendiumGrouping<TData>
{
    public string Key { get; init; }
    public string Name { get; init; }
    public Func<object, string>? GroupMapping { get; init; }
    public Func<TData, object> GroupFunc { get; init; }
}