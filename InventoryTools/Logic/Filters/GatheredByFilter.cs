using System.Collections.Generic;
using System.Linq;
using CriticalCommonLib;
using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Filters.Abstract;

namespace InventoryTools.Logic.Filters;

public class GatheredByFilter : UintMultipleChoiceFilter
{
    public override string Key { get; set; } = "GatheredByFilter";
    public override string Name { get; set; } = "Gathered By?";
    public override string HelpText { get; set; } = "How is this item gathered?";
    public override FilterCategory FilterCategory { get; set; } = FilterCategory.Gathering;

    public override List<uint> DefaultValue { get; set; } = new();

    public override FilterType AvailableIn { get; set; } = FilterType.SearchFilter | FilterType.SortingFilter |
                                                           FilterType.HistoryFilter | FilterType.GameItemFilter;

    public override bool? FilterItem(FilterConfiguration configuration, InventoryItem item)
    {
        return FilterItem(configuration, item.Item);
    }

    public override bool? FilterItem(FilterConfiguration configuration, ItemEx item)
    {
        
        var currentValue = CurrentValue(configuration);
        if (currentValue.Count == 0)
        {
            return null;
        }

        if (currentValue.Contains(10) && item.ObtainedFishing)
        {
            return true;
        }

        return item.GatheringTypes.Select(c => c.Row)
            .Any(c => currentValue.Contains(c));
    }

    public override Dictionary<uint, string> GetChoices(FilterConfiguration configuration)
    {
        var dictionary = Service.ExcelCache.GetSheet<GatheringTypeEx>().Where(c => c.RowId < 4).ToDictionary(c => c.RowId, c => c.FormattedName);
        dictionary.Add(10, "Fishing");
        return dictionary;
    }

    public override bool HideAlreadyPicked { get; set; } = true;
}