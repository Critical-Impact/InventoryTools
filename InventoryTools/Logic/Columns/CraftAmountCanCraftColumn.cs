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
        public override ColumnCategory ColumnCategory => ColumnCategory.Crafting;
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
            return (int?) (currentValue.QuantityCanCraft / currentValue.Yield);
        }
        
        public override void Draw(FilterConfiguration configuration, CraftItem item, int rowIndex)
        {
            if (CurrentValue(item) > 0)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.ParsedBlue);
            }

            base.Draw(configuration, item, rowIndex);
            if (CurrentValue(item) > 0)
            {
                ImGui.PopStyleColor();
            }
        }

        public override string Name { get; set; } = "Amount can Craft";
        public override string RenderName => "Craftable";
        public override float Width { get; set; } = 60;
        public override bool? CraftOnly => true;
        public override string HelpText { get; set; } =
            "This is the amount that you could craft given the items in your inventory";
        public override bool HasFilter { get; set; } = false;
        public override FilterType AvailableIn { get; } = Logic.FilterType.CraftFilter;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
    }
}