using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using CriticalCommonLib;
using CriticalCommonLib.Crafting;
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
    public class CraftItemTable : RenderTableBase
    {
        public CraftItemTable(FilterConfiguration filterConfiguration) : base(filterConfiguration)
        {
            _tableFlags = ImGuiTableFlags.RowBg | ImGuiTableFlags.BordersV |
                          ImGuiTableFlags.BordersOuterV | ImGuiTableFlags.BordersInnerV |
                          ImGuiTableFlags.BordersH | ImGuiTableFlags.BordersOuterH |
                          ImGuiTableFlags.BordersInnerH |
                          ImGuiTableFlags.Resizable |
                          ImGuiTableFlags.Hideable | ImGuiTableFlags.ScrollX |
                          ImGuiTableFlags.ScrollY;
            filterConfiguration.CraftList.GenerateCraftChildren();
            filterConfiguration.StartRefresh();

        }
        
        public override void RefreshColumns()
        {
            if (FilterConfiguration.FreezeCraftColumns != FreezeCols)
            {
                FreezeCols = FilterConfiguration.FreezeCraftColumns;
            }
            if (FilterConfiguration.CraftColumns != null)
            {
                var newColumns = new List<IColumn>();
                foreach (var column in FilterConfiguration.CraftColumns)
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

        public List<CraftItem> CraftItems = new();

        public override bool Draw(Vector2 size)
        {
            if (Columns.Count == 0)
            {
                if (NeedsRefresh)
                {
                    Refresh(ConfigurationManager.Config);
                }
                return true;
            }

            var isExpanded = false;
            if(ImGui.CollapsingHeader("To Craft", ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.CollapsingHeader))
            {
                isExpanded = true;
                using (var craftContentChild = ImRaii.Child("CraftContent", size * ImGui.GetIO().FontGlobalScale))
                {
                    if (craftContentChild.Success)
                    {
                        if (FilterConfiguration.FilterType == FilterType.CraftFilter)
                        {
                            using (var table = ImRaii.Table(Key + "CraftTable", Columns.Count, _tableFlags))
                            {
                                if (!table || !table.Success) return isExpanded;
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

                                if (refresh || NeedsRefresh)
                                {
                                    Refresh(ConfigurationManager.Config);
                                }


                                var overallIndex = 0;
                                var groupedCrafts = FilterConfiguration.CraftList.GetOutputList();
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
                                    if(!groupedCraft.CraftItems.Any()) continue;
                                    ImGui.TableNextRow(ImGuiTableRowFlags.Headers, FilterConfiguration.TableHeight);
                                    ImGui.TableNextColumn();
                                    var headerColor = ImRaii.PushColor(ImGuiCol.Header, new Vector4(0, 0, 0, 0));
                                    using (var treeNode = ImRaii.TreeNode("##" + groupedCraft.FormattedName(),
                                               ImGuiTreeNodeFlags.SpanFullWidth | ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.CollapsingHeader))
                                    {
                                        headerColor.Pop();
                                        if (Columns.Count >= 2)
                                        {
                                            ImGui.TableNextColumn();
                                            ImGui.TextColored(FilterConfiguration.CraftHeaderColour, groupedCraft.FormattedName());
                                        }

                                        if (treeNode.Success)
                                        {
                                            for (var index = 0; index < groupedCraft.CraftItems.Count; index++)
                                            {
                                                var item = groupedCraft.CraftItems[index];
                                                ImGui.TableNextRow(ImGuiTableRowFlags.None,
                                                    FilterConfiguration.TableHeight);
                                                for (var columnIndex = 0; columnIndex < Columns.Count; columnIndex++)
                                                {
                                                    var column = Columns[columnIndex];
                                                    column.Draw(FilterConfiguration, item, overallIndex);
                                                    ImGui.SameLine();
                                                    if (columnIndex == Columns.Count - 1)
                                                    {
                                                        PluginService.PluginLogic.RightClickColumn.Draw(FilterConfiguration,
                                                            item,
                                                            overallIndex);
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
            }

            return isExpanded;
        }

        private List<CraftItem> GetOutputItemList()
        {
            return CraftItems.Where(c => c.IsOutputItem).ToList();
        }

        public int GetCraftListCount()
        {
            return CraftItems.Count(c => !FilterConfiguration.CraftList.HideComplete || !c.IsCompleted);
        }

        public override void DrawFooterItems()
        {
            
        }

        public override void Refresh(InventoryToolsConfiguration configuration)
        {
            if (FilterConfiguration.FilterResult != null && FilterConfiguration.CraftList.BeenGenerated && FilterConfiguration.CraftList.BeenUpdated)
            {
                PluginLog.Verbose("CraftTable: Refreshing");
                CraftItems = FilterConfiguration.CraftList.GetFlattenedMergedMaterials(true);
                FilterConfiguration.CraftList.ClearGroupCache();
                IsSearching = false;
                NeedsRefresh = false;
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
                if (FilterConfiguration.FilterType == FilterType.CraftFilter)
                {
                    foreach (var item in CraftItems)
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
            if (FilterConfiguration.FilterType == FilterType.CraftFilter)
            {
                foreach (var item in CraftItems)
                {
                    var newLine = new ExpandoObject() as IDictionary<string, Object>;
                    newLine["Id"] = item.ItemId;
                    foreach (var column in Columns)
                    {
                        newLine[column.Name] = column.JsonExport(item) ?? "";
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