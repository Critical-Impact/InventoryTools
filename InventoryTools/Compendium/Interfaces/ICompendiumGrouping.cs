using System;
using System.Collections.Generic;

namespace InventoryTools.Compendium.Interfaces;

public interface ICompendiumGrouping<TData> : ICompendiumGrouping
{
    /// <summary>
    /// The groups a row is in. Most groupings put a row in one group, but a row can be in more than one (for example a mob that spawns in several zones).
    /// </summary>
    public IEnumerable<object> GetGroupKeys(TData row);
}

public interface ICompendiumGrouping
{
    public string Key { get; }
    public string Name { get; }
    public Func<object, string> GroupMapping { get; init; }
}