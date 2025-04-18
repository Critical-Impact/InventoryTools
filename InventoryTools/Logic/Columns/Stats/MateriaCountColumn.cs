using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns.Stats;

public class MateriaCountColumn : IntegerColumn
{
    public MateriaCountColumn(ILogger<MateriaCountColumn> logger, ImGuiService imGuiService) : base(logger, imGuiService)
    {
    }

    public override string Name { get; set; } = "Materia Count";
    public override float Width { get; set; } = 90;
    public override string HelpText { get; set; } = "How many materia does this item have or can it have?";
    public override ColumnCategory ColumnCategory { get; } = ColumnCategory.Stats;
    public override int? CurrentValue(ColumnConfiguration columnConfiguration, SearchResult searchResult)
    {
        return searchResult.InventoryItem?.MateriaCount ?? searchResult.Item.Base.MateriaSlotCount;
    }

    public override bool HasFilter { get; set; } = true;
    public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
}