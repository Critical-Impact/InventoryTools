using System;
using System.Collections.Generic;
using System.Linq;
using CriticalCommonLib.Models;
using Dalamud.Logging;
using ImGuiNET;

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
            filterConfiguration.ListUpdated += FilterConfigurationUpdated;
        }

        private void FilterConfigurationUpdated(FilterConfiguration filterconfiguration)
        {
            PluginLog.Log("FilterTable: Filter configuration changed");
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
                //Actually return a proper key, generate maybe?
                return FilterConfiguration.Name;
            }
        }
        public List<IColumn> Columns { get; } = new();
        public int? SortColumn { get; set; }
        public ImGuiSortDirection? SortDirection { get; set; }
        public List<SortingResult> Items { get; set; } = new List<SortingResult>();
        public int? FreezeCols { get; set; }
        public int? FreezeRows { get; set; }
        public bool ShowFilterRow { get; set; }
        public bool NeedsRefresh { get; set; }
        public FilterConfiguration FilterConfiguration { get; set; }


        public delegate IEnumerable<SortingResult> PreFilterDelegate(IEnumerable<SortingResult> items);
        public delegate void ChangedDelegate(FilterTable itemTable);
        
        public event PreFilterDelegate PreFilter;
        public event ChangedDelegate Refreshed;

        public void Refresh()
        {
            //Do something with unsortable items
            if (FilterConfiguration.FilterResult != null)
            {
                PluginLog.Log("FilterTable: Refreshing");
                var items = FilterConfiguration.FilterResult.Value.SortedItems.AsEnumerable();
                items = PreFilter != null ? PreFilter.Invoke(items) : items; 
                for (var index = 0; index < Columns.Count; index++)
                {
                    var column = Columns[index];
                    items = column.Filter(items);
                    if (SortColumn != null && index == SortColumn)
                    {
                        items = column.Sort(SortDirection ?? ImGuiSortDirection.None, items);
                    }
                }

                Items = items.ToList();
                NeedsRefresh = false;
                Refreshed?.Invoke(this);
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

        public bool HighlightItems => PluginLogic.PluginConfiguration.ActiveUiFilter == FilterConfiguration.Key;

        public void Draw()
        {
            var highlightItems = HighlightItems;
            ImGui.Checkbox( "Highlight?"+ "###" + Key + "VisibilityCheckbox", ref highlightItems);
            if (highlightItems != HighlightItems)
            {
                PluginLogic.Instance.ToggleActiveUiFilterByKey(FilterConfiguration.Key);
            }
            if (ImGui.BeginTable(Key, Columns.Count, _tableFlags))
            {
                var refresh = false;
                ImGui.TableSetupScrollFreeze(FreezeCols ?? 0, FreezeRows ?? (ShowFilterRow ? 2 : 1));
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
                        if (column.DrawFilter(Key, index))
                        {
                            refresh = true;
                        }
                    }
                }

                if (refresh || NeedsRefresh)
                {
                    Refresh();
                }

                for (var index = 0; index < Items.Count; index++)
                {
                    var item = Items[index];
                    ImGui.TableNextRow();
                    for (var i = 0; i < Columns.Count; i++)
                    {
                        var column = Columns[i];
#if DEBUG
                        if (i == 1 && ImGui.BeginPopupContextItem(index + "_" + i))
                        {
                            ImGui.Text(item.GetExtraInformation());
                            if (ImGui.Button("Close"))
                                ImGui.CloseCurrentPopup();
                            ImGui.EndPopup();
                        }
#endif
                        column.Draw(item, index);
                    }
                }

                ImGui.EndTable();
            }
        }

        public void Dispose()
        {
            FilterConfiguration.ConfigurationChanged -= FilterConfigurationUpdated;
            FilterConfiguration.ListUpdated -= FilterConfigurationUpdated;
        }
    }
}