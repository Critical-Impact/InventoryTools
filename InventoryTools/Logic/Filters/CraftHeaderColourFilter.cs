using System.Numerics;
using AllaganLib.GameSheets.Sheets.Rows;
using CriticalCommonLib.Models;

using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters;

public class CraftHeaderColourFilter : ColorFilter
{
    public override string Key { get; set; } = "CraftHeaderColour";
    public override string Name { get; set; } = "Header Text Colour";
    public override string HelpText { get; set; } = "The colour of the header text in the craft list.";
    public override FilterCategory FilterCategory { get; set; } = FilterCategory.Display;
    public override FilterType AvailableIn { get; set; } = FilterType.CraftFilter;
    public override bool? FilterItem(FilterConfiguration configuration, InventoryItem item)
    {
        return null;
    }

    public override bool? FilterItem(FilterConfiguration configuration, ItemRow item)
    {
        return null;
    }

    public override Vector4? CurrentValue(FilterConfiguration configuration)
    {
        return configuration.CraftHeaderColour;
    }

    public override void UpdateFilterConfiguration(FilterConfiguration configuration, Vector4? newValue)
    {
        if (newValue != null)
        {
            configuration.CraftHeaderColour = newValue.Value;
        }
    }

    public CraftHeaderColourFilter(ILogger<CraftHeaderColourFilter> logger, ImGuiService imGuiService) : base(logger, imGuiService)
    {
    }
}