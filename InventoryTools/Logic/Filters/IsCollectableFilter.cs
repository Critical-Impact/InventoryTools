using System;
using AllaganLib.GameSheets.Sheets.Rows;
using CriticalCommonLib.Models;
using InventoryTools.Logic.GenericFilters;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters;

public class IsCollectableFilter : GenericBooleanFilter
{
    public IsCollectableFilter(ILogger<IsCollectableFilter> logger, ImGuiService imGuiService) : base("IsCollectable", "Is Collectable?", "Is the item collectable?", FilterCategory.Basic, item => item.Item.Base.IsCollectable, item => item.Base.IsCollectable, logger, imGuiService)
    {
    }
}