using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Misc;

namespace InventoryTools.Logic.Columns
{
    public class IsHousingItemColumn : CheckboxColumn
    {
        public override ColumnCategory ColumnCategory => ColumnCategory.Basic;
        public override bool? CurrentValue(InventoryItem item)
        {
            return item.Item == null ? false : CurrentValue(item.Item);
        }

        public override bool? CurrentValue(ItemEx item)
        {
            return Helpers.HousingCategoryIds.Contains(item.ItemUICategory.Row);
        }

        public override bool? CurrentValue(SortingResult item)
        {
            return CurrentValue(item.InventoryItem);
        }

        public override string Name { get; set; } = "Is Housing Item?";
        public override string RenderName => "Is Housing?";
        public override float Width { get; set; } = 100;
        public override string HelpText { get; set; } = "Is this item a housing item? This might be slightly inaccurate for the time being.";
        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
    }
}