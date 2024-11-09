using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns
{
    public class ItemILevelColumn : IntegerColumn
    {
        public ItemILevelColumn(ILogger<ItemILevelColumn> logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }
        public override ColumnCategory ColumnCategory => ColumnCategory.Basic;
        public override int? CurrentValue(ColumnConfiguration columnConfiguration, SearchResult searchResult)
        {
            if ((int)searchResult.Item.Base.LevelItem.RowId == 0)
            {
                return null;
            }

            return (int)searchResult.Item.Base.LevelItem.RowId;
        }
        public override string Name { get; set; } = "iLevel";
        public override float Width { get; set; } = 50.0f;
        public override string HelpText { get; set; } = "Shows the iLevel of the item.";
        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
        public override FilterType DefaultIn => Logic.FilterType.GameItemFilter;
    }
}