using CriticalCommonLib.Crafting;
using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using ImGuiNET;
using InventoryTools.Logic.Columns.Abstract;

namespace InventoryTools.Logic.Columns
{
    public class DebugCraftColumn : TextColumn
    {
        public override ColumnCategory ColumnCategory => ColumnCategory.Debug;
        public override string? CurrentValue(InventoryItem item)
        {
            return CurrentValue(item.Item);
        }

        public override string? CurrentValue(ItemEx item)
        {
            return "";
        }

        public override string? CurrentValue(SortingResult item)
        {
            return CurrentValue(item.InventoryItem);
        }

        public override void Draw(FilterConfiguration configuration, CraftItem item, int rowIndex)
        {
            ImGui.TableNextColumn();
            ImGui.Text("Required: " +  item.QuantityRequired);
            ImGui.Text("Needed: " +  item.QuantityNeeded);
            ImGui.Text("Needed Pre Update: " +  item.QuantityNeededPreUpdate);
            ImGui.Text("Available: " +  item.QuantityAvailable);
            ImGui.Text("Ready: " +  item.QuantityReady);
            ImGui.Text("Can Craft: " +  item.QuantityCanCraft);
            ImGui.Text("Will Retrieve: " + item.QuantityWillRetrieve);
        }

        public override string Name { get; set; } = "Debug - Craft";
        public override float Width { get; set; } = 200;
        public override string HelpText { get; set; } = "Shows craft debug information";
        public override bool HasFilter { get; set; } = true;
        public override bool IsDebug { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
    }
}