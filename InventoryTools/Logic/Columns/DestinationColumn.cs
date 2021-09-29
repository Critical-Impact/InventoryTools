using System.Collections.Generic;
using System.Linq;
using CriticalCommonLib.Models;
using ImGuiNET;
using InventoryTools.Extensions;

namespace InventoryTools.Logic
{
    public class DestinationColumn : IColumn
    {
        public string Name { get; set; } = "Destination";
        public float Width { get; set; } = 100.0f;
        public string FilterText { get; set; } = "";

        public IEnumerable<InventoryItem> Filter(IEnumerable<InventoryItem> items)
        {
            return items;
        }

        public IEnumerable<SortingResult> Filter(IEnumerable<SortingResult> items)
        {
            return FilterText == "" ? items : items.Where(c => c.DestinationRetainerId.HasValue && PluginLogic.CharacterMonitor.Characters.ContainsKey(c.DestinationRetainerId.Value) && PluginLogic.CharacterMonitor.Characters[c.DestinationRetainerId.Value].Name.ToLower().PassesFilter(FilterText.ToLower()));
        }

        public IEnumerable<InventoryItem> Sort(ImGuiSortDirection direction, IEnumerable<InventoryItem> items)
        {
            return items;
        }

        public IEnumerable<SortingResult> Sort(ImGuiSortDirection direction, IEnumerable<SortingResult> items)
        {
            return direction == ImGuiSortDirection.Ascending ? items.OrderBy(c => c.DestinationRetainerId.HasValue ? PluginLogic.CharacterMonitor.Characters[c.DestinationRetainerId.Value]?.Name.ToLower() ?? "" : "") : items.OrderByDescending(c => c.DestinationRetainerId.HasValue ? PluginLogic.CharacterMonitor.Characters[c.DestinationRetainerId.Value]?.Name.ToLower() ?? "" : "");
        }

        public void Draw(InventoryItem item)
        {
            ImGui.TableNextColumn();
            ImGui.Text("N/A");
        }

        public void Draw(SortingResult item)
        {
            ImGui.TableNextColumn();
            ImGui.Text(item.DestinationRetainerId.HasValue ? PluginLogic.CharacterMonitor.Characters[item.DestinationRetainerId.Value]?.Name ?? "" : "Unknown");
        }

        public void Setup(int columnIndex)
        {
            ImGui.TableSetupColumn(Name, ImGuiTableColumnFlags.WidthFixed, Width,(uint)columnIndex);
        }
    }
}