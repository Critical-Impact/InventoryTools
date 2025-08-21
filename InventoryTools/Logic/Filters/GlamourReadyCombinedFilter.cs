using System;
using AllaganLib.GameSheets.Sheets.Rows;
using CriticalCommonLib.Models;
using InventoryTools.Logic.GenericFilters;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters;

public class GlamourReadyCombinedFilter : GenericBooleanFilter
{
    public GlamourReadyCombinedFilter(ILogger<GlamourReadyCombinedFilter> logger, ImGuiService imGuiService) : base("grCombined", "Glamour Ready Combined",
        "Is the item combined in the glamour chest?", FilterCategory.Basic,
        item => item.SortedCategory == InventoryCategory.GlamourChest && item.GlamourId != 0, null, logger, imGuiService)
    {
    }
}