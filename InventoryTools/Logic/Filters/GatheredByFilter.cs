using System.Collections.Generic;
using System.Linq;
using AllaganLib.GameSheets.Sheets.Rows;
using CriticalCommonLib.Models;
using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters;

public class GatheredByFilter : UintMultipleChoiceFilter
{
    private readonly ExcelSheet<GatheringType> _gatheringTypeSheet;

    public GatheredByFilter(ILogger<GatheredByFilter> logger, ImGuiService imGuiService, ExcelSheet<GatheringType> gatheringTypeSheet) : base(logger, imGuiService)
    {
        _gatheringTypeSheet = gatheringTypeSheet;
    }

    public override string Key { get; set; } = "GatheredByFilter";
    public override string Name { get; set; } = "Gathered By?";
    public override string HelpText { get; set; } = "How is this item gathered?";
    public override FilterCategory FilterCategory { get; set; } = FilterCategory.Gathering;

    public override List<uint> DefaultValue { get; set; } = new();

    public override bool? FilterItem(FilterConfiguration configuration, InventoryItem item)
    {
        return FilterItem(configuration, item.Item);
    }

    public override bool? FilterItem(FilterConfiguration configuration, ItemRow item)
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

        return item.GatheringTypes.Select(c => c.RowId)
            .Any(c => currentValue.Contains(c));
    }

    public override Dictionary<uint, string> GetChoices(FilterConfiguration configuration)
    {
        var dictionary = _gatheringTypeSheet.Where(c => c.RowId < 4).ToDictionary(c => c.RowId, c => c.Name.ExtractText());
        dictionary.Add(10, "Fishing");
        return dictionary;
    }

    public override bool HideAlreadyPicked { get; set; } = true;
}