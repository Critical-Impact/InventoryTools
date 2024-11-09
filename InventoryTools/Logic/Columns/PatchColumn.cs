using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns;

public class PatchColumn : DecimalColumn
{
    public PatchColumn(ILogger<PatchColumn> logger, ImGuiService imGuiService) : base(logger, imGuiService)
    {
    }
    public override ColumnCategory ColumnCategory => ColumnCategory.Basic;
    public override decimal? CurrentValue(ColumnConfiguration columnConfiguration, SearchResult searchResult)
    {
        return searchResult.Item.Patch;
    }
    public override string Name { get; set; } = "Patch Added";
    public override string RenderName => "Patch";
    public override float Width { get; set; } = 100;
    public override string HelpText { get; set; } = "Shows the patch in which the item was added.";
    public override bool HasFilter { get; set; } = true;
    public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
}