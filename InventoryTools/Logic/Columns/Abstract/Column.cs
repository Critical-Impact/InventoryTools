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
        private string _filterText = "";
        public virtual uint MaxFilterLength { get; set; } = 200;

        public virtual FilterType AvailableIn => Logic.FilterType.SearchFilter | Logic.FilterType.SortingFilter |
                                                 Logic.FilterType.GameItemFilter | Logic.FilterType.CraftFilter | Logic.FilterType.HistoryFilter;

        public virtual bool? CraftOnly => null;
        public bool CanBeRemoved => true;
        public virtual bool IsConfigurable => false;

        public abstract ColumnCategory ColumnCategory { get; }
        public abstract T CurrentValue(ColumnConfiguration columnConfiguration, InventoryItem item);
        public abstract T CurrentValue(ColumnConfiguration columnConfiguration, ItemEx item);
        public abstract T CurrentValue(ColumnConfiguration columnConfiguration, SortingResult item);
        public abstract T CurrentValue(ColumnConfiguration columnConfiguration, CraftItem item);
        public abstract T CurrentValue(ColumnConfiguration columnConfiguration, InventoryChange change);
        public virtual void DrawEditor(ColumnConfiguration columnConfiguration, FilterConfiguration configuration)
        {
        }

        public abstract string CsvExport(ColumnConfiguration columnConfiguration, InventoryItem item);
        public abstract string CsvExport(ColumnConfiguration columnConfiguration, ItemEx item);
        public abstract string CsvExport(ColumnConfiguration columnConfiguration, SortingResult item);

        public virtual string CsvExport(ColumnConfiguration columnConfiguration, CraftItem item)
        {
            return CsvExport(columnConfiguration, item.Item);
        }

        public virtual string CsvExport(ColumnConfiguration columnConfiguration, InventoryChange item)
        {
            return CsvExport(columnConfiguration, item.InventoryItem);
        }

        public virtual dynamic? JsonExport(ColumnConfiguration columnConfiguration, InventoryItem item)
        {
            return CurrentValue(columnConfiguration, item);
        }

        public virtual dynamic? JsonExport(ColumnConfiguration columnConfiguration, ItemEx item)
        {
            return CurrentValue(columnConfiguration, item);
        }

        public virtual dynamic? JsonExport(ColumnConfiguration columnConfiguration, SortingResult item)
        {
            return CurrentValue(columnConfiguration, item);
        }

        public virtual dynamic? JsonExport(ColumnConfiguration columnConfiguration, CraftItem item)
        {
            return CurrentValue(columnConfiguration, item);
        }

        public dynamic? JsonExport(ColumnConfiguration columnConfiguration, InventoryChange item)
        {
            return JsonExport(columnConfiguration, item.InventoryItem);
        }

        public abstract string Name { get; set; }
        public virtual string? RenderName { get; } = null;
        public virtual FilterType DefaultIn { get; } = Logic.FilterType.None;
        public abstract float Width { get; set; }
        public abstract string HelpText { get; set; }

        public string FilterText
        {
            get => _filterText;
            set
            {
                _filterText = value.Replace((char)SeIconChar.Collectible,  ' ').Replace((char)SeIconChar.HighQuality, ' ');
                _filterComparisonText = new ComparisonExtensions.FilterComparisonText(_filterText);
            }
        }

        private ComparisonExtensions.FilterComparisonText? _filterComparisonText;

        public ComparisonExtensions.FilterComparisonText FilterComparisonText
        {
            get
            {
                if (_filterComparisonText == null)
                {
                    _filterComparisonText = new ComparisonExtensions.FilterComparisonText(FilterText);
                }

                return _filterComparisonText;
            }
        }


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
             type.HasFlag(InventoryTools.Logic.FilterType.HistoryFilter));

        public abstract IEnumerable<InventoryItem> Filter(ColumnConfiguration columnConfiguration,
            IEnumerable<InventoryItem> items);

        public abstract IEnumerable<SortingResult> Filter(ColumnConfiguration columnConfiguration,
            IEnumerable<SortingResult> items);

        public abstract IEnumerable<ItemEx> Filter(ColumnConfiguration columnConfiguration, IEnumerable<ItemEx> items);

        public abstract IEnumerable<CraftItem> Filter(ColumnConfiguration columnConfiguration,
            IEnumerable<CraftItem> items);
        public abstract IEnumerable<InventoryChange> Filter(ColumnConfiguration columnConfiguration,
            IEnumerable<InventoryChange> items);

        public abstract IEnumerable<InventoryItem> Sort(ColumnConfiguration columnConfiguration,
            ImGuiSortDirection direction, IEnumerable<InventoryItem> items);

        public abstract IEnumerable<SortingResult> Sort(ColumnConfiguration columnConfiguration,
            ImGuiSortDirection direction, IEnumerable<SortingResult> items);

        public abstract IEnumerable<ItemEx> Sort(ColumnConfiguration columnConfiguration, ImGuiSortDirection direction,
            IEnumerable<ItemEx> items);
        public abstract IEnumerable<CraftItem> Sort(ColumnConfiguration columnConfiguration,
            ImGuiSortDirection direction, IEnumerable<CraftItem> items);

        public abstract IEnumerable<InventoryChange> Sort(ColumnConfiguration columnConfiguration,
            ImGuiSortDirection direction, IEnumerable<InventoryChange> items);

        public abstract List<MessageBase>? Draw(FilterConfiguration configuration,
            ColumnConfiguration columnConfiguration,
            InventoryItem item, int rowIndex, int columnIndex);

        public abstract List<MessageBase>? Draw(FilterConfiguration configuration,
            ColumnConfiguration columnConfiguration,
            SortingResult item, int rowIndex, int columnIndex);
        public abstract List<MessageBase>? Draw(FilterConfiguration configuration,
            ColumnConfiguration columnConfiguration,
            ItemEx item, int rowIndex, int columnIndex);

        public abstract List<MessageBase>? Draw(FilterConfiguration configuration,
            ColumnConfiguration columnConfiguration,
            CraftItem item, int rowIndex, int columnIndex);

        public virtual List<MessageBase>? Draw(FilterConfiguration configuration,
            ColumnConfiguration columnConfiguration,
            InventoryChange item, int rowIndex, int columnIndex)
        {
            return Draw(configuration, columnConfiguration, item.InventoryItem, rowIndex, columnIndex);
        }

        public abstract List<MessageBase>? DoDraw(IItem item, T currentValue, int rowIndex,
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

        public virtual bool DrawFilter(string tableKey, int columnIndex)
        {
            if (FilterType == ColumnFilterType.Text)
            {
                var filter = FilterText;
                var hasChanged = false;

                ImGui.TableSetColumnIndex(columnIndex);
                ImGui.PushItemWidth(-20.000000f);
                ImGui.PushID(Name);
                ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(0, 0));
                ImGui.InputText("##" + tableKey + "FilterI" + Name, ref filter, MaxFilterLength);
                ImGui.PopStyleVar();
                ImGui.SameLine(0.0f, ImGui.GetStyle().ItemInnerSpacing.X);
                ImGui.TableHeader("");
                ImGui.PopID();
                ImGui.PopItemWidth();
                if (filter != FilterText)
                {
                    FilterText = filter;
                    hasChanged = true;
                }

                return hasChanged;
            }
            else if (FilterType == ColumnFilterType.Choice)
            {
                var hasChanged = false;
                ImGui.TableSetColumnIndex(columnIndex);
                ImGui.PushItemWidth(-20.000000f);
                using (ImRaii.PushId(Name))
                {
                    using (ImRaii.PushStyle(ImGuiStyleVar.FramePadding, new Vector2(0, 0)))
                    {

                        var currentItem = FilterText;

                        using (var combo = ImRaii.Combo("##Choice", currentItem))
                        {
                            if (combo.Success)
                            {
                                if (FilterChoices != null)
                                {
                                    if (ImGui.Selectable("", false))
                                    {
                                        FilterText = "";
                                        hasChanged = true;
                                    }

                                    foreach (var column in FilterChoices)
                                    {
                                        if (ImGui.Selectable(column, currentItem == column))
                                        {
                                            FilterText = column;
                                            hasChanged = true;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    ImGui.SameLine(0.0f, ImGui.GetStyle().ItemInnerSpacing.X);
                    ImGui.TableHeader("");
                }
                ImGui.PopItemWidth();
                return hasChanged;
            }

            return false;
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