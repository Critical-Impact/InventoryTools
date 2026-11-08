using System;
using System.Collections.Generic;
using InventoryTools.Compendium.Interfaces;

namespace InventoryTools.Compendium.Models;

public record CompendiumGrouping<TData> : ICompendiumGrouping<TData>
{
    public string Key { get; init; }
    public string Name { get; init; }
    public Func<object, string>? GroupMapping { get; init; }

    /// <summary>
    /// Puts a row in one group. Set this or <see cref="GroupsFunc"/>.
    /// </summary>
    public Func<TData, object>? GroupFunc { get; init; }

    /// <summary>
    /// Puts a row in zero or more groups. When set, it is used in place of <see cref="GroupFunc"/>.
    /// </summary>
    public Func<TData, IEnumerable<object>>? GroupsFunc { get; init; }

    public IEnumerable<object> GetGroupKeys(TData row)
    {
        if (GroupsFunc != null)
        {
            return GroupsFunc(row);
        }

        if (GroupFunc != null)
        {
            return [GroupFunc(row)];
        }

        return [];
    }
}
