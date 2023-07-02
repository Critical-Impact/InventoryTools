using CriticalCommonLib.Crafting;
using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using Dalamud.Interface.Colors;
using ImGuiNET;
using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Ui.Widgets;

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
            return (int?) (currentValue.CraftOperationsRequired);
        }
        
        public override void Draw(FilterConfiguration configuration, CraftItem item, int rowIndex)
        {
            ImGui.TableNextColumn();
            if (CurrentValue(item) > 0)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.ParsedBlue);
            }

            var currentValue = CurrentValue(item);
            if (currentValue != null)
            {
                var fmt = $"{currentValue.Value:n0}";
                if (item.Yield > 1)
                {
                    fmt += " (" + item.Yield + ")";
                }
                ImGuiUtil.VerticalAlignText(fmt, configuration.TableHeight, false);
            }
            else
            {
                ImGuiUtil.VerticalAlignText(EmptyText, configuration.TableHeight, false);
            }

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