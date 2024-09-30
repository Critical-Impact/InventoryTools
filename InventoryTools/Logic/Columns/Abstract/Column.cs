using System;
using System.Collections.Generic;
using System.Numerics;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Interfaces;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services.Mediator;
using CriticalCommonLib.Sheets;
using Dalamud.Game.Text;
using ImGuiNET;
using InventoryTools.Extensions;
using Dalamud.Interface.Utility.Raii;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace InventoryTools.Logic.Columns.Abstract
{
    public abstract class Column<T> : IColumn
    {
        public Column(ILogger logger, ImGuiService imGuiService)
        {
            Logger = logger;
            ImGuiService = imGuiService;
        }

        [JsonIgnore] protected ILogger Logger { get; }
        [JsonIgnore] protected ImGuiService ImGuiService { get; }
        public virtual uint MaxFilterLength { get; set; } = 200;

        public virtual FilterType AvailableIn => Logic.FilterType.SearchFilter | Logic.FilterType.SortingFilter |
                                                 Logic.FilterType.GameItemFilter | Logic.FilterType.CraftFilter | Logic.FilterType.HistoryFilter | Logic.FilterType.CuratedList;

        public virtual bool? CraftOnly => null;
        public bool CanBeRemoved => true;
        public virtual bool IsConfigurable => false;

        public abstract ColumnCategory ColumnCategory { get; }
        public abstract T CurrentValue(ColumnConfiguration columnConfiguration, SearchResult searchResult);
        public virtual void DrawEditor(ColumnConfiguration columnConfiguration, FilterConfiguration configuration)
        {
        }

        public abstract string CsvExport(ColumnConfiguration columnConfiguration, SearchResult searchResult);

        public virtual dynamic? JsonExport(ColumnConfiguration columnConfiguration, SearchResult searchResult)
        {
            return CurrentValue(columnConfiguration, searchResult);
        }
        
        public abstract string Name { get; set; }
        public virtual string? RenderName { get; } = null;
        public virtual FilterType DefaultIn { get; } = Logic.FilterType.None;
        public abstract float Width { get; set; }
        public abstract string HelpText { get; set; }
        public virtual List<string>? FilterChoices { get; set; } = null;
        public abstract bool HasFilter { get; set; }
        public abstract ColumnFilterType FilterType { get; set; }
        
        public virtual bool IsDebug { get; set; } = false;

        public bool Disposed => _disposed;

        public bool AvailableInType(FilterType type) =>
            AvailableIn.HasFlag(InventoryTools.Logic.FilterType.SearchFilter) &&
            type.HasFlag(InventoryTools.Logic.FilterType.SearchFilter)
            ||
            (AvailableIn.HasFlag(InventoryTools.Logic.FilterType.SortingFilter) &&
             type.HasFlag(InventoryTools.Logic.FilterType.SortingFilter))
            ||
            (AvailableIn.HasFlag(InventoryTools.Logic.FilterType.CraftFilter) &&
             type.HasFlag(InventoryTools.Logic.FilterType.CraftFilter))
            ||
            (AvailableIn.HasFlag(InventoryTools.Logic.FilterType.GameItemFilter) &&
             type.HasFlag(InventoryTools.Logic.FilterType.GameItemFilter))
            ||
            (AvailableIn.HasFlag(InventoryTools.Logic.FilterType.HistoryFilter) &&
             type.HasFlag(InventoryTools.Logic.FilterType.HistoryFilter))
            ||
            (AvailableIn.HasFlag(InventoryTools.Logic.FilterType.CuratedList) &&
             type.HasFlag(InventoryTools.Logic.FilterType.CuratedList));

        public abstract IEnumerable<SearchResult> Filter(ColumnConfiguration columnConfiguration,
            IEnumerable<SearchResult> searchResults);

        public abstract IEnumerable<SearchResult> Sort(ColumnConfiguration columnConfiguration,
            ImGuiSortDirection direction, IEnumerable<SearchResult> searchResults);

        public abstract List<MessageBase>? Draw(FilterConfiguration configuration,
            ColumnConfiguration columnConfiguration,
            SearchResult searchResult, int rowIndex, int columnIndex);

        public abstract List<MessageBase>? DoDraw(SearchResult searchResult, T currentValue, int rowIndex,
            FilterConfiguration filterConfiguration, ColumnConfiguration columnConfiguration);

        public virtual void Setup(FilterConfiguration filterConfiguration, ColumnConfiguration columnConfiguration,
            int columnIndex)
        {
            var imGuiTableColumnFlags = ImGuiTableColumnFlags.WidthFixed;
            if (filterConfiguration.DefaultSortColumn != null && filterConfiguration.DefaultSortColumn == columnConfiguration.Key)
            {
                imGuiTableColumnFlags |= ImGuiTableColumnFlags.DefaultSort;
                if (filterConfiguration.DefaultSortOrder != null)
                {
                    imGuiTableColumnFlags |= filterConfiguration.DefaultSortOrder == ImGuiSortDirection.Ascending
                        ? ImGuiTableColumnFlags.PreferSortAscending
                        : ImGuiTableColumnFlags.PreferSortDescending;
                }
            }

            if (columnIndex == 0)
            {
                imGuiTableColumnFlags |= ImGuiTableColumnFlags.NoHide;
            }
            ImGui.TableSetupColumn(columnConfiguration.Name ?? (RenderName ?? Name), imGuiTableColumnFlags, Width, (uint)columnIndex);
        }
        public virtual IFilterEvent? DrawFooterFilter(FilterConfiguration configuration, FilterTable filterTable)
        {
            return null;
        }

        public virtual event IColumn.ButtonPressedDelegate? ButtonPressed
        {
            add { throw new NotSupportedException(); }
            remove { }
        }

        public virtual void InvalidateSearchCache()
        {
        }


        private bool _disposed;
        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        protected virtual void Dispose(bool disposing)
        {
            if(!Disposed && disposing)
            {

            }
            _disposed = true;         
        }

    }
}