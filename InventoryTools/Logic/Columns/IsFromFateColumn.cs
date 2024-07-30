using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns;

public class IsFromFateColumn : CheckboxColumn
{
    public IsFromFateColumn(ILogger<IsFromFateColumn> logger, ImGuiService imGuiService) : base(logger, imGuiService)
    {
        
    }
    
    public override string Name { get; set; } = "Is From Fate?";
    public override float Width { get; set; } = 80;
    public override string HelpText { get; set; } = "Is this item dropped/acquired in a fate?";
    public override ColumnCategory ColumnCategory { get; } = ColumnCategory.Basic;
    public override bool HasFilter { get; set; } = true;
    public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Boolean;

    public override bool? CurrentValue(ColumnConfiguration columnConfiguration, SearchResult searchResult)
    {
        return searchResult.Item.FateItems.Count != 0;
    }
}