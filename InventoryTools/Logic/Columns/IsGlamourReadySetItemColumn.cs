using AllaganLib.GameSheets.Caches;
using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns;

public class IsGlamourReadySetItemColumn : CheckboxColumn
{
    public IsGlamourReadySetItemColumn(ILogger<IsGlamourReadySetItemColumn> logger, ImGuiService imGuiService) : base(logger, imGuiService)
    {
    }

    public override ColumnCategory ColumnCategory => ColumnCategory.Basic;
    public override bool HasFilter { get; set; } = true;
    public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Boolean;

    public override bool? CurrentValue(ColumnConfiguration columnConfiguration, SearchResult searchResult)
    {
        return searchResult.Item.HasUsesByType(ItemInfoType.GlamourReadySetItem);
    }

    public override string Name { get; set; } = "Is Glamour Ready Set Item?";
    public override float Width { get; set; } = 100;
    public override string HelpText { get; set; } = "Is this item part of a glamour ready item?";
}