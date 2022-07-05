using CriticalCommonLib.Crafting;
using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using Dalamud.Interface.Colors;
using ImGuiNET;
using InventoryTools.Logic.Columns.Abstract;

namespace InventoryTools.Logic.Columns
{
    public class CraftAmountCanCraftColumn : IntegerColumn
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
            return (int?) currentValue.QuantityCanCraft;
        }
        
        public override void Draw(CraftItem item, int rowIndex, FilterConfiguration configuration)
        {
            if (CurrentValue(item) > 0)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.ParsedBlue);
            }

            base.Draw(item, rowIndex, configuration);
            if (CurrentValue(item) > 0)
            {
                ImGui.PopStyleColor();
            }
        }

        public override string Name { get; set; } = "Craftable";
        public override float Width { get; set; } = 60;
        public override string FilterText { get; set; } = "This is the amount that you could craft given the items in your inventory.";
        public override bool HasFilter { get; set; } = false;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
    }
}