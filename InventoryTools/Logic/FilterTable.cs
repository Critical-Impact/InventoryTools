using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using CriticalCommonLib.MarketBoard;
using CsvHelper;
using Dalamud.Logging;
using ImGuiNET;
using InventoryTools.Logic.Columns;
using Lumina.Excel.GeneratedSheets;

namespace InventoryTools.Logic
{
    public class FilterTable : IDisposable
    {
        private ImGuiTableFlags _tableFlags = ImGuiTableFlags.RowBg | ImGuiTableFlags.BordersV |
                                              ImGuiTableFlags.BordersOuterV | ImGuiTableFlags.BordersInnerV |
                                              ImGuiTableFlags.BordersH | ImGuiTableFlags.BordersOuterH |
                                              ImGuiTableFlags.BordersInnerH |
                                              ImGuiTableFlags.Resizable | ImGuiTableFlags.Sortable |
                                              ImGuiTableFlags.Hideable | ImGuiTableFlags.ScrollX |
                                              ImGuiTableFlags.ScrollY;

        public FilterTable(FilterConfiguration filterConfiguration)
        {
            FilterConfiguration = filterConfiguration;
            filterConfiguration.ConfigurationChanged += FilterConfigurationUpdated;
            filterConfiguration.TableConfigurationChanged += FilterConfigurationOnTableConfigurationChanged;
            filterConfiguration.ListUpdated += FilterConfigurationUpdated;
            unsafe
            {
                var clipperNative = Marshal.AllocHGlobal(Marshal.SizeOf<ImGuiListClipper>());
                var clipper = new ImGuiListClipper();
                Marshal.StructureToPtr(clipper, clipperNative, false);
                _clipper = new ImGuiListClipperPtr(clipperNative);
                _clipper.ItemsHeight = 32;
            }

        }

        private void FilterConfigurationOnTableConfigurationChanged(FilterConfiguration filterconfiguration)
        {
            RefreshColumns();
        }

        private ImGuiListClipperPtr _clipper;

        private void FilterConfigurationUpdated(FilterConfiguration filterconfiguration)
        {
            this.NeedsRefresh = true;
        }

        public string Name
        {
            get
            {
                return FilterConfiguration.Name;
            }
        }

        public string Key
        {
            get
            {
                return FilterConfiguration.TableId;
            }
        }
        public List<IColumn> Columns { get; private set; } = new();
        public int? SortColumn { get; set; }
        public ImGuiSortDirection? SortDirection { get; set; }
        public List<SortingResult> SortedItems { get; set; } = new List<SortingResult>();
        public List<SortingResult> RenderSortedItems { get; set; } = new List<SortingResult>();
        public List<Item> Items { get; set; } = new List<Item>();
        public List<Item> RenderItems { get; set; } = new List<Item>();
        public int? FreezeCols { get; set; }
        public int? FreezeRows { get; set; }
        public bool ShowFilterRow { get; set; }
        public bool NeedsRefresh { get; set; }
        public bool IsSearching { get; set; }
        public FilterConfiguration FilterConfiguration { get; set; }


        public delegate IEnumerable<SortingResult> PreFilterSortedItemsDelegate(IEnumerable<SortingResult> items);
        public delegate IEnumerable<Item> PreFilterItemsDelegate(IEnumerable<Item> items);
        public delegate void ChangedDelegate(FilterTable itemTable);
        
        public event PreFilterSortedItemsDelegate? PreFilterSortedItems;
        
        public event PreFilterItemsDelegate? PreFilterItems;
        public event ChangedDelegate? Refreshed;

