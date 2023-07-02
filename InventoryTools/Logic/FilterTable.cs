using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using CriticalCommonLib.Sheets;
using CsvHelper;
using Dalamud.Interface.Colors;
using Dalamud.Logging;
using ImGuiNET;
using InventoryTools.Logic.Columns;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using OtterGui;
using OtterGui.Raii;

namespace InventoryTools.Logic
{
    public class FilterTable : RenderTableBase
    {
        public FilterTable(FilterConfiguration filterConfiguration) : base(filterConfiguration)
        {
            
        }

        public override event PreFilterSortedItemsDelegate? PreFilterSortedItems;
        
        public override event PreFilterItemsDelegate? PreFilterItems;
        public override event ChangedDelegate? Refreshed;

        public override void Refresh(InventoryToolsConfiguration configuration)
        {
            //Do something with unsortable items
            if (FilterConfiguration.FilterResult != null)
            {
                if (FilterConfiguration.FilterType == FilterType.SearchFilter 
                    || FilterConfiguration.FilterType == FilterType.SortingFilter 
                    || FilterConfiguration.FilterType == FilterType.CraftFilter)
                {
                    PluginLog.Verbose("FilterTable: Refreshing");
                    var items = FilterConfiguration.FilterResult.SortedItems.AsEnumerable();
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
                    _refreshing = false;
                    PluginService.FrameworkService.RunOnFrameworkThread(() => { Refreshed?.Invoke(this); });
                }
                else if(FilterConfiguration.FilterType == FilterType.GameItemFilter)
                {
                    PluginLog.Verbose("FilterTable: Refreshing");
                    var items = FilterConfiguration.FilterResult.AllItems.AsEnumerable();
                    items = PreFilterItems != null ? PreFilterItems.Invoke(items) : items;
                    IsSearching = false;
                    for (var index = 0; index < Columns.Count; index++)
                    {
                        var column = Columns[index];
                        if (column.FilterText != "")
                        {
                            IsSearching = true;
                        }

                        items = column.Filter((IEnumerable<ItemEx>)items);
                        if (SortColumn != null && index == SortColumn)
                        {
                            items = column.Sort(SortDirection ?? ImGuiSortDirection.None, (IEnumerable<ItemEx>)items);
                        }
                    }

                    Items = items.Where(c => c.NameString.ToString() != "").ToList();
                    RenderItems = Items.ToList();
                    NeedsRefresh = false;
                    _refreshing = false;
                    PluginService.FrameworkService.RunOnFrameworkThread(() => { Refreshed?.Invoke(this); });
                }
                else
                {
                    PluginLog.Verbose("FilterTable: Refreshing");
                    var items = FilterConfiguration.FilterResult.InventoryHistory.AsEnumerable();
                    //items = PreFilterItems != null ? PreFilterItems.Invoke(items) : items;
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
                    InventoryChanges = items.Where(c => c.InventoryItem.FormattedName != "").ToList();
                    RenderInventoryChanges = InventoryChanges.ToList();
                    NeedsRefresh = false;
                    _refreshing = false;
                    PluginService.FrameworkService.RunOnFrameworkThread(() => { Refreshed?.Invoke(this); });
                }
            }
            else
            {
                _refreshing = false;
            }
        }

        public override void RefreshColumns()
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


