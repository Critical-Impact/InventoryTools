using System;
using System.Collections.Generic;
using CriticalCommonLib.Services.Mediator;
using DalaMock.Host.Mediator;
using Dalamud.Bindings.ImGui;
using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Logic.Columns.Abstract.ColumnSettings;

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
        public List<IColumnSetting> FilterSettings { get; set; }
        public List<IColumnSetting> Settings { get; set; }
        public string? FilterIcon { get; }
        public IEnumerable<SearchResult> Filter(ColumnConfiguration columnConfiguration,
            IEnumerable<SearchResult> items);
        public IEnumerable<SearchResult> Sort(ColumnConfiguration columnConfiguration, ImGuiSortDirection direction,
            IEnumerable<SearchResult> items);
        public List<MessageBase>? Draw(FilterConfiguration configuration, ColumnConfiguration columnConfiguration,
            SearchResult searchResult,
            int rowIndex, int columnIndex);
        public List<MessageBase>? DrawEditor(ColumnConfiguration columnConfiguration, FilterConfiguration configuration);
        public string CsvExport(ColumnConfiguration columnConfiguration, SearchResult item);
        public dynamic? JsonExport(ColumnConfiguration columnConfiguration, SearchResult item);

        public void Setup(FilterConfiguration filterConfiguration, ColumnConfiguration configuration, int columnIndex);

        public void SetupFilter(string tableKey)
        {
        }

        public bool? DrawFilter(ColumnConfiguration columnConfiguration, int columnIndex);

        public IFilterEvent? DrawFooterFilter(ColumnConfiguration columnConfiguration, FilterTable filterTable);

        public delegate void ButtonPressedDelegate(string buttonName, object eventData);
        public event ButtonPressedDelegate? ButtonPressed;

        public void InvalidateSearchCache();
    }
}