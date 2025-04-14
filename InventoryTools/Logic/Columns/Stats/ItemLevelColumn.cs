using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns.Stats
{
    public class ItemLevelColumn : IntegerColumn
    {
        public ItemLevelColumn(ILogger<ItemLevelColumn> logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }
        public override ColumnCategory ColumnCategory => ColumnCategory.Stats;
        public override int? CurrentValue(ColumnConfiguration columnConfiguration, SearchResult searchResult)
        {
            return searchResult.Item.Base.LevelEquip;
        }
        public override string Name { get; set; } = "Required Level";
        public override float Width { get; set; } = 80.0f;
        public override string HelpText { get; set; } = "The required level to equip the item.";
        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
        public override FilterType DefaultIn => Logic.FilterType.GameItemFilter;
    }
}