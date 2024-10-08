using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Services.Mediator;
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
    public class CraftItemTable : RenderTableBase
    {
        public CraftItemTable(RightClickService rightClickService, InventoryToolsConfiguration configuration) : base(rightClickService, configuration)
        {
            _tableFlags = ImGuiTableFlags.RowBg | ImGuiTableFlags.BordersV |
                          ImGuiTableFlags.BordersOuterV | ImGuiTableFlags.BordersInnerV |
                          ImGuiTableFlags.BordersH | ImGuiTableFlags.BordersOuterH |
                          ImGuiTableFlags.BordersInnerH |
                          ImGuiTableFlags.Resizable |
                          ImGuiTableFlags.Hideable | ImGuiTableFlags.ScrollX |
                          ImGuiTableFlags.ScrollY;
        }

        public override void Initialize(FilterConfiguration filterConfiguration)
        {
            base.Initialize(filterConfiguration);
            filterConfiguration.CraftList.GenerateCraftChildren();
            filterConfiguration.NeedsRefresh = true;
        }

        public override void RefreshColumns()
        {
            if (FilterConfiguration.FreezeCraftColumns != FreezeCols)
            {
                FreezeCols = FilterConfiguration.FreezeCraftColumns;
            }
            if (FilterConfiguration.CraftColumns != null)
            {
                this.Columns = FilterConfiguration.CraftColumns.ToList();
            }
        }

        public List<SearchResult> CraftItems = new();

        public List<(CraftGrouping craftGrouping, List<SearchResult> searchResults)> CraftGroups = new();

        public override List<MessageBase> Draw(Vector2 size, bool shouldDraw = true)
        {
            var messages = new List<MessageBase>();
            FilterConfiguration.AllowRefresh = true;
            if (Columns.Count == 0 || !shouldDraw)
            {
                return messages;
            }

            var tabMode = FilterConfiguration.CraftDisplayMode == CraftDisplayMode.Tabs;

            if (tabMode)
            {
                using (var craftContentChild = ImRaii.Child("CraftContent", size * ImGui.GetIO().FontGlobalScale))
                {
                    if (craftContentChild.Success)
                    {
                        using var tabBar = ImRaii.TabBar("CraftTabs", ImGuiTabBarFlags.FittingPolicyScroll | ImGuiTabBarFlags.TabListPopupButton);
                        if (!tabBar.Success) return messages;

                        var groupedCrafts = CraftGroups;
                        if (groupedCrafts.Count == 0)
                        {
                            using var tabItem = ImRaii.TabItem("No Items");
                            if (!tabItem.Success) return messages;
                            ImGui.TextWrapped(
                                "No items have been added to the list. Add items via the search menu button at the top right of the screen or by right clicking on an item anywhere within the plugin.");
                        }
                        else
                        {
                            foreach (var groupedCraft in groupedCrafts)
                            {
                                using var tabItem = ImRaii.TabItem( groupedCraft.Item1.FormattedName());
                                if (!tabItem.Success) continue;

                                using var table = ImRaii.Table(Key + "CraftTable", Columns.Count, _tableFlags);
                                if (!table.Success || Columns.Count == 0) continue;
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

                                if (refresh && !Refreshing)
                                {
                                    NeedsRefresh = true;
                                }

                                if (!groupedCraft.searchResults.Any()) continue;
                                ImGui.TableNextRow(ImGuiTableRowFlags.Headers, FilterConfiguration.TableHeight);
                                ImGui.TableNextColumn();

                                for (var index = 0; index < groupedCraft.searchResults.Count; index++)
                                {
                                    var item = groupedCraft.searchResults[index];
                                    ImGui.TableNextRow(ImGuiTableRowFlags.None,
                                        FilterConfiguration.TableHeight);
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
                                            var menuItems = DrawMenu(
                                                FilterConfiguration, column,
                                                item,
                                                index);
                                            if (menuItems != null)
                                            {
                                                messages.AddRange(menuItems);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                using (var craftContentChild = ImRaii.Child("CraftContent", size * ImGui.GetIO().FontGlobalScale))
                {
                    if (craftContentChild.Success)
                    {
                        using (var table = ImRaii.Table(Key + "CraftTable", Columns.Count, _tableFlags))
                        {
                            if (!table || !table.Success) return messages;
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

                            if (refresh && !Refreshing)
                            {
                                NeedsRefresh = true;
                            }


                            var overallIndex = 0;
                            var groupedCrafts = CraftGroups;
                            if (groupedCrafts.Count == 0)
                            {
                                ImGui.TableNextRow(ImGuiTableRowFlags.None, 32);
                                for (var columnIndex = 0; columnIndex < Columns.Count; columnIndex++)
                                {
                                    ImGui.TableNextColumn();
                                    if (columnIndex == 1)
                                    {
                                        ImGui.TextWrapped(
                                            "No items have been added to the list. Add items via the search menu button at the top right of the screen or by right clicking on an item anywhere within the plugin.");
                                    }
                                }
                            }

                            foreach (var groupedCraft in groupedCrafts)
                            {
                                if (!groupedCraft.searchResults.Any()) continue;
                                ImGui.TableNextRow(ImGuiTableRowFlags.Headers, FilterConfiguration.TableHeight);
                                ImGui.TableNextColumn();
                                var headerColor = ImRaii.PushColor(ImGuiCol.Header, new Vector4(0, 0, 0, 0));
                                using (var treeNode = ImRaii.TreeNode("##" + groupedCraft.craftGrouping.FormattedName(),
                                           ImGuiTreeNodeFlags.SpanFullWidth | ImGuiTreeNodeFlags.DefaultOpen |
                                           ImGuiTreeNodeFlags.CollapsingHeader))
                                {
                                    headerColor.Pop();
                                    if (Columns.Count >= 2)
                                    {
                                        ImGui.TableNextColumn();
                                        ImGui.TextColored(FilterConfiguration.CraftHeaderColour,
                                            groupedCraft.craftGrouping.FormattedName());
                                    }

                                    if (treeNode.Success)
                                    {
                                        for (var index = 0; index < groupedCraft.searchResults.Count; index++)
                                        {
                                            var item = groupedCraft.searchResults[index];
                                            ImGui.TableNextRow(ImGuiTableRowFlags.None,
                                                FilterConfiguration.TableHeight);
                                            for (var columnIndex = 0; columnIndex < Columns.Count; columnIndex++)
                                            {
                                                var column = Columns[columnIndex];
                                                var columnMessages = column.Column.Draw(FilterConfiguration, column, item, overallIndex, columnIndex);

                                                if (columnMessages != null)
                                                {
                                                    messages.AddRange(columnMessages);
                                                }
                                                if (columnIndex == 0)
                                                {
                                                    ImGui.SameLine();
                                                    var menuMessages = DrawMenu(
                                                        FilterConfiguration, column,
                                                        item,
                                                        overallIndex);
                                                    if (menuMessages != null)
                                                    {
                                                        messages.AddRange(menuMessages);
                                                    }
                                                }
                                            }

                                            overallIndex++;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return messages;
        }

        public int GetCraftListCount()
        {
            return CraftItems.Count(c => !FilterConfiguration.CraftList.HideComplete || !c.CraftItem!.IsCompleted);
        }

        public override void DrawFooterItems()
        {

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
                if (FilterConfiguration.FilterType == FilterType.CraftFilter)
                {
                    foreach (var item in CraftItems)
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
            return ExportToJson(CraftItems);
        }

        public string ExportToJson(List<SearchResult> toExport)
        {
            var lines = new List<dynamic>();
            var converter = new ExpandoObjectConverter();
            if (FilterConfiguration.FilterType == FilterType.CraftFilter)
            {
                foreach (var item in CraftItems)
                {
                    var newLine = new ExpandoObject() as IDictionary<string, Object>;
                    newLine["Id"] = item.Item.ItemId;
                    foreach (var column in Columns)
                    {
                        newLine[column.Column.Name] = column.Column.JsonExport(column, item) ?? "";
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
    }
}