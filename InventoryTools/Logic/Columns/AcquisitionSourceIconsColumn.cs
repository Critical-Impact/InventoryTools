using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Interfaces;
using CriticalCommonLib.Models;
using CriticalCommonLib.Models.ItemSources;
using CriticalCommonLib.Services.Mediator;
using CriticalCommonLib.Sheets;
using ImGuiNET;
using InventoryTools.Extensions;
using InventoryTools.Logic.Columns.Abstract;
using OtterGui;
using Dalamud.Interface.Utility.Raii;
using InventoryTools.Mediator;
using InventoryTools.Services;
using InventoryTools.Ui;
using Microsoft.Extensions.Logging;
using IItemSource = CriticalCommonLib.Models.ItemSources.IItemSource;

namespace InventoryTools.Logic.Columns
{
    public class AcquisitionSourceIconsColumn : Column<List<IItemSource>?>
    {
        public AcquisitionSourceIconsColumn(ILogger<AcquisitionSourceIconsColumn> logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }
        public override ColumnCategory ColumnCategory => ColumnCategory.Basic;

        public override List<IItemSource>? CurrentValue(ColumnConfiguration columnConfiguration, SearchResult searchResult)
        {
            return searchResult.Item.Sources;
        }

        public override List<MessageBase>? DoDraw(SearchResult searchResult, List<IItemSource>? currentValue, int rowIndex,
            FilterConfiguration filterConfiguration, ColumnConfiguration columnConfiguration)
        {
            ImGui.TableNextColumn();
            if (!ImGui.TableGetColumnFlags().HasFlag(ImGuiTableColumnFlags.IsEnabled)) return null;
            
            var messages = new List<MessageBase>();
            if (currentValue != null)
            {
                var itemSources = columnConfiguration.FilterText != "" ? currentValue.Where(c => c.FormattedName.ToLower().PassesFilter(columnConfiguration.FilterText)) : currentValue;
                ImGuiService.WrapTableColumnElements("SourceIconContainer" + rowIndex,itemSources, filterConfiguration.TableHeight * ImGui.GetIO().FontGlobalScale - ImGui.GetStyle().FramePadding.X, item =>
                {
                    var sourceIcon = ImGuiService.GetIconTexture(item.Icon);
                    if (item is ItemSource source && source.ItemId != null && source.HasItem && source.Item != null)
                    {
                        if (ImGui.ImageButton(sourceIcon.ImGuiHandle,
                                new Vector2(filterConfiguration.TableHeight, filterConfiguration.TableHeight) *
                                ImGui.GetIO().FontGlobalScale, new Vector2(0, 0), new Vector2(1, 1), 0))
                        {
                            messages.Add(new OpenUintWindowMessage(typeof(ItemWindow), source.ItemId.Value));
                        }

                        if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled &
                                                ImGuiHoveredFlags.AllowWhenOverlapped &
                                                ImGuiHoveredFlags.AllowWhenBlockedByPopup &
                                                ImGuiHoveredFlags.AllowWhenBlockedByActiveItem &
                                                ImGuiHoveredFlags.AnyWindow) &&
                            ImGui.IsMouseReleased(ImGuiMouseButton.Right))
                        {
                            ImGui.OpenPopup("RightClick" + source.ItemId);
                        }

                        using (var popup = ImRaii.Popup("RightClick" + source.ItemId))
                        {
                            if (popup.Success)
                            {
                                if (source.Item != null)
                                {
                                    ImGuiService.RightClickService.DrawRightClickPopup(source.Item, messages);
                                }
                            }
                        }
                    }
                    else if (item is DutySource dutySource)
                    {
                        if (ImGui.ImageButton(sourceIcon.ImGuiHandle,
                                new Vector2(filterConfiguration.TableHeight, filterConfiguration.TableHeight) *
                                ImGui.GetIO().FontGlobalScale, new Vector2(0, 0), new Vector2(1, 1), 0))
                        {
                            messages.Add(new OpenUintWindowMessage(typeof(DutyWindow),
                                dutySource.ContentFinderConditionId));
                        }
                    }
                    else if (item is AirshipSource airshipSource)
                    {
                        if (ImGui.ImageButton(sourceIcon.ImGuiHandle,
                                new Vector2(filterConfiguration.TableHeight, filterConfiguration.TableHeight) *
                                ImGui.GetIO().FontGlobalScale, new Vector2(0, 0), new Vector2(1, 1), 0))
                        {
                            messages.Add(new OpenUintWindowMessage(typeof(AirshipWindow),
                                airshipSource.AirshipExplorationPointExId));
                        }
                    }
                    else if (item is SubmarineSource submarineSource)
                    {
                        if (ImGui.ImageButton(sourceIcon.ImGuiHandle,
                                new Vector2(filterConfiguration.TableHeight, filterConfiguration.TableHeight) *
                                ImGui.GetIO().FontGlobalScale, new Vector2(0, 0), new Vector2(1, 1), 0))
                        {
                            messages.Add(new OpenUintWindowMessage(typeof(SubmarineWindow),
                                submarineSource.SubmarineExplorationExId));
                        }
                    }
                    else if (item is VentureSource ventureSource)
                    {
                        if (ImGui.ImageButton(sourceIcon.ImGuiHandle,
                                new Vector2(filterConfiguration.TableHeight, filterConfiguration.TableHeight) *
                                ImGui.GetIO().FontGlobalScale, new Vector2(0, 0), new Vector2(1, 1), 0))
                        {
                            messages.Add(new OpenUintWindowMessage(typeof(RetainerTaskWindow),
                                ventureSource.RetainerTask.RowId));
                        }
                    }
                    else
                    {
                        if (ImGui.ImageButton(sourceIcon.ImGuiHandle,
                                new Vector2(filterConfiguration.TableHeight, filterConfiguration.TableHeight) *
                                ImGui.GetIO().FontGlobalScale, new Vector2(0, 0), new Vector2(1, 1), 0))
                        {

                        }
                    }

                    ImGuiUtil.HoverTooltip(item.FormattedName);

                    return true;
                });
            }
            return null;
        }

        public override void Setup(FilterConfiguration filterConfiguration, ColumnConfiguration columnConfiguration,
            int columnIndex)
        {
            ImGui.TableSetupColumn(RenderName ?? Name, ImGuiTableColumnFlags.WidthFixed, Width, (uint)columnIndex);
        }

        public override string CsvExport(ColumnConfiguration columnConfiguration, SearchResult searchResult)
        {
            return String.Join(", ", searchResult.Item.Sources.Select(c => c.Name));
        }
        
        public override dynamic? JsonExport(ColumnConfiguration columnConfiguration, SearchResult searchResult)
        {
            return String.Join(", ", searchResult.Item.Sources.Select(c => c.Name));
        }

        public override string Name { get; set; } = "Acqusition";
        public override float Width { get; set; } = 250;

        public override string HelpText { get; set; } =
            "Shows icons indicating what items can be obtained with(gathering, crafting, currency, etc)";
        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
        public override IEnumerable<SearchResult> Filter(ColumnConfiguration columnConfiguration,
            IEnumerable<SearchResult> searchResults)
        {
            return columnConfiguration.FilterText == "" ? searchResults : searchResults.Where(c =>
            {
                var currentValue = CurrentValue(columnConfiguration, c);
                if (currentValue == null)
                {
                    return false;
                }

                return currentValue.Any(c => c.FormattedName.ToLower().PassesFilter(columnConfiguration.FilterComparisonText));
            });
        }

        public override IEnumerable<SearchResult> Sort(ColumnConfiguration columnConfiguration,
            ImGuiSortDirection direction, IEnumerable<SearchResult> items)
        {
            return direction == ImGuiSortDirection.Ascending ? items.OrderBy(item =>
            {
                var currentValue = CurrentValue(columnConfiguration, item);
                return currentValue?.Count ?? 0;
            }) : items.OrderByDescending(item =>
            {
                var currentValue = CurrentValue(columnConfiguration, item);
                return currentValue?.Count ?? 0;
            });
        }

        public override List<MessageBase>? Draw(FilterConfiguration configuration,
            ColumnConfiguration columnConfiguration,
            SearchResult searchResult, int rowIndex, int columnIndex)
        {
            return DoDraw(searchResult, CurrentValue(columnConfiguration, searchResult), rowIndex, configuration, columnConfiguration);
        }
        
        public override FilterType DefaultIn => Logic.FilterType.GameItemFilter | Logic.FilterType.CraftFilter;
        
    }
}