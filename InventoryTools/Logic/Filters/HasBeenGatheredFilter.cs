using AllaganLib.GameSheets.Sheets.Rows;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;

using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters;

public class HasBeenGatheredFilter : BooleanFilter
{
    private readonly IGameInterface _gameInterface;

    public HasBeenGatheredFilter(ILogger<HasBeenGatheredFilter> logger, ImGuiService imGuiService, IGameInterface gameInterface) : base(logger, imGuiService)
    {
        _gameInterface = gameInterface;
    }
    public override string Key { get; set; } = "HasBeenGathered";
    public override string Name { get; set; } = "Has been gathered before?";
    public override string HelpText { get; set; } = "Has this gathering item been gathered at least once by the currently logged in character? This only supports mining and botany at present.";
    public override FilterCategory FilterCategory { get; set; } = FilterCategory.Gathering;

    public override bool? FilterItem(FilterConfiguration configuration, InventoryItem item)
    {
        return FilterItem(configuration, item.Item);
    }

    public override bool? FilterItem(FilterConfiguration configuration, ItemRow item)
    {
        var currentValue = CurrentValue(configuration);
        if (currentValue == null) return true;

        var isItemGathered = _gameInterface.IsItemGathered(item.RowId);
        if (isItemGathered == null)
        {
            return null;
        }
        if(currentValue.Value && isItemGathered.Value)
        {
            return true;
        }
                
        return !currentValue.Value && !isItemGathered.Value;
    }
}