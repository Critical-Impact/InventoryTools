using CriticalCommonLib.Crafting;
using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using Dalamud.Interface.Colors;
using ImGuiNET;
using InventoryTools.Logic.Columns.Abstract;

namespace InventoryTools.Logic.Columns
{
    public class CraftAmountReadyColumn : IntegerColumn
    {
        public override int? CurrentValue(InventoryItem item)
        {
            return 0;
        }

        public override int? CurrentValue(ItemEx item)
        {
            return 0;
        }

        public override int? CurrentValue(SortingResult item)
        {
            return 0;
        }

        public override int? CurrentValue(CraftItem currentValue)
        {
            if (currentValue.IsOutputItem)
            {
                return 0;
            }
            return (int?) currentValue.QuantityReady;
        }
        
        public override void Draw(FilterConfiguration configuration, CraftItem item, int rowIndex)
        {
            if (item.IsOutputItem)
            {
                ImGui.TableNextColumn();
                return;
            }
            if(item.QuantityReady >= item.QuantityNeeded)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.HealerGreen);
            }

            base.Draw(configuration, item, rowIndex);
            if(item.QuantityReady >= item.QuantityNeeded)
            {
                ImGui.PopStyleColor();
            }
        }

        public override string Name { get; set; } = "Inventory";
        public override float Width { get; set; } = 60;
        public override bool? CraftOnly => true;
        public override FilterType AvailableIn { get; } = Logic.FilterType.CraftFilter;
        public override string HelpText { get; set; } =
            "This is the amount available within your filtered inventories available to complete the craft.";
        public override string FilterText { get; set; } = "";
        public override bool HasFilter { get; set; } = false;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
    }
}