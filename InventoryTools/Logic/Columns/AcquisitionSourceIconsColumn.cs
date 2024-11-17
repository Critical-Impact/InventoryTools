using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using AllaganLib.GameSheets.ItemSources;
using AllaganLib.Shared.Extensions;
using CriticalCommonLib.Services.Mediator;

using ImGuiNET;
using InventoryTools.Extensions;
using InventoryTools.Logic.Columns.Abstract;
using OtterGui;
using Dalamud.Interface.Utility.Raii;
using InventoryTools.Mediator;
using InventoryTools.Services;
using InventoryTools.Ui;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns
{
    public class AcquisitionSourceIconsColumn : Column<List<List<ItemSource>>?>
    {
        private readonly ItemInfoRenderer _itemInfoRenderer;

        public AcquisitionSourceIconsColumn(ILogger<AcquisitionSourceIconsColumn> logger, ImGuiService imGuiService, ItemInfoRenderer itemInfoRenderer) : base(logger, imGuiService)
        {
            _itemInfoRenderer = itemInfoRenderer;
        }
        public override ColumnCategory ColumnCategory => ColumnCategory.Basic;

        public override List<List<ItemSource>>? CurrentValue(ColumnConfiguration columnConfiguration, SearchResult searchResult)
        {
            List<List<ItemSource>> groupedItemSources = new List<List<ItemSource>>();
            foreach (var itemSourceGroup in searchResult.Item.Sources.GroupBy(c => c.Type))
            {
                if (_itemInfoRenderer.ShouldGroupSource(itemSourceGroup.Key))
                {
                    groupedItemSources.Add(itemSourceGroup.ToList());
                }
                else
                {
                    groupedItemSources.AddRange(itemSourceGroup.Select(c => new List<ItemSource>(){c}));
                }
            }
            return groupedItemSources;
        }

        public override List<MessageBase>? DoDraw(SearchResult searchResult, List<List<ItemSource>>? currentValue, int rowIndex,
            FilterConfiguration filterConfiguration, ColumnConfiguration columnConfiguration)
        {
            ImGui.TableNextColumn();
            if (!ImGui.TableGetColumnFlags().HasFlag(ImGuiTableColumnFlags.IsEnabled)) return null;

            using var foo = ImRaii.PushId(rowIndex);
            var messages = new List<MessageBase>();
            if (currentValue != null)
            {
                var itemSources = columnConfiguration.FilterText != "" ? currentValue.Where(c => c.Any(d => _itemInfoRenderer.GetSourceTypeName(d.Type).Singular.ToLower().PassesFilterComparisonText(columnConfiguration.FilterComparisonText) ||  _itemInfoRenderer.RenderSourceName(d).ToLower().PassesFilterComparisonText(columnConfiguration.FilterComparisonText))) : currentValue;

                ImGuiService.WrapTableColumnElements("SourceIconContainer" + rowIndex,itemSources, filterConfiguration.TableHeight * ImGui.GetIO().FontGlobalScale - ImGui.GetStyle().FramePadding.X, itemList =>
                {
                    var item = itemList.First();
                    var sourceIcon = ImGuiService.GetIconTexture(_itemInfoRenderer.RenderSourceIcon(item));
                    if (item is ItemDungeonSource dungeonSource)
                    {
                        if (ImGui.ImageButton(sourceIcon.ImGuiHandle,
                                new Vector2(filterConfiguration.TableHeight, filterConfiguration.TableHeight) *
                                ImGui.GetIO().FontGlobalScale, new Vector2(0, 0), new Vector2(1, 1), 0))
                        {
                            messages.Add(new OpenUintWindowMessage(typeof(DutyWindow), dungeonSource.ContentFinderCondition.RowId));
                        }
                    }
                    else if (item is ItemAirshipDropSource airshipSource)
                    {
                        if (ImGui.ImageButton(sourceIcon.ImGuiHandle,
                                new Vector2(filterConfiguration.TableHeight, filterConfiguration.TableHeight) *
                                ImGui.GetIO().FontGlobalScale, new Vector2(0, 0), new Vector2(1, 1), 0))
                        {
                            messages.Add(new OpenUintWindowMessage(typeof(AirshipWindow),
                                airshipSource.AirshipExplorationPoint.RowId));
                        }
                    }
                    else if (item is ItemSubmarineDropSource submarineSource)
                    {
                        if (ImGui.ImageButton(sourceIcon.ImGuiHandle,
                                new Vector2(filterConfiguration.TableHeight, filterConfiguration.TableHeight) *
                                ImGui.GetIO().FontGlobalScale, new Vector2(0, 0), new Vector2(1, 1), 0))
                        {
                            messages.Add(new OpenUintWindowMessage(typeof(SubmarineWindow),
                                submarineSource.SubmarineExploration.RowId));
                        }
                    }
                    else if (item is ItemVentureSource ventureSource)
                    {
                        if (ImGui.ImageButton(sourceIcon.ImGuiHandle,
                                new Vector2(filterConfiguration.TableHeight, filterConfiguration.TableHeight) *
                                ImGui.GetIO().FontGlobalScale, new Vector2(0, 0), new Vector2(1, 1), 0))
                        {
                            messages.Add(new OpenUintWindowMessage(typeof(RetainerTaskWindow),
                                ventureSource.RetainerTaskRow.RowId));
                        }
                    }
                    else if (item is ItemExplorationVentureSource explorationVentureSource)
                    {
                        if (ImGui.ImageButton(sourceIcon.ImGuiHandle,
                                new Vector2(filterConfiguration.TableHeight, filterConfiguration.TableHeight) *
                                ImGui.GetIO().FontGlobalScale, new Vector2(0, 0), new Vector2(1, 1), 0))
                        {
                            messages.Add(new OpenUintWindowMessage(typeof(RetainerTaskWindow),
                                explorationVentureSource.RetainerTaskRow.RowId));
                        }
                    }
                    else if (item is ItemSpecialShopSource specialShopSource)
                    {
                        if (ImGui.ImageButton(sourceIcon.ImGuiHandle,
                                new Vector2(filterConfiguration.TableHeight, filterConfiguration.TableHeight) *
                                ImGui.GetIO().FontGlobalScale, new Vector2(0, 0), new Vector2(1, 1), 0))
                        {
                            if (specialShopSource.CostItems.Count != 0)
                            {
                                messages.Add(new OpenUintWindowMessage(typeof(ItemWindow),
                                    specialShopSource.CostItems.First().RowId));
                            }
                        }
                    }
                    else if (item.CostItem != null)
                    {
                        if (ImGui.ImageButton(sourceIcon.ImGuiHandle,
                                new Vector2(filterConfiguration.TableHeight, filterConfiguration.TableHeight) *
                                ImGui.GetIO().FontGlobalScale, new Vector2(0, 0), new Vector2(1, 1), 0))
                        {
                            messages.Add(new OpenUintWindowMessage(typeof(ItemWindow), item.CostItem.RowId));
                        }

                        if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled &
                                                ImGuiHoveredFlags.AllowWhenOverlapped &
                                                ImGuiHoveredFlags.AllowWhenBlockedByPopup &
                                                ImGuiHoveredFlags.AllowWhenBlockedByActiveItem &
                                                ImGuiHoveredFlags.AnyWindow) &&
                            ImGui.IsMouseReleased(ImGuiMouseButton.Right))
                        {
                            ImGui.OpenPopup("RightClick" + item.CostItem.RowId);
                        }

                        using (var popup = ImRaii.Popup("RightClick" + item.CostItem.RowId))
                        {
                            if (popup.Success)
                            {
                                if (item.CostItem != null)
                                {
                                    ImGuiService.RightClickService.DrawRightClickPopup(item.Item, messages);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (ImGui.ImageButton(sourceIcon.ImGuiHandle,
                                new Vector2(filterConfiguration.TableHeight, filterConfiguration.TableHeight) *
                                ImGui.GetIO().FontGlobalScale, new Vector2(0, 0), new Vector2(1, 1), 0))
                        {
                            if (item.CostItem != null)
                            {
                                messages.Add(new OpenUintWindowMessage(typeof(ItemWindow), item.CostItem.RowId));
                            }
                        }
                    }

                    if (ImGui.IsItemHovered())
                    {
                        using var tt = ImRaii.Tooltip();
                        _itemInfoRenderer.DrawSource(itemList);
                    }

                    return true;
                });
            }
            return messages;
        }

        public override void Setup(FilterConfiguration filterConfiguration, ColumnConfiguration columnConfiguration,
            int columnIndex)
        {
            ImGui.TableSetupColumn(RenderName ?? Name, ImGuiTableColumnFlags.WidthFixed, Width, (uint)columnIndex);
        }

        public override string CsvExport(ColumnConfiguration columnConfiguration, SearchResult searchResult)
        {
            return String.Join(", ", searchResult.Item.Sources.Select(c => _itemInfoRenderer.RenderSourceName(c)));
        }

        public override dynamic? JsonExport(ColumnConfiguration columnConfiguration, SearchResult searchResult)
        {
            return String.Join(", ", searchResult.Item.Sources.Select(c => _itemInfoRenderer.RenderSourceName(c)));
        }

        public override string Name { get; set; } = "Acquisition";
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

                return currentValue.SelectMany(d => d).Any(e => _itemInfoRenderer.GetSourceTypeName(e.Type).Singular.ToLower().PassesFilterComparisonText(columnConfiguration.FilterComparisonText) || _itemInfoRenderer.RenderSourceName(e).ToLower().PassesFilterComparisonText(columnConfiguration.FilterComparisonText));
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