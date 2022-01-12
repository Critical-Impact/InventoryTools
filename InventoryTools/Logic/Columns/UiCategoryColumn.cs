using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using CriticalCommonLib.Models;
using ImGuiNET;
using InventoryTools.Extensions;

namespace InventoryTools.Logic
{
    public class UiCategoryColumn : IColumn
    {
        public string Name { get; set; } = "Category";
        public float Width { get; set; } = 200.0f;
        public string FilterText { get; set; } = "";
        
        public IEnumerable<InventoryItem> Filter(IEnumerable<InventoryItem> items)
        {
            return FilterText == "" ? items : items.Where(c => c.ItemUICategory != null && c.FormattedUiCategory.ToLower().PassesFilter(FilterText.ToLower()));
        }

        public IEnumerable<SortingResult> Filter(IEnumerable<SortingResult> items)
        {
            return FilterText == "" ? items : items.Where(c => c.InventoryItem.ItemUICategory != null && c.InventoryItem.FormattedUiCategory.ToLower().PassesFilter(FilterText.ToLower()));
        }

        public IEnumerable<InventoryItem> Sort(ImGuiSortDirection direction, IEnumerable<InventoryItem> items)
        {
            return direction == ImGuiSortDirection.Ascending ? items.OrderBy(c => c.ItemUICategory != null ? c.ItemUICategory.Name : "") : items.OrderByDescending(c => c.ItemUICategory != null ? c.ItemUICategory.Name : "");
        }

        public IEnumerable<SortingResult> Sort(ImGuiSortDirection direction, IEnumerable<SortingResult> items)
        {
            return direction == ImGuiSortDirection.Ascending ? items.OrderBy(c => c.InventoryItem.ItemUICategory != null ? c.InventoryItem.ItemUICategory.Name : "") : items.OrderByDescending(c => c.InventoryItem.ItemUICategory != null ? c.InventoryItem.ItemUICategory.Name : "");
        }

        public void Draw(InventoryItem item)
        {
            ImGui.TableNextColumn();
            ImGui.Text(item.FormattedUiCategory);
        }

        public void Draw(SortingResult item, int rowIndex)
        {
            ImGui.TableNextColumn();
            ImGui.Text(item.InventoryItem.FormattedUiCategory);
        }

        public void Setup(int columnIndex)
        {
            ImGui.TableSetupColumn(Name, ImGuiTableColumnFlags.WidthFixed, Width,(uint)columnIndex);
        }
    }
}