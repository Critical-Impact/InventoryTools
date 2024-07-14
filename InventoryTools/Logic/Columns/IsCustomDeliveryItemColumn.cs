using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns;

public class IsCustomDeliveryItemColumn : CheckboxColumn
{
    private readonly ExcelCache _excelCache;

    public IsCustomDeliveryItemColumn(ILogger<IsCustomDeliveryItemColumn> logger, ImGuiService imGuiService, ExcelCache excelCache) : base(logger, imGuiService)
    {
        _excelCache = excelCache;
    }

    public override string Name { get; set; } = "Is custom delivery item?";
    public override string? RenderName { get; } = "Custom delivery item?";
    public override float Width { get; set; } = 80;
    public override string HelpText { get; set; } = "Is this item used for custom deliveries?";
    public override ColumnCategory ColumnCategory { get; } = ColumnCategory.Basic;
    public override bool HasFilter { get; set; } = true;
    public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Boolean;

    public override bool? CurrentValue(ColumnConfiguration columnConfiguration, InventoryItem item)
    {
        return CurrentValue(columnConfiguration, item.Item);
    }

    public override bool? CurrentValue(ColumnConfiguration columnConfiguration, ItemEx item)
    {
        return item.HandInGrandCompanySupply;
    }

    public override bool? CurrentValue(ColumnConfiguration columnConfiguration, SortingResult item)
    {
        return CurrentValue(columnConfiguration, item.Item);
    }
}