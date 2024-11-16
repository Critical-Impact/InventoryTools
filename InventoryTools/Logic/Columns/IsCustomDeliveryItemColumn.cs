using AllaganLib.GameSheets.Caches;
using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns;

public class IsCustomDeliveryItemColumn : CheckboxColumn
{
    public IsCustomDeliveryItemColumn(ILogger<IsCustomDeliveryItemColumn> logger, ImGuiService imGuiService) : base(logger, imGuiService)
    {
    }

    public override string Name { get; set; } = "Is custom delivery item?";
    public override string? RenderName { get; } = "Custom delivery item?";
    public override float Width { get; set; } = 80;
    public override string HelpText { get; set; } = "Is this item used for custom deliveries?";
    public override ColumnCategory ColumnCategory { get; } = ColumnCategory.Basic;
    public override bool HasFilter { get; set; } = true;
    public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Boolean;

    public override bool? CurrentValue(ColumnConfiguration columnConfiguration, SearchResult searchResult)
    {
        return searchResult.Item.HasSourcesByType(ItemInfoType.CustomDelivery);
    }
}