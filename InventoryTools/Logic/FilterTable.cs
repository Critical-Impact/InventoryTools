using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using CriticalCommonLib.Services.Mediator;
using CriticalCommonLib.Sheets;
using CsvHelper;
using Dalamud.Interface.Colors;
using ImGuiNET;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using OtterGui;
using Dalamud.Interface.Utility.Raii;
using InventoryTools.Services;

namespace InventoryTools.Logic
{
    public class FilterTable : RenderTableBase
    {
        public FilterTable(RightClickService rightClickService, InventoryToolsConfiguration configuration) : base(rightClickService, configuration)
        {
            
        }

        public override void RefreshColumns()
        {
            if (FilterConfiguration.FreezeColumns != FreezeCols)
            {
                FreezeCols = FilterConfiguration.FreezeColumns;
            }
            _inititalSortingSetup = false;
            if (FilterConfiguration.Columns != null)
            {
                this.Columns = FilterConfiguration.Columns.ToList();
            }
        }

        private bool _inititalSortingSetup = false;
        private bool _refreshing = false;
        public override List<MessageBase> Draw(Vector2 size, bool shouldDraw = true)
        {
            var messages = new List<MessageBase>();
            var highlightItems = HighlightItems;

            FilterConfiguration.AllowRefresh = true;
            
            if (Columns.Count == 0 || !shouldDraw)
            {
                return messages;
            }

            using (var filterTableChild = ImRaii.Child("FilterTableContent", size * ImGui.GetIO().FontGlobalScale, false,
                       ImGuiWindowFlags.HorizontalScrollbar))
            {
                if (filterTableChild.Success)
                {
                    using var table = ImRaii.Table(Key, Columns.Count, _tableFlags);
                    if (table.Success)
                    {
                        var refresh = false;
                        ImGui.TableSetupScrollFreeze(Math.Min(FreezeCols ?? 0, Columns.Count),
                            FreezeRows ?? (ShowFilterRow ? 2 : 1));
                        for (var index = 0; index < Columns.Count; index++)
                        {
                            var column = Columns[index];
                            column.Column.Setup(FilterConfiguration, column, index);
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
                            ImGuiUtil.HoverTooltip(column.Column.HelpText);
                        }

                        var currentSortSpecs = ImGui.TableGetSortSpecs();
                        if (_inititalSortingSetup == false)
                        {
                            currentSortSpecs.Specs.SortDirection = FilterConfiguration.DefaultSortOrder ?? ImGuiSortDirection.Ascending;
                            currentSortSpecs.Specs.ColumnIndex = FilterConfiguration.Columns != null ? (short)FilterConfiguration.Columns.FindIndex(c => c.Key == FilterConfiguration.DefaultSortColumn) : (short)0;
                            currentSortSpecs.SpecsCount = 1;
                            currentSortSpecs.SpecsDirty = true;
                            _inititalSortingSetup = true;
                        }
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
                            currentSortSpecs.SpecsDirty = false;
                        }

                        if (ShowFilterRow)
                        {
                            ImGui.TableNextRow(ImGuiTableRowFlags.Headers);
                            foreach (var column in Columns)
                            {
                                column.Column.SetupFilter(Key);
                            }


                            for (var index = 0; index < Columns.Count; index++)
                            {
                                var column = Columns[index];
                                if (column.Column.HasFilter && column.DrawFilter(Key, index))
                                {
                                    refresh = true;
                                }
                            }
                        }

                        if (refresh && !Refreshing)
                        {
                            NeedsRefresh = true;
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

                            var renderSortedItems = RenderSortedItems;
                            clipper.Begin(renderSortedItems.Count);
                            while (clipper.Step())
                            {
                                for (var index = clipper.DisplayStart; index < clipper.DisplayEnd; index++)
                                {
                                    var item = renderSortedItems[index];
                                    ImGui.TableNextRow(ImGuiTableRowFlags.None, FilterConfiguration.TableHeight);
                                    ImGui.PushID(index);
                                    for (var columnIndex = 0; columnIndex < Columns.Count; columnIndex++)
                                    {
                                        var column = Columns[columnIndex];
                                        var columnMessages = column.Column.Draw(FilterConfiguration, column, item, index, columnIndex);
                                        if (columnMessages != null)
                                        {
                                            messages.AddRange(columnMessages);
                                        }
                                        if (columnIndex == 0)
                                        {
                                            ImGui.SameLine();
                                            var menuMessages = DrawMenu(FilterConfiguration, column,
                                                item,
                                                index);
                                            if (menuMessages != null)
                                            {
                                                messages.AddRange(menuMessages);
                                            }
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
                                    if (index >= 0 && index < RenderItems.Count)
                                    {
                                        var item = RenderItems[index];
                                        ImGui.TableNextRow(ImGuiTableRowFlags.None, FilterConfiguration.TableHeight);
                                        for (var columnIndex = 0; columnIndex < Columns.Count; columnIndex++)
                                        {
                                            var column = Columns[columnIndex];
                                            var columnMessages = column.Column.Draw(FilterConfiguration, column, (ItemEx)item, index, columnIndex);
                                            if (columnMessages != null)
                                            {
                                                messages.AddRange(columnMessages);
                                            }

                                            if (columnIndex == 0)
                                            {
                                                ImGui.SameLine();
                                                var menuMessages = DrawMenu(FilterConfiguration, column,
                                                    (ItemEx)item, index);
                                                if (menuMessages != null)
                                                {
                                                    messages.AddRange(menuMessages);
                                                }
                                            }
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
                                        var columnMessages = column.Column.Draw(FilterConfiguration, column, item, index, columnIndex);
                                        if (columnMessages != null)
                                        {
                                            messages.AddRange(columnMessages);
                                        }
                                        ImGui.SameLine();
                                        if (columnIndex == Columns.Count - 1)
                                        {
                                            var menuMessages = DrawMenu(FilterConfiguration, column,item, index);
                                            if (menuMessages != null)
                                            {
                                                messages.AddRange(menuMessages);
                                            }
                                        }
                                    }
                                }
                            }

                            clipper.End();
                            clipper.Destroy();
                        }
                    }
                }
            }
            return messages;
        }

        public override void DrawFooterItems()
        {
            foreach (var column in Columns)
            {
                var result = column.Column.DrawFooterFilter(FilterConfiguration, this);
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
                    csv.WriteField(column.ExportName ?? column.Column.Name);
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
                            csv.WriteField(column.Column.CsvExport(column, item));
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
                            csv.WriteField(column.Column.CsvExport(column, item));
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
                            csv.WriteField(column.Column.CsvExport(column, item));
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
                        newLine[column.Column.Name.ToLower()] = column.Column.JsonExport(column, item) ?? "";
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
                        newLine[column.Column.Name.ToLower()] = column.Column.JsonExport(column, item) ?? "";
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
                        newLine[column.Column.Name.ToLower()] = column.Column.JsonExport(column, item) ?? "";
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
    }
}