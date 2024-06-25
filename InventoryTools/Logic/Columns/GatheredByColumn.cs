using System.Linq;
using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns;

public class GatheredByColumn : TextColumn
{
    public GatheredByColumn(ILogger<GatheredByColumn> logger, ImGuiService imGuiService) : base(logger, imGuiService)
    {
    }
    public override ColumnCategory ColumnCategory { get; } = ColumnCategory.Basic;
    public override bool HasFilter { get; set; } = true;
    public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;

    public override string? CurrentValue(ColumnConfiguration columnConfiguration, SearchResult searchResult)
    {
        var currentValue = searchResult.Item.GatheringTypes.Select(c => c.Value!.FormattedName).ToList();
        if (searchResult.Item.ObtainedFishing)
        {
            currentValue.Add("Fishing");
        }

        return string.Join(",", currentValue);
    }
    
    public override string Name { get; set; } = "Gathered By?";
    public override float Width { get; set; } = 100;
    public override string HelpText { get; set; } = "How is this item gathered?";
}