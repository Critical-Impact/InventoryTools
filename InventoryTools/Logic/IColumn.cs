using System.Collections.Generic;
using System.Numerics;
using CriticalCommonLib.Models;
using ImGuiNET;

namespace InventoryTools.Logic
{
    public interface IColumn
    {
        public string Name { get; set; }
        public float Width { get; set; }
        
        public string FilterText { get; set; }
        
        public IEnumerable<InventoryItem> Filter(IEnumerable<InventoryItem> items);
        public IEnumerable<SortingResult> Filter(IEnumerable<SortingResult> items);
        
        public IEnumerable<InventoryItem> Sort(ImGuiSortDirection direction, IEnumerable<InventoryItem> items);
        public IEnumerable<SortingResult> Sort(ImGuiSortDirection direction, IEnumerable<SortingResult> items);
        
        public void Draw(InventoryItem item);
        public void Draw(SortingResult item);

        public void Setup(int columnIndex);
        
        public void SetupFilter(string tableKey)
        {
            ImGui.TableSetupColumn(tableKey + "Filter" + Name, ImGuiTableColumnFlags.NoSort);
        }

        public bool DrawFilter(string tableKey, int columnIndex)
        {
            var filter = FilterText;
            var hasChanged = false;
            ImGui.TableSetColumnIndex(columnIndex);
            ImGui.PushItemWidth(-20.000000f);
            ImGui.PushID(Name);
            ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(0, 0));
            ImGui.InputText("##" + tableKey + "FilterI" + Name, ref filter, 200);
            ImGui.PopStyleVar();
            ImGui.SameLine(0.0f, ImGui.GetStyle().ItemInnerSpacing.X);
            ImGui.TableHeader("");
            ImGui.PopID();
            if (filter != FilterText)
            {
                FilterText = filter;
                hasChanged = true;
            }

            return hasChanged;
        }
    }
}