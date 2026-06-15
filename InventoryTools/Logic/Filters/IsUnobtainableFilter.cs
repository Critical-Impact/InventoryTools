using System;
using AllaganLib.GameSheets.Sheets.Rows;
using CriticalCommonLib.Models;
using InventoryTools.Logic.GenericFilters;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters;

public class IsUnobtainableFilter : GenericBooleanFilter
{
    public IsUnobtainableFilter(ILogger<IsUnobtainableFilter> logger, ImGuiService imGuiService) : base("IsUnobtainable", "Is the item unobtainable?", "Has the item been made unobtainable?", FilterCategory.Basic, item => item.Item.IsUnobtainable, item => item.IsUnobtainable,  logger, imGuiService)
    {
    }
}