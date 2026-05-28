using AllaganLib.GameSheets.Sheets.Rows;
using CriticalCommonLib.Models;
using InventoryTools.Logic.GenericFilters;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters;

public class IsStackableFilter : GenericBooleanFilter
{
    public IsStackableFilter(ILogger<IsStackableFilter> logger, ImGuiService imGuiService) : base("IsStackable", "Is Stackable?", "Is the item stackable (can hold more than 1 in a stack)?", FilterCategory.Basic, item => item.Item.Base.StackSize > 1, item => item.Base.StackSize > 1, logger, imGuiService)
    {
    }
}
