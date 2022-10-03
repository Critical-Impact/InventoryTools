using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Columns.Abstract;

namespace InventoryTools.Logic.Columns
{
    public class AcquiredColumn : CheckboxColumn
    {
        public override bool? CurrentValue(InventoryItem item)
        {
            return CurrentValue(item.Item);
        }

        public override bool? CurrentValue(ItemEx item)
        {
            var action = item.ItemAction?.Value;
            if (!ActionTypeExt.IsValidAction(action)) {
                return null;
            }
            return PluginService.GameInterface.HasAcquired(item);
        }

        public override bool? CurrentValue(SortingResult item)
        {
            return CurrentValue(item.InventoryItem.Item);
        }

        public override string Name { get; set; } = "Acquired";
        public override float Width { get; set; } = 125.0f;

        public override string HelpText { get; set; } =
            "If a item can be acquired(mounts, minions, etc) this shows whether or not it has been.";
        
        public override string FilterText { get; set; } = "";
        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Boolean;
    }
}