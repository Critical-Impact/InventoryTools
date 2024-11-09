using System.Collections.Generic;
using System.Linq;
using AllaganLib.GameSheets.Sheets.Rows;
using CriticalCommonLib.Models;

using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters;

public class DefaultSortColumnFilter : ChoiceFilter<string>
{
    public override FilterType AvailableIn { get; set; } =
        FilterType.SearchFilter | FilterType.CraftFilter | FilterType.SortingFilter | FilterType.GameItemFilter | FilterType.HistoryFilter | FilterType.CuratedList;

    public override string? CurrentValue(FilterConfiguration configuration)
    {
        return configuration.DefaultSortColumn;
    }

    public override void ResetFilter(FilterConfiguration configuration)
    {
        configuration.DefaultSortColumn = null;
    }

    public override void UpdateFilterConfiguration(FilterConfiguration configuration, string? newValue)
    {
        configuration.DefaultSortColumn = newValue;
    }

    public override string Key { get; set; } = "DefaultSortColumn";
    public override string Name { get; set; } = "Default Sort Column";
    public override string HelpText { get; set; } = "The column to use to sort by default";
    public override FilterCategory FilterCategory { get; set; } = FilterCategory.Columns;

    public override string? DefaultValue { get; set; } = null;
    public override bool? FilterItem(FilterConfiguration configuration, InventoryItem item)
    {
        return null;
    }

    public override bool? FilterItem(FilterConfiguration configuration, ItemRow item)
    {
        return null;
    }

    public override List<string> GetChoices(FilterConfiguration configuration)
    {
        if (configuration.Columns != null) return configuration.Columns.Select(c => c.Key).ToList();
        return new();
    }

    public override string GetFormattedChoice(FilterConfiguration filterConfiguration, string choice)
    {
        var column = filterConfiguration.GetColumn(choice);
        if (column == null)
        {
            return "";
        }
        return column.Name ?? column.Column.Name;
    }

    public DefaultSortColumnFilter(ILogger<DefaultSortColumnFilter> logger, ImGuiService imGuiService) : base(logger, imGuiService)
    {
    }
}