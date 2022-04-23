using System.Collections.Generic;
using CriticalCommonLib.Models;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;

namespace InventoryTools.Logic.Columns
{
    public interface IColumn
    {
        public string Name { get; set; }
        public float Width { get; set; }
        
        public string FilterText { get; set; }
        
        public bool HasFilter { get; set; }
        
        public ColumnFilterType FilterType { get; set; }
        
        public bool IsDebug { get; set; }
        
        public IEnumerable<InventoryItem> Filter(IEnumerable<InventoryItem> items);
        public IEnumerable<SortingResult> Filter(IEnumerable<SortingResult> items);
        public IEnumerable<Item> Filter(IEnumerable<Item> items);
        
        public IEnumerable<InventoryItem> Sort(ImGuiSortDirection direction, IEnumerable<InventoryItem> items);
        public IEnumerable<SortingResult> Sort(ImGuiSortDirection direction, IEnumerable<SortingResult> items);
        public IEnumerable<Item> Sort(ImGuiSortDirection direction, IEnumerable<Item> items);
        
        public void Draw(InventoryItem item, int rowIndex);
        public void Draw(SortingResult item, int rowIndex);
        public void Draw(Item item, int rowIndex);

        public void Setup(int columnIndex);
        
        public void SetupFilter(string tableKey)
        {
            ImGui.TableSetupColumn(tableKey + "Filter" + Name, ImGuiTableColumnFlags.NoSort);
        }
        
        public delegate void ButtonPressedDelegate(string buttonName, object eventData);
        public event ButtonPressedDelegate? ButtonPressed;

        public bool DrawFilter(string tableKey, int columnIndex);
    }
}