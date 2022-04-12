using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using Lumina.Excel.GeneratedSheets;

namespace InventoryTools.Logic.Columns
{
    public class AcquiredColumn : CheckboxColumn
    {
        public override bool? CurrentValue(InventoryItem item)
        {
            if (item.Item == null)
            {
                return null;
            }
            return CurrentValue(item.Item);
        }

        public override bool? CurrentValue(Item item)
        {
            var action = item.ItemAction?.Value;
            if (!ActionTypeExt.IsValidAction(action)) {
                return null;
            }
            return GameInterface.HasAcquired(item);
        }

        public override bool? CurrentValue(SortingResult item)
        {
            if (item.InventoryItem.Item == null)
            {
                return null;
            }
            return  CurrentValue(item.InventoryItem.Item);
        }
        

        public override string Name { get; set; } = "Acquired";
        public override float Width { get; set; } = 125.0f;
        public override string FilterText { get; set; } = "";
        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Boolean;
        public override event IColumn.ButtonPressedDelegate? ButtonPressed;
    }
}