using System.Collections.Generic;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using ImGuiNET;

namespace InventoryTools.Logic.Columns
{
    public interface IColumn
    {
        public string Name { get; set; }
        public float Width { get; set; }
        
        public string HelpText { get; set; }
        
        public string FilterText { get; set; }
        
        public List<string>? FilterChoices { get; set; }
        
        public bool HasFilter { get; set; }
        
        public ColumnFilterType FilterType { get; set; }
        
        public bool IsDebug { get; set; }

        public FilterType AvailableIn { get; }
        public bool AvailableInType(FilterType type);
        
        public IEnumerable<InventoryItem> Filter(IEnumerable<InventoryItem> items);
        public IEnumerable<SortingResult> Filter(IEnumerable<SortingResult> items);
        public IEnumerable<ItemEx> Filter(IEnumerable<ItemEx> items);
        public IEnumerable<CraftItem> Filter(IEnumerable<CraftItem> items);
        
        public IEnumerable<InventoryItem> Sort(ImGuiSortDirection direction, IEnumerable<InventoryItem> items);
        public IEnumerable<SortingResult> Sort(ImGuiSortDirection direction, IEnumerable<SortingResult> items);
        public IEnumerable<ItemEx> Sort(ImGuiSortDirection direction, IEnumerable<ItemEx> items);
        
        public IEnumerable<CraftItem> Sort(ImGuiSortDirection direction, IEnumerable<CraftItem> items);
        
        public void Draw(FilterConfiguration configuration, InventoryItem item, int rowIndex);
        public void Draw(FilterConfiguration configuration, SortingResult item, int rowIndex);
        public void Draw(FilterConfiguration configuration, ItemEx item, int rowIndex);
        
        public void Draw(FilterConfiguration configuration, CraftItem item, int rowIndex);

        public string CsvExport(InventoryItem item);
        public string CsvExport(SortingResult item);
        public string CsvExport(ItemEx item);
        public string CsvExport(CraftItem item);

        public void Setup(int columnIndex);
        
        public void SetupFilter(string tableKey)
        {
            ImGui.TableSetupColumn(tableKey + "Filter" + Name, ImGuiTableColumnFlags.NoSort);
        }

        public IFilterEvent? DrawFooterFilter(FilterConfiguration configuration);
        
        public delegate void ButtonPressedDelegate(string buttonName, object eventData);
        public event ButtonPressedDelegate? ButtonPressed;

        public bool DrawFilter(string tableKey, int columnIndex);
    }
}