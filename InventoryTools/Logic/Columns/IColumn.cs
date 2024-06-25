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
        public IEnumerable<SearchResult> Filter(ColumnConfiguration columnConfiguration,
            IEnumerable<SearchResult> items);
        public IEnumerable<SearchResult> Sort(ColumnConfiguration columnConfiguration, ImGuiSortDirection direction,
            IEnumerable<SearchResult> items);
        public List<MessageBase>? Draw(FilterConfiguration configuration, ColumnConfiguration columnConfiguration,
            SearchResult searchResult,
            int rowIndex, int columnIndex);
        public void DrawEditor(ColumnConfiguration columnConfiguration, FilterConfiguration configuration);
        public string CsvExport(ColumnConfiguration columnConfiguration, SearchResult item);
        public dynamic? JsonExport(ColumnConfiguration columnConfiguration, SearchResult item);

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