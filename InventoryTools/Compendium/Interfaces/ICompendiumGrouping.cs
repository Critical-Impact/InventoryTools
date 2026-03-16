using System;

namespace InventoryTools.Compendium.Interfaces;

public interface ICompendiumGrouping<TData> : ICompendiumGrouping
{
    public Func<TData, object> GroupFunc { get; }
}

public interface ICompendiumGrouping
{
    public string Key { get; }
    public string Name { get; }
    public Func<object, string> GroupMapping { get; init; }
}