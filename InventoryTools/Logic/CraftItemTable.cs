using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using CriticalCommonLib.Crafting;
using Dalamud.Logging;
using ImGuiNET;
using InventoryTools.Logic.Columns;

namespace InventoryTools.Logic
{
    public class CraftItemTable : RenderTableBase
    {
        public CraftItemTable(FilterConfiguration filterConfiguration) : base(filterConfiguration)
        {
            filterConfiguration.CraftList.GenerateCraftChildren();
            filterConfiguration.StartRefresh();

        }
        
        public override void RefreshColumns()
        {
            FreezeCols = 2;
            PluginLog.Verbose("Refreshing craft item table columns");

            var newColumns = new List<IColumn>();
            newColumns.Add(new IconColumn());
            newColumns.Add(new NameColumn());
            if (FilterConfiguration.SimpleCraftingMode == true)
            {
                newColumns.Add(new CraftAmountRequiredColumn());
                newColumns.Add(new CraftSimpleColumn());
            }
            else
            {
                newColumns.Add(new QuantityAvailableColumn());
                newColumns.Add(new CraftAmountRequiredColumn());
                newColumns.Add(new CraftAmountReadyColumn());
                newColumns.Add(new CraftAmountAvailableColumn());
                newColumns.Add(new CraftAmountUnavailableColumn());
                newColumns.Add(new CraftAmountCanCraftColumn());
            }

            newColumns.Add(new MarketBoardMinPriceColumn());
            newColumns.Add(new MarketBoardMinTotalPriceColumn());
            newColumns.Add(new AcquisitionSourceIconsColumn());
            newColumns.Add(new CraftGatherColumn());
            this.Columns = newColumns;
        }

        public List<CraftItem> CraftItems = new();

        public override void Draw(Vector2 size)
        {
            if (Columns.Count == 0)
            {
                if (NeedsRefresh)
                {
                    Refresh(ConfigurationManager.Config);
                }
                return;
            }
            if(ImGui.CollapsingHeader("To Craft", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.BeginChild("CraftContent", size * ImGui.GetIO().FontGlobalScale);
                if (FilterConfiguration.FilterType == FilterType.CraftFilter &&
                    ImGui.BeginTable(Key + "CraftTable", Columns.Count, _tableFlags))
                {
                    var refresh = false;
                    ImGui.TableSetupScrollFreeze(Math.Min(FreezeCols ?? 0, Columns.Count),
                        FreezeRows ?? (ShowFilterRow ? 2 : 1));
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
                        SortColumn = null;
                        SortDirection = null;
                        refresh = true;
                    }

                    if (refresh || NeedsRefresh)
                    {
                        Refresh(ConfigurationManager.Config);
                    }

                    //Make the visibility of zero quantity required items a toggle
                    var outputs = CraftItems.Where(c => c.IsOutputItem).ToList();
                    var preCrafts = CraftItems.Where(c => c.QuantityNeeded != 0 && !c.IsOutputItem && c.Item.CanBeCrafted).OrderByDescending(c => c.ItemId).ToList();
                    var everythingElse = CraftItems.Where(c => c.QuantityNeeded != 0 && !c.Item.CanBeCrafted).OrderByDescending(c => c.ItemId).ToList();

                    var overallIndex = 0;
                    if (outputs.Count == 0)
                    {
                        ImGui.TableNextRow(ImGuiTableRowFlags.None, 32);
                        for (var columnIndex = 0; columnIndex < Columns.Count; columnIndex++)
                        {
                            ImGui.TableNextColumn();
                            if (columnIndex == 1)
                            {
                                ImGui.Text("No items have been selected.");
                            }
                        }
                    }
                    for (var index = 0; index < outputs.Count; index++)
                    {
                        overallIndex++;
                        var item = outputs[index];
                        ImGui.TableNextRow(ImGuiTableRowFlags.None, 32);
                        for (var columnIndex = 0; columnIndex < Columns.Count; columnIndex++)
                        {
                            var column = Columns[columnIndex];
                            column.Draw(FilterConfiguration, item, index);
                            ImGui.SameLine();
                            if (columnIndex == Columns.Count - 1)
                            {
                                PluginService.PluginLogic.RightClickColumn.Draw(FilterConfiguration, item, overallIndex);
                            }
                        }
                    }

                    ImGui.TableNextRow(ImGuiTableRowFlags.Headers, 32);
                    ImGui.TableNextColumn();
                    ImGui.TableNextColumn();
                    ImGui.Text("Precrafts:");
                    for (var index = 0; index < preCrafts.Count; index++)
                    {
                        overallIndex++;
                        var item = preCrafts[index];
                        ImGui.TableNextRow(ImGuiTableRowFlags.None, 32);
                        for (var columnIndex = 0; columnIndex < Columns.Count; columnIndex++)
                        {
                            var column = Columns[columnIndex];
                            column.Draw(FilterConfiguration, item, index);
                            ImGui.SameLine();
                            if (columnIndex == Columns.Count - 1)
                            {
                                PluginService.PluginLogic.RightClickColumn.Draw(FilterConfiguration, item, overallIndex);
                            }
                        }
                    }

                    ImGui.TableNextRow(ImGuiTableRowFlags.Headers, 32);
                    ImGui.TableNextColumn();
                    ImGui.TableNextColumn();
                    ImGui.Text("Gather/Buy:");
                    for (var index = 0; index < everythingElse.Count; index++)
                    {
                        overallIndex++;
                        var item = everythingElse[index];
                        ImGui.TableNextRow(ImGuiTableRowFlags.None, 32);
                        for (var columnIndex = 0; columnIndex < Columns.Count; columnIndex++)
                        {
                            var column = Columns[columnIndex];
                            column.Draw(FilterConfiguration, item, index);
                            ImGui.SameLine();
                            if (columnIndex == Columns.Count - 1)
                            {
                                PluginService.PluginLogic.RightClickColumn.Draw(FilterConfiguration, item, overallIndex);
                            }
                        }
                    }

                    ImGui.EndTable();
                }
                ImGui.EndChild();
            }
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
        
        public override void Dispose()
        {
            FilterConfiguration.ConfigurationChanged -= FilterConfigurationUpdated;
            FilterConfiguration.ListUpdated -= FilterConfigurationUpdated;
            FilterConfiguration.TableConfigurationChanged -= FilterConfigurationOnTableConfigurationChanged;
        }
    }
}