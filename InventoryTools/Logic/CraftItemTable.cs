using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using CriticalCommonLib.Crafting;
using Dalamud.Logging;
using ImGuiNET;
using InventoryTools.Logic.Columns;
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
                using (ImRaii.Child("CraftContent", size * ImGui.GetIO().FontGlobalScale))
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


                            if (refresh || NeedsRefresh)
                            {
                                Refresh(ConfigurationManager.Config);
                            }

                            //Make the visibility of zero quantity required items a toggle
                            var outputs = GetOutputItemList();
                            var preCrafts = GetPrecraftItemList();
                            var everythingElse = GetRemainingItemList();

                            ImGui.TableNextRow(ImGuiTableRowFlags.Headers, FilterConfiguration.TableHeight);
                            ImGui.TableNextColumn();
                            var overallIndex = 0;

                            using (var treeNode = ImRaii.TreeNode("Crafts:",
                                       ImGuiTreeNodeFlags.SpanFullWidth | ImGuiTreeNodeFlags.DefaultOpen))
                            {
                                if (treeNode.Success)
                                {
                                    if (outputs.Count == 0)
                                    {
                                        ImGui.TableNextRow(ImGuiTableRowFlags.None, 32);
                                        for (var columnIndex = 0; columnIndex < Columns.Count; columnIndex++)
                                        {
                                            ImGui.TableNextColumn();
                                            if (columnIndex == 1)
                                            {
                                                ImGui.Text(
                                                    "No items have been added to the list. Add items from the top right or by right clicking on an item anywhere within the plugin.");
                                            }
                                        }
                                    }

                                    for (var index = 0; index < outputs.Count; index++)
                                    {
                                        var item = outputs[index];
                                        ImGui.TableNextRow(ImGuiTableRowFlags.None, FilterConfiguration.TableHeight);
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

                            ImGui.TableNextRow(ImGuiTableRowFlags.Headers, FilterConfiguration.TableHeight);
                            ImGui.TableNextColumn();
                            using (var treeNode = ImRaii.TreeNode("Precrafts:", ImGuiTreeNodeFlags.SpanFullWidth | ImGuiTreeNodeFlags.DefaultOpen))
                            {
                                for (var columnIndex = 0; columnIndex < Columns.Count; columnIndex++)
                                {
                                    if (columnIndex != 0)
                                    {
                                        ImGui.TableNextColumn();
                                    }
                                }

                                if (treeNode.Success)
                                {
                                    for (var index = 0; index < preCrafts.Count; index++)
                                    {
                                        var item = preCrafts[index];
                                        ImGui.TableNextRow(ImGuiTableRowFlags.None, FilterConfiguration.TableHeight);
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

                            ImGui.TableNextRow(ImGuiTableRowFlags.Headers, FilterConfiguration.TableHeight);
                            ImGui.TableNextColumn();
                            using (var treeNode = ImRaii.TreeNode("Gather/Buy:",
                                       ImGuiTreeNodeFlags.SpanFullWidth | ImGuiTreeNodeFlags.DefaultOpen))
                            {
                                for (var columnIndex = 0; columnIndex < Columns.Count; columnIndex++)
                                {
                                    if (columnIndex != 0)
                                    {
                                        ImGui.TableNextColumn();
                                    }
                                }

                                if (treeNode.Success)
                                {
                                    for (var index = 0; index < everythingElse.Count; index++)
                                    {
                                        var item = everythingElse[index];
                                        ImGui.TableNextRow(ImGuiTableRowFlags.None, FilterConfiguration.TableHeight);
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

            return isExpanded;
        }

        private List<CraftItem> GetRemainingItemList()
        {
            var craftItems = CraftItems.Where(c => c.QuantityNeeded != 0 && !c.Item.CanBeCrafted);
            if (FilterConfiguration.HideCompletedRows)
            {
                craftItems = craftItems.Where(c => c.QuantityMissing != 0);
            }
            return craftItems.OrderByDescending(c => c.ItemId).ToList();
        }

        private List<CraftItem> GetPrecraftItemList()
        {


            var craftItems = CraftItems.Where(c => c.QuantityNeeded != 0 && !c.IsOutputItem && c.Item.CanBeCrafted);
            if (FilterConfiguration.HideCompletedRows)
            {
                craftItems = craftItems.Where(c => c.QuantityMissing != 0);
            }
            return craftItems.ToList();
        }

        private List<CraftItem> GetOutputItemList()
        {
            return CraftItems.Where(c => c.IsOutputItem).ToList();
        }

        public override void DrawFooterItems()
        {
            
        }

        public override void Refresh(InventoryToolsConfiguration configuration)
        {
            if (FilterConfiguration.FilterResult != null)
            {
                PluginLog.Verbose("CraftTable: Refreshing");
                CraftItems = FilterConfiguration.CraftList.GetFlattenedMergedMaterials();
                IsSearching = false;
                NeedsRefresh = false;
            }
        }
    }
}