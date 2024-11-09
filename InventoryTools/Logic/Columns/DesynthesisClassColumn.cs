using AllaganLib.GameSheets.Extensions;
using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns;

public class DesynthesisClassColumn : TextColumn
{
    public DesynthesisClassColumn(ILogger<DesynthesisClassColumn> logger, ImGuiService imGuiService) : base(logger, imGuiService)
    {
    }
    public override ColumnCategory ColumnCategory => ColumnCategory.Basic;
    public override string? CurrentValue(ColumnConfiguration columnConfiguration, SearchResult searchResult)
    {
        if (!searchResult.Item.CanBeDesynthed || searchResult.Item.Base.ClassJobRepair.RowId == 0)
        {
            return null;
        }

        return searchResult.Item.Base.ClassJobRepair.ValueNullable?.Name.ToString().ToTitleCase() ?? "Unknown";
    }
    public override string Name { get; set; } = "Desynthesis Class";
    public override string RenderName  => "Desynth Class";
    public override float Width { get; set; } = 100;
    public override string HelpText { get; set; } = "What class is related to de-synthesising this item?";
    public override bool HasFilter { get; set; } = true;
    public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
}