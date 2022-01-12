using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using CriticalCommonLib.Models;
using ImGuiNET;
using InventoryTools.Extensions;

namespace InventoryTools.Logic
{
    public class NameColumn : IColumn
    {
        public string Name { get; set; } = "Name";
        public float Width { get; set; } = 250.0f;
        public string FilterText { get; set; } = "";
        
        public IEnumerable<InventoryItem> Filter(IEnumerable<InventoryItem> items)
        {
            return FilterText == "" ? items : items.Where(c => c.FormattedName.ToLower().PassesFilter(FilterText.ToLower()));
        }

        public IEnumerable<SortingResult> Filter(IEnumerable<SortingResult> items)
        {
            return FilterText == "" ? items : items.Where(c => c.InventoryItem.FormattedName.ToLower().PassesFilter(FilterText.ToLower()));
        }

        public IEnumerable<InventoryItem> Sort(ImGuiSortDirection direction, IEnumerable<InventoryItem> items)
        {
            return direction == ImGuiSortDirection.Ascending ? items.OrderBy(c => c.FormattedName.ToLower()) : items.OrderByDescending(c => c.FormattedName.ToLower());
        }

        public IEnumerable<SortingResult> Sort(ImGuiSortDirection direction, IEnumerable<SortingResult> items)
        {
            return direction == ImGuiSortDirection.Ascending ? items.OrderBy(c => c.InventoryItem.FormattedName.ToLower()) : items.OrderByDescending(c => c.InventoryItem.FormattedName.ToLower());
        }

        public void Draw(InventoryItem item)
        {
            ImGui.TableNextColumn();
            ImGui.TextColored(item.ItemColour, item.FormattedName);
        }

        public void Draw(SortingResult item, int rowIndex)
        {
            ImGui.TableNextColumn();
            ImGui.TextColored(item.InventoryItem.ItemColour, item.InventoryItem.FormattedName);
        }

        public void Setup(int columnIndex)
        {
            ImGui.TableSetupColumn(Name, ImGuiTableColumnFlags.WidthFixed, Width,(uint)columnIndex);
        }
    }
}