        private bool _refreshing = false;
        public override bool Draw(Vector2 size)
        {
            var highlightItems = HighlightItems;

            
            if (Columns.Count == 0)
            {
                if (NeedsRefresh && !_refreshing)
                {
                    _refreshing = true;
                    PluginService.FrameworkService.RunOnFrameworkThread(() => Refresh(ConfigurationManager.Config));
                }
                return true;
            }

            var isExpanded = false;

            using (var filterTableChild = ImRaii.Child("FilterTableContent", size * ImGui.GetIO().FontGlobalScale, false,
                       ImGuiWindowFlags.HorizontalScrollbar))
            {
                if (filterTableChild.Success)
                {
                    if ((FilterConfiguration.FilterType != FilterType.CraftFilter ||
                         FilterConfiguration.FilterType == FilterType.CraftFilter && ImGui.CollapsingHeader(
                             "Items in Retainers/Bags",
                             ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.CollapsingHeader)))
                    {
                        using var table = ImRaii.Table(Key, Columns.Count, _tableFlags);
                        if (table.Success)
                        {
                            isExpanded = true;
                            var refresh = false;
                            ImGui.TableSetupScrollFreeze(Math.Min(FreezeCols ?? 0, Columns.Count),
                                FreezeRows ?? (ShowFilterRow ? 2 : 1));
                            for (var index = 0; index < Columns.Count; index++)
                            {
                                var column = Columns[index];
                                column.Setup(index);
                            }
                            
                            ImGui.TableHeadersRow();
                            
                            for (var index = 0; index < Columns.Count; index++)
                            {
                                var column = Columns[index];
                                ImGui.TableSetColumnIndex(index);
                                using (ImRaii.PushColor(ImGuiCol.Text, ImGuiColors.ParsedGrey))
                                {
                                    ImGuiUtil.RightAlign("?", SortColumn == index ? 8 : 0);
                                }
                                ImGuiUtil.HoverTooltip(column.HelpText);
                            }

                            var currentSortSpecs = ImGui.TableGetSortSpecs();
                            if (currentSortSpecs.SpecsDirty)
                            {
                                var actualSpecs = currentSortSpecs.Specs;
                                if (SortColumn != actualSpecs.ColumnIndex)
                                {
                                    SortColumn = actualSpecs.ColumnIndex;
                                    refresh = true;
                                }

                                if (SortDirection != actualSpecs.SortDirection)
                                {
                                    SortDirection = actualSpecs.SortDirection;
                                    refresh = true;
                                }
                            }
                            else
                            {
                                if (SortColumn != null)
                                {
                                    SortColumn = null;
                                    refresh = true;
                                }

                                if (SortDirection != null)
                                {
                                    SortDirection = null;
                                    refresh = true;
                                }

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
                                FilterConfiguration.FilterType == FilterType.SortingFilter ||
                                FilterConfiguration.FilterType == FilterType.CraftFilter)
                            {
                                ImGuiListClipperPtr clipper;
                                unsafe
                                {
                                    clipper = new ImGuiListClipperPtr(ImGuiNative.ImGuiListClipper_ImGuiListClipper());
                                    clipper.ItemsHeight = 32;
                                }

                                clipper.Begin(RenderSortedItems.Count);
                                while (clipper.Step())
                                {
                                    for (var index = clipper.DisplayStart; index < clipper.DisplayEnd; index++)
                                    {
                                        var item = RenderSortedItems[index];
                                        ImGui.TableNextRow(ImGuiTableRowFlags.None, FilterConfiguration.TableHeight);
                                        ImGui.PushID(index);
                                        for (var columnIndex = 0; columnIndex < Columns.Count; columnIndex++)
                                        {
                                            var column = Columns[columnIndex];
                                            column.Draw(FilterConfiguration, item, index);
                                            ImGui.SameLine();
                                            if (columnIndex == Columns.Count - 1)
                                            {
                                                PluginService.PluginLogic.RightClickColumn.Draw(FilterConfiguration,
                                                    item,
                                                    index);
                                            }
                                        }

                                        ImGui.PopID();
                                    }
                                }

                                clipper.End();
                                clipper.Destroy();
                            }
                            else if(FilterConfiguration.FilterType == FilterType.GameItemFilter)
                            {
                                ImGuiListClipperPtr clipper;
                                unsafe
                                {
                                    clipper = new ImGuiListClipperPtr(ImGuiNative.ImGuiListClipper_ImGuiListClipper());
                                    clipper.ItemsHeight = 32;
                                }

                                clipper.Begin(RenderItems.Count);
                                while (clipper.Step())
                                {
                                    for (var index = clipper.DisplayStart; index < clipper.DisplayEnd; index++)
                                    {
                                        var item = RenderItems[index];
                                        ImGui.TableNextRow(ImGuiTableRowFlags.None, FilterConfiguration.TableHeight);
                                        for (var columnIndex = 0; columnIndex < Columns.Count; columnIndex++)
                                        {
                                            var column = Columns[columnIndex];
                                            column.Draw(FilterConfiguration, (ItemEx)item, index);
                                            ImGui.SameLine();
                                            if (columnIndex == Columns.Count - 1)
                                            {
                                                PluginService.PluginLogic.RightClickColumn.Draw(FilterConfiguration,
                                                    (ItemEx)item, index);
                                            }
                                        }
                                    }
                                }

                                clipper.End();
                                clipper.Destroy();
                            }
                            else
                            {
                                ImGuiListClipperPtr clipper;
                                unsafe
                                {
                                    clipper = new ImGuiListClipperPtr(ImGuiNative.ImGuiListClipper_ImGuiListClipper());
                                    clipper.ItemsHeight = 32;
                                }

                                clipper.Begin(InventoryChanges.Count);
                                while (clipper.Step())
                                {
                                    for (var index = clipper.DisplayStart; index < clipper.DisplayEnd; index++)
                                    {
                                        var item = RenderInventoryChanges[index];
                                        ImGui.TableNextRow(ImGuiTableRowFlags.None, FilterConfiguration.TableHeight);
                                        for (var columnIndex = 0; columnIndex < Columns.Count; columnIndex++)
                                        {
                                            var column = Columns[columnIndex];
                                            column.Draw(FilterConfiguration, item, index);
                                            ImGui.SameLine();
                                            if (columnIndex == Columns.Count - 1)
                                            {
                                                PluginService.PluginLogic.RightClickColumn.Draw(FilterConfiguration,item, index);
                                            }
                                        }
                                    }
                                }

                                clipper.End();
                                clipper.Destroy();
                            }
                        }
                    }
                    else
                    {
                        if (NeedsRefresh)
                        {
                            Refresh(ConfigurationManager.Config);
                        }
                    }
                }
            }
            return isExpanded;
        }

