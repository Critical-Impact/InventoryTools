using System.Collections.Generic;
using System.Linq;
using CriticalCommonLib.Models;
using ImGuiNET;
using InventoryTools.Extensions;

namespace InventoryTools.Logic
{
    public class SourceColumn : IColumn
    {
        public string Name { get; set; } = "Source";
        public float Width { get; set; } = 100.0f;
        public string FilterText { get; set; } = "";

        public IEnumerable<InventoryItem> Filter(IEnumerable<InventoryItem> items)
        {
            return FilterText == "" ? items : items.Where(c => PluginLogic.CharacterMonitor.Characters[c.RetainerId]?.Name.ToLower().PassesFilter(FilterText.ToLower()) ?? false);
        }

        public IEnumerable<SortingResult> Filter(IEnumerable<SortingResult> items)
        {
            return FilterText == "" ? items : items.Where(c => PluginLogic.CharacterMonitor.Characters[c.SourceRetainerId]?.Name.ToLower().PassesFilter(FilterText.ToLower()) ?? false);
        }

        public IEnumerable<InventoryItem> Sort(ImGuiSortDirection direction, IEnumerable<InventoryItem> items)
        {
            return direction == ImGuiSortDirection.Ascending ? items.OrderBy(c => PluginLogic.CharacterMonitor.Characters[c.RetainerId]?.Name.ToLower() ?? "") : items.OrderByDescending(c => PluginLogic.CharacterMonitor.Characters[c.RetainerId]?.Name.ToLower() ?? "");
        }

        public IEnumerable<SortingResult> Sort(ImGuiSortDirection direction, IEnumerable<SortingResult> items)
        {
            return direction == ImGuiSortDirection.Ascending ? items.OrderBy(c => PluginLogic.CharacterMonitor.Characters[c.SourceRetainerId]?.Name.ToLower() ?? "") : items.OrderByDescending(c => PluginLogic.CharacterMonitor.Characters[c.SourceRetainerId]?.Name.ToLower() ?? "");
        }

        public void Draw(InventoryItem item)
        {
            ImGui.TableNextColumn();
            ImGui.Text(PluginLogic.CharacterMonitor.Characters[item.RetainerId]?.Name ?? "Unknown");
        }

        public void Draw(SortingResult item)
        {
            ImGui.TableNextColumn();
            if (PluginLogic.CharacterMonitor.Characters.ContainsKey(item.SourceRetainerId))
            {
                ImGui.Text(PluginLogic.CharacterMonitor.Characters[item.SourceRetainerId]?.Name ?? "Unknown");
            }
            else
            {
                ImGui.Text("Unknown");
            }
        }

        public void Setup(int columnIndex)
        {
            ImGui.TableSetupColumn(Name, ImGuiTableColumnFlags.WidthFixed, Width,(uint)columnIndex);
        }
    }
}