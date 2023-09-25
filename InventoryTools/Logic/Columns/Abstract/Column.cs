using System;
using System.Collections.Generic;
using System.Numerics;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using Dalamud.Game.Text;
using ImGuiNET;
using InventoryTools.Extensions;
using Dalamud.Interface.Utility.Raii;

namespace InventoryTools.Logic.Columns.Abstract
{
    public abstract class Column<T> : IColumn
    {
        private string _filterText = "";
        public virtual uint MaxFilterLength { get; set; } = 200;

        public virtual FilterType AvailableIn => Logic.FilterType.SearchFilter | Logic.FilterType.SortingFilter |
                                                 Logic.FilterType.GameItemFilter | Logic.FilterType.CraftFilter | Logic.FilterType.HistoryFilter;

        public virtual bool? CraftOnly => null;
        public bool CanBeRemoved => true;

        public abstract ColumnCategory ColumnCategory { get; }
        public abstract T CurrentValue(InventoryItem item);
        public abstract T CurrentValue(ItemEx item);
        public abstract T CurrentValue(SortingResult item);
        public abstract T CurrentValue(CraftItem item);
        public abstract T CurrentValue(InventoryChange change);
        public abstract string CsvExport(InventoryItem item);
        public abstract string CsvExport(ItemEx item);
        public abstract string CsvExport(SortingResult item);

        public virtual string CsvExport(CraftItem item)
        {
            return CsvExport(item.Item);
        }

        public string CsvExport(InventoryChange item)
        {
            return CsvExport(item.InventoryItem);
        }

        public virtual dynamic? JsonExport(InventoryItem item)
        {
            return CurrentValue(item);
        }

        public virtual dynamic? JsonExport(ItemEx item)
        {
            return CurrentValue(item);
        }

        public virtual dynamic? JsonExport(SortingResult item)
        {
            return CurrentValue(item);
        }

        public virtual dynamic? JsonExport(CraftItem item)
        {
            return CurrentValue(item);
        }

        public dynamic? JsonExport(InventoryChange item)
        {
            return JsonExport(item.InventoryItem);
        }

        public abstract string Name { get; set; }
        public virtual string? RenderName { get; } = null;
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

        public abstract IEnumerable<InventoryItem> Filter(IEnumerable<InventoryItem> items);

        public abstract IEnumerable<SortingResult> Filter(IEnumerable<SortingResult> items);

        public abstract IEnumerable<ItemEx> Filter(IEnumerable<ItemEx> items);

        public abstract IEnumerable<CraftItem> Filter(IEnumerable<CraftItem> items);
        public abstract IEnumerable<InventoryChange> Filter(IEnumerable<InventoryChange> items);

        public abstract IEnumerable<InventoryItem> Sort(ImGuiSortDirection direction, IEnumerable<InventoryItem> items);

        public abstract IEnumerable<SortingResult> Sort(ImGuiSortDirection direction, IEnumerable<SortingResult> items);

        public abstract IEnumerable<ItemEx> Sort(ImGuiSortDirection direction, IEnumerable<ItemEx> items);
        public abstract IEnumerable<CraftItem> Sort(ImGuiSortDirection direction, IEnumerable<CraftItem> items);

        public abstract IEnumerable<InventoryChange> Sort(ImGuiSortDirection direction, IEnumerable<InventoryChange> items);

        public abstract void Draw(FilterConfiguration configuration, InventoryItem item, int rowIndex);

        public abstract void Draw(FilterConfiguration configuration, SortingResult item, int rowIndex);
        public abstract void Draw(FilterConfiguration configuration, ItemEx item, int rowIndex);

        public abstract void Draw(FilterConfiguration configuration, CraftItem item, int rowIndex);

        public virtual void Draw(FilterConfiguration configuration, InventoryChange item, int rowIndex)
        {
            Draw(configuration, item.InventoryItem, rowIndex);
        }

        public abstract IColumnEvent? DoDraw(T currentValue, int rowIndex, FilterConfiguration filterConfiguration);

        public abstract void Setup(int columnIndex);
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