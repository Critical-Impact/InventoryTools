using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns;

public class VentureTypeColumn : TextColumn
{
    public VentureTypeColumn(ILogger<VentureTypeColumn> logger, ImGuiService imGuiService) : base(logger, imGuiService)
    {
    }
    public override ColumnCategory ColumnCategory => ColumnCategory.Basic;

    public override string? CurrentValue(ColumnConfiguration columnConfiguration, SearchResult searchResult)
    {
        return searchResult.Item.RetainerTaskNames;
    }
    public override string Name { get; set; } = "Venture Type";
    public override float Width { get; set; } = 100;
    public override string HelpText { get; set; } = "The type of ventures that the item can be acquired from";
    public override bool HasFilter { get; set; } = true;
    public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
}