using System;
using InventoryTools.Logic.Filters;

namespace InventoryTools.Extensions;

public static class FilterCategoryExtensions
{
    public static string FormattedName(this FilterCategory filterCategory)
    {
        return filterCategory switch
        {
            FilterCategory.SourceCategories => "Source (Categories)",
            FilterCategory.UseCategories => "Use (Categories)",
            _ => filterCategory.ToString().ToSentence()
        };
    }
}