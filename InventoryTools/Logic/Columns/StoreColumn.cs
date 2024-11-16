using AllaganLib.GameSheets.Caches;
using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns;

public class StoreColumn : CheckboxColumn
{
    public StoreColumn(ILogger<StoreColumn> logger, ImGuiService imGuiService) : base(logger, imGuiService)
    {
    }
    public override ColumnCategory ColumnCategory => ColumnCategory.Basic;
    public override bool? CurrentValue(ColumnConfiguration columnConfiguration, SearchResult searchResult)
    {
        return searchResult.Item.HasSourcesByType(ItemInfoType.CashShop);
    }
    public override string Name { get; set; } = "Is sold in Square Store?";
    public override string RenderName => "Is Square Store Item?";
    public override float Width { get; set; } = 80;
    public override string HelpText { get; set; } = "Is this item sold in the square store?";
    public override bool HasFilter { get; set; } = true;
    public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Boolean;
}