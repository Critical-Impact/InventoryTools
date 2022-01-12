using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using CriticalCommonLib.Models;
using ImGuiNET;
using InventoryTools.Extensions;

namespace InventoryTools.Logic
{
    public class ItemILevelColumn : IColumn
    {
        public string Name { get; set; } = "iLevel";
        public float Width { get; set; } = 50.0f;
        public string FilterText { get; set; } = "";
        
        public IEnumerable<InventoryItem> Filter(IEnumerable<InventoryItem> items)
        {
            return FilterText == "" ? items : items.Where(c => c.EquipSlotCategory.RowId != 0 && ((int)c.Item.LevelItem.Row).PassesFilter(FilterText));
        }

        public IEnumerable<SortingResult> Filter(IEnumerable<SortingResult> items)
        {
            return FilterText == "" ? items : items.Where(c => c.InventoryItem.EquipSlotCategory.RowId != 0 && ((int)c.InventoryItem.Item.LevelItem.Row).PassesFilter(FilterText.ToLower()));
        }

        public IEnumerable<InventoryItem> Sort(ImGuiSortDirection direction, IEnumerable<InventoryItem> items)
        {
            return direction == ImGuiSortDirection.Ascending ? items.OrderBy(c => c.Item.LevelItem.Row) : items.OrderByDescending(c => c.Item.LevelItem.Row);
        }

        public IEnumerable<SortingResult> Sort(ImGuiSortDirection direction, IEnumerable<SortingResult> items)
        {
            return direction == ImGuiSortDirection.Ascending ? items.OrderBy(c => c.InventoryItem.Item.LevelItem.Row) : items.OrderByDescending(c => c.InventoryItem.Item.LevelItem.Row);
        }

        public void Draw(InventoryItem item)
        {
            ImGui.TableNextColumn();
            ImGui.Text(item.EquipSlotCategory.RowId == 0 ? "" : item.Item.LevelItem.Row.ToString());
        }

        public void Draw(SortingResult item, int rowIndex)
        {
            ImGui.TableNextColumn();
            ImGui.Text(item.InventoryItem.EquipSlotCategory.RowId == 0 ? "" : item.InventoryItem.Item.LevelItem.Row.ToString());
        }

        public void Setup(int columnIndex)
        {
            ImGui.TableSetupColumn(Name, ImGuiTableColumnFlags.WidthFixed, Width,(uint)columnIndex);
        }
    }
}