        public void Refresh(InventoryToolsConfiguration configuration)
        {
            //Do something with unsortable items
            if (FilterConfiguration.FilterResult != null)
            {
                if (FilterConfiguration.FilterType == FilterType.SearchFilter ||
                    FilterConfiguration.FilterType == FilterType.SortingFilter)
                {
                    PluginLog.Verbose("FilterTable: Refreshing");
                    var items = FilterConfiguration.FilterResult.Value.SortedItems.AsEnumerable();
                    items = PreFilterSortedItems != null ? PreFilterSortedItems.Invoke(items) : items;
                    IsSearching = false;
                    for (var index = 0; index < Columns.Count; index++)
                    {
                        var column = Columns[index];
                        if (column.FilterText != "")
                        {
                            IsSearching = true;
                        }

                        items = column.Filter(items);
                        if (SortColumn != null && index == SortColumn)
                        {
                            items = column.Sort(SortDirection ?? ImGuiSortDirection.None, items);
                        }
                    }

                    SortedItems = items.ToList();
                    RenderSortedItems = SortedItems.Where(item => !item.InventoryItem.IsEmpty).ToList();
                    NeedsRefresh = false;
                    Refreshed?.Invoke(this);
                }
                else
                {
                    PluginLog.Verbose("FilterTable: Refreshing");
                    var items = FilterConfiguration.FilterResult.Value.AllItems.AsEnumerable();
                    items = PreFilterItems != null ? PreFilterItems.Invoke(items) : items;
                    IsSearching = false;
                    for (var index = 0; index < Columns.Count; index++)
                    {
                        var column = Columns[index];
                        if (column.FilterText != "")
                        {
                            IsSearching = true;
                        }

                        items = column.Filter(items);
                        if (SortColumn != null && index == SortColumn)
                        {
                            items = column.Sort(SortDirection ?? ImGuiSortDirection.None, items);
                        }
                    }

                    Items = items.Where(c => c.Name.ToString() != "").ToList();
                    RenderItems = Items.ToList();
                    NeedsRefresh = false;
                    Refreshed?.Invoke(this);
                }
            }
        }

        public void RefreshColumns()
        {
            if (FilterConfiguration.FreezeColumns != FreezeCols)
            {
                FreezeCols = FilterConfiguration.FreezeColumns;
            }
            if (FilterConfiguration.Columns != null)
            {
                var newColumns = new List<IColumn>();
                foreach (var column in FilterConfiguration.Columns)
                {
                    var newColumn = PluginLogic.GetClassFromString(column);
                    if (newColumn != null && newColumn is IColumn)
                    {
                        newColumns.Add(newColumn);
                    }
                }

                this.Columns = newColumns;
            }
        }

        public void AddColumn(IColumn column)
        {
            Columns.Add(column);
        }

        public void SortTable(IColumn column, ImGuiSortDirection sortDirection)
        {
            SortColumn = Columns.IndexOf(column);
            SortDirection = sortDirection;
        }

        public void ClearSort()
        {
            SortColumn = null;
            SortDirection = null;
        }

        public void SetTableFlags(ImGuiTableFlags tableFlags)
        {
            _tableFlags = tableFlags;
        }

        public void RefreshPricing()
        {
            foreach (var item in RenderSortedItems)
            {
                Universalis.QueuePriceCheck(item.InventoryItem.ItemId);
            }
            foreach (var item in RenderItems)
            {
                Universalis.QueuePriceCheck(item.RowId);
            }
        }

        public bool HighlightItems => PluginLogic.PluginConfiguration.ActiveUiFilter == FilterConfiguration.Key;

