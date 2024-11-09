using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns
{
    public class ItemIdColumn : IntegerColumn
    {
        public ItemIdColumn(ILogger<ItemIdColumn> logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }
        public override ColumnCategory ColumnCategory => ColumnCategory.Basic;
        public override int? CurrentValue(ColumnConfiguration columnConfiguration, SearchResult searchResult)
        {
            return (int)searchResult.Item.RowId;
        }
        public override string Name { get; set; } = "Item ID";
        public override float Width { get; set; } = 100.0f;
        public override string HelpText { get; set; } = "Shows the item's internal ID.";
        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
    }
}