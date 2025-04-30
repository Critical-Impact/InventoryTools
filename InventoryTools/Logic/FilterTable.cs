using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using CriticalCommonLib.Services.Mediator;

using CsvHelper;
using DalaMock.Shared.Interfaces;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using ImGuiNET;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using OtterGui;
using Dalamud.Interface.Utility.Raii;
using InventoryTools.Logic.Columns;
using InventoryTools.Logic.Settings;
using InventoryTools.Services;

namespace InventoryTools.Logic
{
    public class FilterTable : RenderTableBase
    {
        private readonly IFont _font;

        public FilterTable(ImGuiMenuService imGuiMenuService, ImGuiTooltipService imGuiTooltipService, InventoryToolsConfiguration configuration, ImGuiTooltipModeSetting tooltipModeSetting, IFont font) : base(imGuiMenuService, imGuiTooltipService, tooltipModeSetting, configuration)
        {
            _font = font;
        }


        public virtual bool DrawFilter(ColumnConfiguration columnConfiguration, string tableKey, int columnIndex)
        {
            var hasSettings = columnConfiguration.Column.FilterSettings.Any();
            var hasChanged = false;
            ImGui.TableSetColumnIndex(columnIndex);
            ImGui.PushID(columnIndex);
            float totalWidth = ImGui.GetContentRegionAvail().X;
            float spacing = ImGui.GetStyle().ItemInnerSpacing.X;
            float inputWidth = totalWidth - spacing;
            float buttonWidth = ImGui.GetFontSize() * 2.0f;

            if (hasSettings)
            {
                inputWidth -= buttonWidth;
            }

            if (columnConfiguration.Column.FilterType == ColumnFilterType.Custom)
            {
                if (columnConfiguration.Column.DrawFilter(columnConfiguration, columnIndex) == true)
                {
                    return true;
                }
            }
            else if (columnConfiguration.Column.FilterType == ColumnFilterType.Text)
            {
                var filter = columnConfiguration.FilterText;
                ImGui.SetNextItemWidth(inputWidth);
                ImGui.InputText("##" + tableKey + "FilterI" + columnConfiguration.Column.Name, ref filter, columnConfiguration.Column.MaxFilterLength);
                if (filter != columnConfiguration.FilterText)
                {
                    columnConfiguration.FilterText = filter;
                    hasChanged = true;
                }
            }
            else if (columnConfiguration.Column.FilterType == ColumnFilterType.Choice)
            {
                var currentItem = columnConfiguration.FilterText;
                ImGui.SetNextItemWidth(inputWidth);
                using (var combo = ImRaii.Combo("##Choice", currentItem))
                {
                    if (combo.Success)
                    {
                        if (columnConfiguration.Column.FilterChoices != null)
                        {
                            if (ImGui.Selectable("", false))
                            {
                                columnConfiguration.FilterText = "";
                                hasChanged = true;
                            }

                            foreach (var column in columnConfiguration.Column.FilterChoices)
                            {
                                if (ImGui.Selectable(column, currentItem == column))
                                {
                                    columnConfiguration.FilterText = column;
                                    hasChanged = true;
                                }
                            }
                        }
                    }
                }
            }
            else if (columnConfiguration.Column.FilterType == ColumnFilterType.Boolean)
            {
                var currentItem = columnConfiguration.FilterText;

                if (currentItem == "true")
                {
                    currentItem = "Yes";
                }
                else if(currentItem == "false")
                {
                    currentItem = "No";
                }
                ImGui.SetNextItemWidth(inputWidth);
                using (var combo = ImRaii.Combo("##Choice", currentItem))
                {
                    if (combo.Success)
                    {
                        if (ImGui.Selectable("", false))
                        {
                            columnConfiguration.FilterText = "";
                            hasChanged = true;
                        }



                        if (ImGui.Selectable("Yes", currentItem == "Yes"))
                        {
                            columnConfiguration.FilterText = "true";
                            hasChanged = true;
                        }

                        if (ImGui.Selectable("No", currentItem == "No"))
                        {
                            columnConfiguration.FilterText = "false";
                            hasChanged = true;
                        }
                    }
                }
            }

            if (hasSettings)
            {
                var popupLocation = new Vector2(ImGui.GetItemRectMin().X - ImGui.GetStyle().CellPadding.X, ImGui.GetItemRectMax().Y + ImGui.GetStyle().CellPadding.Y);

                var hasFilterSettings = false;
                foreach (var setting in columnConfiguration.Column.FilterSettings)
                {
                    if (setting.HasValueSet(columnConfiguration.FilterConfiguration))
                    {
                        hasFilterSettings = true;
                        break;
                    }
                }

                ImGui.SameLine();
                using (var color = ImRaii.PushColor(ImGuiCol.Text, ImGuiColors.HealerGreen, hasFilterSettings))
                {
                    using (var font = ImRaii.PushFont(_font.IconFont))
                    {
                        var opening = false;
                        if (ImGui.Button(columnConfiguration.Column.FilterIcon ?? FontAwesomeIcon.Filter.ToIconString()))
                        {
                            ImGui.SetNextWindowPos(popupLocation, ImGuiCond.Appearing);

                            ImGui.OpenPopup("ExtraFiltersPopup");
                            opening = true;
                        }

                        if (!opening && ImGui.IsItemHovered() && ImGui.IsMouseClicked(ImGuiMouseButton.Right))
                        {
                            foreach (var setting in columnConfiguration.Column.FilterSettings)
                            {
                                setting.ResetFilter(columnConfiguration.FilterConfiguration);
                            }
                            hasChanged = true;

                        }

                        if (!opening && ImGui.IsItemHovered())
                        {
                            font.Pop();
                            color.Pop();
                            using var tooltip = ImRaii.Tooltip();
                            if (tooltip)
                            {
                                ImGui.Text("Extra Filters");
                                ImGui.Text("Right Click: Clear All");
                            }
                        }
                    }
                }

                if (ImGui.BeginPopup("ExtraFiltersPopup"))
                {
                    foreach (var setting in columnConfiguration.Column.FilterSettings)
                    {
                        if (setting.DrawFilter(columnConfiguration.FilterConfiguration, null))
                        {
                            hasChanged = true;
                        }
                    }
                    ImGui.EndPopup();
                }
            }
            ImGui.PopID();

            return hasChanged;
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
                        if (!_inititalSortingSetup)
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
                                if (column.Column.HasFilter && DrawFilter(column, Key, index))
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

                            var renderSortedItems = RenderSearchResults;
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

                            clipper.Begin(RenderSearchResults.Count);
                            while (clipper.Step())
                            {
                                for (var index = clipper.DisplayStart; index < clipper.DisplayEnd; index++)
                                {
                                    if (index >= 0 && index < RenderSearchResults.Count)
                                    {
                                        var item = RenderSearchResults[index];
                                        ImGui.TableNextRow(ImGuiTableRowFlags.None, FilterConfiguration.TableHeight);
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
                                                var menuMessages = DrawMenu(FilterConfiguration, column,item, index);
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

                            clipper.Begin(RenderSearchResults.Count);
                            while (clipper.Step())
                            {
                                for (var index = clipper.DisplayStart; index < clipper.DisplayEnd; index++)
                                {
                                    var item = RenderSearchResults[index];
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
                var result = column.Column.DrawFooterFilter(column, this);
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
                foreach (var item in RenderSearchResults)
                {
                    foreach (var column in Columns)
                    {
                        csv.WriteField(column.Column.CsvExport(column, item));
                    }
                    csv.NextRecord();
                }
            }
        }

        public string ExportToJson()
        {
            return ExportToJson(RenderSearchResults);
        }

        public string ExportToJson(List<SearchResult> toExport)
        {
            var lines = new List<dynamic>();
            var converter = new ExpandoObjectConverter();
            foreach (var item in toExport)
            {
                var newLine = new ExpandoObject() as IDictionary<string, Object>;
                newLine["id"] = item.Item.RowId;
                foreach (var column in Columns)
                {
                    newLine[column.Column.Name.ToLower()] = column.Column.JsonExport(column, item) ?? "";
                }
                lines.Add(newLine);
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