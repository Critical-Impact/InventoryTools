using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Extensions;
using CriticalCommonLib.Services;
using Dalamud.Logging;
using ImGuiNET;
using InventoryTools.Logic.Columns;

namespace InventoryTools.Logic
{
    public class CraftItemTable : RenderTableBase
    {
        public CraftItemTable(FilterConfiguration filterConfiguration) : base(filterConfiguration)
        {
        }
        
        public override void RefreshColumns()
        {
            FreezeCols = 1;

            var newColumns = new List<IColumn>();
            newColumns.Add(new IconColumn());
            newColumns.Add(new NameColumn());
            newColumns.Add(new CraftAmountRequiredColumn());
            newColumns.Add(new MarketBoardMinPriceColumn());
            newColumns.Add(new MarketBoardMinTotalPriceColumn());
            this.Columns = newColumns;
        }

        public List<CraftItem> CraftItems = new();

        public override void Draw()
        {
            if (Columns.Count == 0)
            {
                if (NeedsRefresh)
                {
                    Refresh(ConfigurationManager.Config);
                }
                return;
            }
            ImGui.BeginChild("CraftContent", new Vector2(0, 400)* ImGui.GetIO().FontGlobalScale); 
            if (FilterConfiguration.FilterType == FilterType.CraftFilter && ImGui.BeginTable(Key + "CraftTable", Columns.Count, _tableFlags))
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

                if (refresh || NeedsRefresh)
                {
                    Refresh(ConfigurationManager.Config);
                }

                var outputs = CraftItems.Where(c => c.IsOutputItem).ToList();
                var preCrafts = CraftItems.Where(c => !c.IsOutputItem && (c.Item?.CanBeCrafted() ?? false)).ToList();
                var everythingElse = CraftItems.Where(c => !(c.Item?.CanBeCrafted() ?? true)).ToList();
                
                ImGui.TableNextRow(ImGuiTableRowFlags.None, 32);
                ImGui.TableNextColumn();
                ImGui.Text("Outputs:");
                var overallIndex = 0;
                for (var index = 0; index < outputs.Count; index++)
                {
                    overallIndex++;
                    var item = outputs[index];
                    ImGui.TableNextRow(ImGuiTableRowFlags.None, 32);
                    for (var columnIndex = 0; columnIndex < Columns.Count; columnIndex++)
                    {
                        var column = Columns[columnIndex];
                        column.Draw(item, index, FilterConfiguration);
                        ImGui.SameLine();
                        if (columnIndex == Columns.Count - 1)
                        {
                            PluginService.PluginLogic.RightClickColumn.Draw(item, overallIndex, FilterConfiguration);
                        }
                    }
                }
                
                ImGui.TableNextRow(ImGuiTableRowFlags.None, 32);
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
                        column.Draw(item, index, FilterConfiguration);
                        ImGui.SameLine();
                        if (columnIndex == Columns.Count - 1)
                        {
                            PluginService.PluginLogic.RightClickColumn.Draw(item, overallIndex, FilterConfiguration);
                        }
                    }
                }
                
                ImGui.TableNextRow(ImGuiTableRowFlags.None, 32);
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
                        column.Draw(item, index, FilterConfiguration);
                        ImGui.SameLine();
                        if (columnIndex == Columns.Count - 1)
                        {
                            PluginService.PluginLogic.RightClickColumn.Draw(item, overallIndex, FilterConfiguration);
                        }
                    }
                }
                ImGui.EndTable();
            }
            else
            {
                ImGui.Text("You shouldn't see me.");
            }
            ImGui.EndChild();
        }

        public override void Refresh(InventoryToolsConfiguration configuration)
        {
            if (FilterConfiguration.FilterResult != null)
            {
                PluginLog.Verbose("CraftTable: Refreshing");
                CraftItems = FilterConfiguration.CraftList.FlattenedMaterials;
                IsSearching = false;
                NeedsRefresh = false;
            }
        }
    }
}