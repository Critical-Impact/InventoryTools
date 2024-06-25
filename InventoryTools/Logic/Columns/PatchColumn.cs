using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns;

public class PatchColumn : DecimalColumn
{
    private readonly ExcelCache _excelCache;

    public PatchColumn(ILogger<PatchColumn> logger, ImGuiService imGuiService, ExcelCache excelCache) : base(logger, imGuiService)
    {
        _excelCache = excelCache;
    }
    public override ColumnCategory ColumnCategory => ColumnCategory.Basic;
    public override decimal? CurrentValue(ColumnConfiguration columnConfiguration, SearchResult searchResult)
    {
        return _excelCache.GetItemPatch(searchResult.Item.RowId);
    }
    public override string Name { get; set; } = "Patch Added";
    public override string RenderName => "Patch";
    public override float Width { get; set; } = 100;
    public override string HelpText { get; set; } = "Shows the patch in which the item was added.";
    public override bool HasFilter { get; set; } = true;
    public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
}