using System;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Models;
using Dalamud.Interface.Colors;
using ImGuiNET;
using InventoryTools.Logic.Columns.Abstract;
using Lumina.Excel.GeneratedSheets;

namespace InventoryTools.Logic.Columns
{
    public class CraftAmountReadyColumn : IntegerColumn
    {
        public override int? CurrentValue(InventoryItem item)
        {
            return 0;
        }

        public override int? CurrentValue(Item item)
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
        
        public override void Draw(CraftItem item, int rowIndex, FilterConfiguration configuration)
        {
            if (item.QuantityReady < item.QuantityNeeded)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudRed);
            }
            else if(item.QuantityReady >= item.QuantityNeeded)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.HealerGreen);
            }

            base.Draw(item, rowIndex, configuration);
            if (item.QuantityReady < item.QuantityNeeded)
            {
                ImGui.PopStyleColor();
            }
            else if(item.QuantityReady >= item.QuantityNeeded)
            {
                ImGui.PopStyleColor();
            }
        }

        public override string Name { get; set; } = "Inventory";
        public override float Width { get; set; } = 60;
        public override string FilterText { get; set; } = "This is the amount available within your filtered inventories available to complete the craft.";
        public override bool HasFilter { get; set; } = false;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
    }
}