        public override void DrawFooterItems()
        {
            foreach (var column in Columns)
            {
                var result = column.DrawFooterFilter(FilterConfiguration, this);
                if (result != null)
                {
                    result.HandleEvent(FilterConfiguration);
                }
            }
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
            using (var writer = new StreamWriter(fileName,Encoding.UTF8, new FileStreamOptions()
                   {
                       Mode = FileMode.Create,
                       Access = FileAccess.ReadWrite,
                   }))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                foreach (var column in Columns)
                {
                    csv.WriteField(column.Name);
                }
                csv.NextRecord();
                if (FilterConfiguration.FilterType == FilterType.SearchFilter ||
                    FilterConfiguration.FilterType == FilterType.SortingFilter ||
                    FilterConfiguration.FilterType == FilterType.CraftFilter)
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
                else if (FilterConfiguration.FilterType == FilterType.GameItemFilter)
                {
                    foreach (var item in RenderItems)
                    {
                        foreach (var column in Columns)
                        {
                            csv.WriteField(column.CsvExport(item));
                        }
                        csv.NextRecord();
                    }
                }
                else if (FilterConfiguration.FilterType == FilterType.HistoryFilter)
                {
                    foreach (var item in RenderInventoryChanges)
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

        public string ExportToJson()
        {
            var lines = new List<dynamic>();
            var converter = new ExpandoObjectConverter();
            if (FilterConfiguration.FilterType == FilterType.SearchFilter ||
                FilterConfiguration.FilterType == FilterType.SortingFilter ||
                FilterConfiguration.FilterType == FilterType.CraftFilter)
            {
                foreach (var item in RenderSortedItems)
                {
                    var newLine = new ExpandoObject() as IDictionary<string, Object>;
                    newLine["id"] = item.InventoryItem.ItemId;
                    foreach (var column in Columns)
                    {
                        newLine[column.Name.ToLower()] = column.JsonExport(item) ?? "";
                    }
                    lines.Add(newLine);
                }
            }
            else if (FilterConfiguration.FilterType == FilterType.GameItemFilter)
            {
                foreach (var item in RenderItems)
                {
                    var newLine = new ExpandoObject() as IDictionary<string, Object>;
                    newLine["id"] = item.RowId;
                    foreach (var column in Columns)
                    {
                        newLine[column.Name.ToLower()] = column.JsonExport(item) ?? "";
                    }
                    lines.Add(newLine);
                }
            }
            else if (FilterConfiguration.FilterType == FilterType.HistoryFilter)
            {
                foreach (var item in RenderInventoryChanges)
                {
                    var newLine = new ExpandoObject() as IDictionary<string, Object>;
                    newLine["id"] = item.InventoryItem.ItemId;
                    foreach (var column in Columns)
                    {
                        newLine[column.Name.ToLower()] = column.JsonExport(item) ?? "";
                    }
                    lines.Add(newLine);
                }
            }

            return JsonConvert.SerializeObject(lines.ToArray(), Formatting.None,
                new JsonSerializerSettings()
                {
                    TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
                    TypeNameHandling = TypeNameHandling.None,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
                    Converters = new List<JsonConverter>()
                    {
                        converter
                    }
                });
        }

        public void ClearFilters()
        {
            Columns.ForEach(c => c.FilterText = "");
            NeedsRefresh = true;
        }
        
        public override void Dispose()
        {
            if (!base.Disposed)
            {
                Columns.ForEach(c => c.Dispose());
                base.Dispose();
            }
        }
    }
}