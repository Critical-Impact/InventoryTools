using System.Collections.Generic;
using AllaganLib.GameSheets.Sheets.Rows;
using CriticalCommonLib.Models;

using ImGuiNET;
using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters;

public class DefaultSortColumnDirectionFilter : ChoiceFilter<ImGuiSortDirection?>
{
    public override FilterType AvailableIn { get; set; } =
        FilterType.SearchFilter | FilterType.CraftFilter | FilterType.SortingFilter | FilterType.GameItemFilter | FilterType.HistoryFilter | FilterType.CuratedList;

    public override ImGuiSortDirection? CurrentValue(FilterConfiguration configuration)
    {
        return configuration.DefaultSortOrder;
    }

    public override void ResetFilter(FilterConfiguration configuration)
    {
        configuration.DefaultSortOrder = null;
    }

    public override void UpdateFilterConfiguration(FilterConfiguration configuration, ImGuiSortDirection? newValue)
    {
        configuration.DefaultSortOrder = newValue;
    }

    public override string Key { get; set; } = "DefaultSortColumnOrder";
    public override string Name { get; set; } = "Default Sort Column Order";
    public override string HelpText { get; set; } = "The direction to sort the list in by default.";
    public override FilterCategory FilterCategory { get; set; } = FilterCategory.Columns;

    public override ImGuiSortDirection? DefaultValue { get; set; } = null;
    public override bool? FilterItem(FilterConfiguration configuration, InventoryItem item)
    {
        return null;
    }

    public override bool? FilterItem(FilterConfiguration configuration, ItemRow item)
    {
        return null;
    }

    public override List<ImGuiSortDirection?> GetChoices(FilterConfiguration configuration)
    {
        return new List<ImGuiSortDirection?>()
        {
            ImGuiSortDirection.Ascending,
            ImGuiSortDirection.Descending
        };
    }

    public override string GetFormattedChoice(FilterConfiguration filterConfiguration, ImGuiSortDirection? choice)
    {
        if (choice == null)
        {
            return "";
        }

        return choice == ImGuiSortDirection.Ascending ? "Ascending" : "Descending";
    }

    public DefaultSortColumnDirectionFilter(ILogger<DefaultSortColumnDirectionFilter> logger, ImGuiService imGuiService) : base(logger, imGuiService)
    {
    }
}