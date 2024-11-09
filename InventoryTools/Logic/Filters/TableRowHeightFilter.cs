using System;
using AllaganLib.GameSheets.Sheets.Rows;
using CriticalCommonLib.Models;

using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters;

public class TableRowHeightFilter : IntegerFilter
{
    public override FilterType AvailableIn { get; set; } =
        FilterType.SearchFilter | FilterType.CraftFilter | FilterType.SortingFilter | FilterType.GameItemFilter | FilterType.HistoryFilter | FilterType.CuratedList;
    public override string Key { get; set; } = "TableRowHeight";
    public override string Name { get; set; } = "Table Row Height";
    public override string HelpText { get; set; } = "How many pixels high should each item row try to display at?";
    public override bool ShowReset { get; set; } = false;
    public override FilterCategory FilterCategory { get; set; } = FilterCategory.Display;

    public override int? CurrentValue(FilterConfiguration configuration)
    {
        return configuration.TableHeight;
    }

    public override void UpdateFilterConfiguration(FilterConfiguration configuration, int? newValue)
    {
        if (newValue != null)
        {
            configuration.TableHeight = Math.Clamp(newValue.Value, 8, 128);
        }
    }

    public override bool? FilterItem(FilterConfiguration configuration, InventoryItem item)
    {
        return null;
    }

    public override bool? FilterItem(FilterConfiguration configuration, ItemRow item)
    {
        return null;
    }

    public TableRowHeightFilter(ILogger<TableRowHeightFilter> logger, ImGuiService imGuiService) : base(logger, imGuiService)
    {
    }
}