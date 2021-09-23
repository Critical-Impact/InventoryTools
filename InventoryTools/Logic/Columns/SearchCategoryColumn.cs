using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using CriticalCommonLib.Models;
using ImGuiNET;
using InventoryTools.Extensions;

namespace InventoryTools.Logic
{
    public class SearchCategoryColumn : IColumn
    {
        public string Name { get; set; } = "MB Category";
        public float Width { get; set; } = 200.0f;
        public string FilterText { get; set; } = "";
        
        public IEnumerable<InventoryItem> Filter(IEnumerable<InventoryItem> items)
        {
            return FilterText == "" ? items : items.Where(c => c.ItemSearchCategory != null && c.FormattedSearchCategory.ToLower().PassesFilter(FilterText.ToLower()));
        }

        public IEnumerable<SortingResult> Filter(IEnumerable<SortingResult> items)
        {
            return FilterText == "" ? items : items.Where(c => c.InventoryItem.ItemSearchCategory != null && c.InventoryItem.FormattedSearchCategory.ToLower().PassesFilter(FilterText.ToLower()));
        }

        public IEnumerable<InventoryItem> Sort(ImGuiSortDirection direction, IEnumerable<InventoryItem> items)
        {
            return direction == ImGuiSortDirection.Ascending ? items.OrderBy(c => c.ItemSearchCategory != null ? c.ItemSearchCategory.Name : "") : items.OrderByDescending(c => c.ItemSearchCategory != null ? c.ItemSearchCategory.Name : "");
        }

        public IEnumerable<SortingResult> Sort(ImGuiSortDirection direction, IEnumerable<SortingResult> items)
        {
            return direction == ImGuiSortDirection.Ascending ? items.OrderBy(c => c.InventoryItem.ItemSearchCategory != null ? c.InventoryItem.ItemSearchCategory.Name : "") : items.OrderByDescending(c => c.InventoryItem.ItemSearchCategory != null ? c.InventoryItem.ItemSearchCategory.Name : "");
        }

        public void Draw(InventoryItem item)
        {
            ImGui.TableNextColumn();
            ImGui.Text(item.FormattedSearchCategory);
        }

        public void Draw(SortingResult item)
        {
            ImGui.TableNextColumn();
            ImGui.Text(item.InventoryItem.FormattedSearchCategory);
        }

        public void Setup(int columnIndex)
        {
            ImGui.TableSetupColumn(Name, ImGuiTableColumnFlags.WidthFixed, Width,(uint)columnIndex);
        }
    }
}