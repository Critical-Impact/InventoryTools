using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns;

public class IsMobDropColumn : CheckboxColumn
{
    public IsMobDropColumn(ILogger<IsMobDropColumn> logger, ImGuiService imGuiService) : base(logger, imGuiService)
    {
    }
    public override ColumnCategory ColumnCategory { get; } = ColumnCategory.Basic;
    public override bool HasFilter { get; set; } = true;
    public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Boolean;
    public override bool? CurrentValue(ColumnConfiguration columnConfiguration, SearchResult searchResult)
    {
        return searchResult.Item.HasMobDrops();
    }
    public override string Name { get; set; } = "Is Dropped by Mobs?";
    public override float Width { get; set; } = 100;
    public override string HelpText { get; set; } = "Is this item dropped by mobs?";
}