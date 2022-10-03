using System;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using Dalamud.Interface.Colors;
using ImGuiNET;
using InventoryTools.Logic.Columns.Abstract;

namespace InventoryTools.Logic.Columns
{
    public class CraftAmountAvailableColumn : IntegerColumn
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
            return item.Quantity;
        }

        public override int? CurrentValue(CraftItem currentValue)
        {
            if (currentValue.IsOutputItem)
            {
                return 0;
            }
            return Math.Min((int)currentValue.QuantityAvailable, (int)currentValue.QuantityNeeded);
        }

        public override void Draw(FilterConfiguration configuration, CraftItem item, int rowIndex)
        {
            if (item.IsOutputItem)
            {
                ImGui.TableNextColumn();
                return;
            }
            if (item.QuantityAvailable != 0)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.ParsedBlue);
            }

            base.Draw(configuration, item, rowIndex);

            if (item.QuantityAvailable != 0)
            {
                ImGui.PopStyleColor();
            }
        }

        public override string Name { get; set; } = "Retrieve";
        public override float Width { get; set; } = 60;
        public override bool? CraftOnly => false;

        public override string HelpText { get; set; } =
            "This is the amount to retrieve from retainers.";
        public override FilterType AvailableIn { get; } = Logic.FilterType.CraftFilter | Logic.FilterType.SortingFilter;
        public override string FilterText { get; set; } = "";
        public override bool HasFilter { get; set; } = false;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
    }
}