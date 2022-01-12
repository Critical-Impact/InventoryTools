using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using CriticalCommonLib.Models;
using ImGuiNET;
using InventoryTools.Extensions;

namespace InventoryTools.Logic
{
    public class ItemLevelColumn : IColumn
    {
        public string Name { get; set; } = "Item Level";
        public float Width { get; set; } = 80.0f;
        public string FilterText { get; set; } = "";
        
        public IEnumerable<InventoryItem> Filter(IEnumerable<InventoryItem> items)
        {
            return FilterText == "" ? items : items.Where(c => ((int)c.Item.LevelEquip).PassesFilter(FilterText));
        }

        public IEnumerable<SortingResult> Filter(IEnumerable<SortingResult> items)
        {
            return FilterText == "" ? items : items.Where(c => ((int)c.InventoryItem.Item.LevelEquip).PassesFilter(FilterText.ToLower()));
        }

        public IEnumerable<InventoryItem> Sort(ImGuiSortDirection direction, IEnumerable<InventoryItem> items)
        {
            return direction == ImGuiSortDirection.Ascending ? items.OrderBy(c => c.Item.LevelEquip.ToString()) : items.OrderByDescending(c => c.Item.LevelEquip.ToString());
        }

        public IEnumerable<SortingResult> Sort(ImGuiSortDirection direction, IEnumerable<SortingResult> items)
        {
            return direction == ImGuiSortDirection.Ascending ? items.OrderBy(c => c.InventoryItem.Item.LevelEquip.ToString()) : items.OrderByDescending(c => c.InventoryItem.Item.LevelEquip.ToString());
        }

        public void Draw(InventoryItem item)
        {
            ImGui.TableNextColumn();
            ImGui.Text(item.Item.LevelEquip.ToString());
        }

        public void Draw(SortingResult item, int rowIndex)
        {
            ImGui.TableNextColumn();
            ImGui.Text(item.InventoryItem.Item.LevelEquip.ToString());
        }

        public void Setup(int columnIndex)
        {
            ImGui.TableSetupColumn(Name, ImGuiTableColumnFlags.WidthFixed, Width,(uint)columnIndex);
        }
    }
}