        public void Draw()
        {
            var highlightItems = HighlightItems;
            ImGui.BeginChild("TopBar", new Vector2(0, 20)* ImGui.GetIO().FontGlobalScale);
            ImGui.Checkbox( "Highlight?"+ "###" + Key + "VisibilityCheckbox", ref highlightItems);
            if (highlightItems != HighlightItems)
            {
                PluginService.PluginLogic.ToggleActiveUiFilterByKey(FilterConfiguration.Key);
            }

            if (Columns.Count == 0)
            {
                if (NeedsRefresh)
                {
                    Refresh(ConfigurationManager.Config);
                }
                return;
            }
            ImGui.EndChild();
            ImGui.BeginChild("Content", new Vector2(0, -25)* ImGui.GetIO().FontGlobalScale); 
            if (ImGui.BeginTable(Key, Columns.Count, _tableFlags))
            {
                var refresh = false;
                ImGui.TableSetupScrollFreeze(Math.Min(FreezeCols ?? 0,Columns.Count), FreezeRows ?? (ShowFilterRow ? 2 : 1));
                for (var index = 0; index < Columns.Count; index++)
                {
                    var column = Columns[index];
                    column.Setup(index);
                }
                ImGui.TableHeadersRow();

                var currentSortSpecs = ImGui.TableGetSortSpecs();
                if (currentSortSpecs.SpecsDirty)
                {
                    var actualSpecs = currentSortSpecs.Specs;
                    if (SortColumn != actualSpecs.ColumnIndex)
                    {
                        PluginLog.Verbose("specs dirty");
                        SortColumn = actualSpecs.ColumnIndex;
                        refresh = true;
                    }

                    if (SortDirection != actualSpecs.SortDirection)
                    {
                        PluginLog.Verbose("specs dirty");
                        SortDirection = actualSpecs.SortDirection;
                        refresh = true;
                    }
                }
                else
                {
                    SortColumn = null;
                    SortDirection = null;
                    refresh = true;
                }

                if (ShowFilterRow)
                {
                    ImGui.TableNextRow(ImGuiTableRowFlags.Headers);
                    foreach (var column in Columns)
                    {
                        column.SetupFilter(Key);
                    }

                    for (var index = 0; index < Columns.Count; index++)
                    {
                        var column = Columns[index];
                        if (column.HasFilter && column.DrawFilter(Key, index))
                        {
                            refresh = true;
                        }
                    }
                }

                if (refresh || NeedsRefresh)
                {
                    Refresh(ConfigurationManager.Config);
                }
                
                if (FilterConfiguration.FilterType == FilterType.SearchFilter ||
                    FilterConfiguration.FilterType == FilterType.SortingFilter)
                {
                    _clipper.Begin(RenderSortedItems.Count);
                    while (_clipper.Step())
                    {
                        for (var index = _clipper.DisplayStart; index < _clipper.DisplayEnd; index++)
                        {
                            var item = RenderSortedItems[index];
                            ImGui.TableNextRow(ImGuiTableRowFlags.None, 32);
                            for (var columnIndex = 0; columnIndex < Columns.Count; columnIndex++)
                            {
                                var column = Columns[columnIndex];
                                column.Draw(item, index);
                                ImGui.SameLine();
                                if (columnIndex == Columns.Count - 1)
                                {
                                    PluginService.PluginLogic.RightClickColumn.Draw(item, index);
                                }
                            }
                        }
                    }
                }
                else
                {
                    _clipper.Begin(RenderItems.Count);
                    while (_clipper.Step())
                    {
                        for (var index = _clipper.DisplayStart; index < _clipper.DisplayEnd; index++)
                        {
                            var item = RenderItems[index];
                            ImGui.TableNextRow(ImGuiTableRowFlags.None, 32);
                            for (var columnIndex = 0; columnIndex < Columns.Count; columnIndex++)
                            {
                                var column = Columns[columnIndex];
                                column.Draw(item, index);
                                ImGui.SameLine();
                                if (columnIndex == Columns.Count - 1)
                                {
                                    PluginService.PluginLogic.RightClickColumn.Draw(item, index);
                                }
                            }
                        }
                    }
                }
                ImGui.EndTable();
            }
            ImGui.EndChild();
            ImGui.BeginChild("BottomBar", new Vector2(0,0), false, ImGuiWindowFlags.None);
            if (ImGui.Button("Refresh Market Prices"))
            {
                RefreshPricing();
            }
            ImGui.SameLine();
            if (ImGui.Button("Export to CSV"))
            {
                PluginService.FileDialogManager.SaveFileDialog("Save to csv", "*.csv", "export.csv", ".csv", SaveCallback, null, true);
            }
            ImGui.SameLine();
            ImGui.Text("Pending Market Requests: " + Universalis.QueuedCount);
            ImGui.EndTabItem();
            ImGui.EndChild();
        }

        public void SaveCallback(bool arg1, string arg2)
        {
            if (arg1)
            {
                ExportToCsv(arg2);
            }
        }

        public void ExportToCsv(string fileName)
        {
            using (var writer = new StreamWriter(fileName))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                foreach (var column in Columns)
                {
                    csv.WriteField(column.Name);
                }
                csv.NextRecord();
                if (FilterConfiguration.FilterType == FilterType.SearchFilter ||
                    FilterConfiguration.FilterType == FilterType.SortingFilter)
                {
                    foreach (var item in RenderSortedItems)
                    {
                        foreach (var column in Columns)
                        {
                            csv.WriteField(column.CsvExport(item));
                        }
                        csv.NextRecord();
                    }
                }
            }

        }

        public void Dispose()
        {
            FilterConfiguration.ConfigurationChanged -= FilterConfigurationUpdated;
            FilterConfiguration.ListUpdated -= FilterConfigurationUpdated;
            FilterConfiguration.TableConfigurationChanged += FilterConfigurationOnTableConfigurationChanged;
            unsafe
            {
                Marshal.FreeHGlobal(new IntPtr(_clipper.NativePtr));
            }
        }
    }
}