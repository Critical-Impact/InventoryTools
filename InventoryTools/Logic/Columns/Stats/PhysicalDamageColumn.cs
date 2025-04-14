using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns.Stats;

public class PhysicalDamageColumn : IntegerColumn
{
    public PhysicalDamageColumn(ILogger<PhysicalDamageColumn> logger, ImGuiService imGuiService) : base(logger, imGuiService)
    {
    }

    public override ColumnCategory ColumnCategory => ColumnCategory.Stats;
    public override bool HasFilter { get; set; } = true;
    public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;

    public override int? CurrentValue(ColumnConfiguration columnConfiguration, SearchResult searchResult)
    {
        if (searchResult.Item.Base.DamagePhys == 0)
        {
            return null;
        }
        return searchResult.Item.Base.DamagePhys;
    }

    public override string Name { get; set; } = "Physical Damage";
    public override float Width { get; set; } = 80;
    public override string HelpText { get; set; } = "The physical damage of the item";
}