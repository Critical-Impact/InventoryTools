using System;
using System.Collections.Generic;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services.Mediator;
using CriticalCommonLib.Sheets;
using ImGuiNET;
using InventoryTools.Logic.Columns.Abstract;

namespace InventoryTools.Logic.Columns
{
    public interface IColumn : IDisposable
    {
        public string Name { get; set; }
        public float Width { get; set; }
        
        public string HelpText { get; set; }
        
        
        public List<string>? FilterChoices { get; set; }
        public ColumnCategory ColumnCategory { get; }
        
        public bool HasFilter { get; set; }
        
        public ColumnFilterType FilterType { get; set; }
        
        public bool IsDebug { get; set; }

        public FilterType AvailableIn { get; }
        public bool AvailableInType(FilterType type);
        
        public bool? CraftOnly { get; }
        
        public bool IsConfigurable { get; }

        public string? RenderName { get; }
        
        public FilterType DefaultIn { get; }
        public uint MaxFilterLength { get; set; }
        
        public IEnumerable<InventoryItem> Filter(ColumnConfiguration columnConfiguration,
            IEnumerable<InventoryItem> items);
        public IEnumerable<SortingResult> Filter(ColumnConfiguration columnConfiguration,
            IEnumerable<SortingResult> items);
        public IEnumerable<ItemEx> Filter(ColumnConfiguration columnConfiguration, IEnumerable<ItemEx> items);
        public IEnumerable<CraftItem> Filter(ColumnConfiguration columnConfiguration, IEnumerable<CraftItem> items);
        public IEnumerable<InventoryChange> Filter(ColumnConfiguration columnConfiguration,
            IEnumerable<InventoryChange> items);
        
        public IEnumerable<InventoryItem> Sort(ColumnConfiguration columnConfiguration, ImGuiSortDirection direction,
            IEnumerable<InventoryItem> items);
        public IEnumerable<SortingResult> Sort(ColumnConfiguration columnConfiguration, ImGuiSortDirection direction,
            IEnumerable<SortingResult> items);
        public IEnumerable<ItemEx> Sort(ColumnConfiguration columnConfiguration, ImGuiSortDirection direction,
            IEnumerable<ItemEx> items);
        
        public IEnumerable<CraftItem> Sort(ColumnConfiguration columnConfiguration, ImGuiSortDirection direction,
            IEnumerable<CraftItem> items);
        public IEnumerable<InventoryChange> Sort(ColumnConfiguration columnConfiguration, ImGuiSortDirection direction,
            IEnumerable<InventoryChange> items);
        
        public List<MessageBase>? Draw(FilterConfiguration configuration, ColumnConfiguration columnConfiguration,
            InventoryItem item,
            int rowIndex, int columnIndex);
        public List<MessageBase>? Draw(FilterConfiguration configuration, ColumnConfiguration columnConfiguration,
            SortingResult item,
            int rowIndex, int columnIndex);
        public List<MessageBase>? Draw(FilterConfiguration configuration, ColumnConfiguration columnConfiguration,
            ItemEx item,
            int rowIndex, int columnIndex);
        
        public List<MessageBase>? Draw(FilterConfiguration configuration, ColumnConfiguration columnConfiguration,
            CraftItem item,
            int rowIndex, int columnIndex);
        public List<MessageBase>? Draw(FilterConfiguration configuration, ColumnConfiguration columnConfiguration,
            InventoryChange item, int rowIndex, int columnIndex);

        public void DrawEditor(ColumnConfiguration columnConfiguration, FilterConfiguration configuration);

        public string CsvExport(ColumnConfiguration columnConfiguration, InventoryItem item);
        public string CsvExport(ColumnConfiguration columnConfiguration, SortingResult item);
        public string CsvExport(ColumnConfiguration columnConfiguration, ItemEx item);
        public string CsvExport(ColumnConfiguration columnConfiguration, CraftItem item);
        public string CsvExport(ColumnConfiguration columnConfiguration, InventoryChange item);

        public dynamic? JsonExport(ColumnConfiguration columnConfiguration, InventoryItem item);
        public dynamic? JsonExport(ColumnConfiguration columnConfiguration, SortingResult item);
        public dynamic? JsonExport(ColumnConfiguration columnConfiguration, ItemEx item);
        public dynamic? JsonExport(ColumnConfiguration columnConfiguration, CraftItem item);
        public dynamic? JsonExport(ColumnConfiguration columnConfiguration, InventoryChange item);

        public void Setup(FilterConfiguration filterConfiguration, ColumnConfiguration configuration, int columnIndex);
        
        public void SetupFilter(string tableKey)
        {
            ImGui.TableSetupColumn(tableKey + "Filter" + Name, ImGuiTableColumnFlags.NoSort);
        }

        public IFilterEvent? DrawFooterFilter(FilterConfiguration configuration, FilterTable filterTable);
        
        public delegate void ButtonPressedDelegate(string buttonName, object eventData);
        public event ButtonPressedDelegate? ButtonPressed;
        
        public void InvalidateSearchCache();
    }
}