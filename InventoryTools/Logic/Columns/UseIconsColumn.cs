using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using AllaganLib.GameSheets.Caches;
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
    public class UseIconsColumn : Column<List<List<ItemSource>>?>
    {
        private readonly ItemInfoRenderer _itemInfoRenderer;

        public UseIconsColumn(ILogger<UseIconsColumn> logger, ImGuiService imGuiService, ItemInfoRenderer itemInfoRenderer) : base(logger, imGuiService)
        {
            _itemInfoRenderer = itemInfoRenderer;
        }
        public override ColumnCategory ColumnCategory => ColumnCategory.Basic;

        public override List<List<ItemSource>>? CurrentValue(ColumnConfiguration columnConfiguration, SearchResult searchResult)
        {
            List<List<ItemSource>> groupedItemUses = new List<List<ItemSource>>();
            foreach (var itemUseGroup in searchResult.Item.Uses.GroupBy(c => c.Type))
            {
                if (_itemInfoRenderer.ShouldGroupUse(itemUseGroup.Key))
                {
                    groupedItemUses.Add(itemUseGroup.ToList());
                }
                else
                {
                    if (itemUseGroup.Key == ItemInfoType.CraftRecipe)
                    {
                        var groupedCrafts = itemUseGroup.Cast<ItemCraftRequirementSource>().GroupBy(c => c.Recipe.CraftType!.RowId);
                        foreach (var groupedCraft in groupedCrafts)
                        {
                            groupedItemUses.Add(groupedCraft.Cast<ItemSource>().ToList());
                        }
                    }
                    else
                    {
                        groupedItemUses.AddRange(itemUseGroup.Select(c => new List<ItemSource>(){c}));
                    }
                }
            }
            return groupedItemUses;
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
                var itemUses = columnConfiguration.FilterText != "" ? currentValue.Where(c => c.Any(d => _itemInfoRenderer.GetSourceTypeName(d.Type).Singular.ToLower().PassesFilterComparisonText(columnConfiguration.FilterComparisonText) ||  _itemInfoRenderer.RenderUseName(d).ToLower().PassesFilterComparisonText(columnConfiguration.FilterComparisonText))) : currentValue;

                ImGuiService.WrapTableColumnElements("UseIconContainer" + rowIndex,itemUses, filterConfiguration.TableHeight * ImGui.GetIO().FontGlobalScale - ImGui.GetStyle().FramePadding.X, itemList =>
                {
                    var item = itemList.First();
                    var useIcon = ImGuiService.GetIconTexture(_itemInfoRenderer.RenderUseIcon(item));
                    if (item is ItemDungeonSource dungeonUse)
                    {
                        if (ImGui.ImageButton(useIcon.ImGuiHandle,
                                new Vector2(filterConfiguration.TableHeight, filterConfiguration.TableHeight) *
                                ImGui.GetIO().FontGlobalScale, new Vector2(0, 0), new Vector2(1, 1), 0))
                        {
                            messages.Add(new OpenUintWindowMessage(typeof(DutyWindow), dungeonUse.ContentFinderCondition.RowId));
                        }
                    }
                    else if (item is ItemAirshipDropSource airshipUse)
                    {
                        if (ImGui.ImageButton(useIcon.ImGuiHandle,
                                new Vector2(filterConfiguration.TableHeight, filterConfiguration.TableHeight) *
                                ImGui.GetIO().FontGlobalScale, new Vector2(0, 0), new Vector2(1, 1), 0))
                        {
                            messages.Add(new OpenUintWindowMessage(typeof(AirshipWindow),
                                airshipUse.AirshipExplorationPoint.RowId));
                        }
                    }
                    else if (item is ItemSubmarineDropSource submarineUse)
                    {
                        if (ImGui.ImageButton(useIcon.ImGuiHandle,
                                new Vector2(filterConfiguration.TableHeight, filterConfiguration.TableHeight) *
                                ImGui.GetIO().FontGlobalScale, new Vector2(0, 0), new Vector2(1, 1), 0))
                        {
                            messages.Add(new OpenUintWindowMessage(typeof(SubmarineWindow),
                                submarineUse.SubmarineExploration.RowId));
                        }
                    }
                    else if (item is ItemVentureSource ventureUse)
                    {
                        if (ImGui.ImageButton(useIcon.ImGuiHandle,
                                new Vector2(filterConfiguration.TableHeight, filterConfiguration.TableHeight) *
                                ImGui.GetIO().FontGlobalScale, new Vector2(0, 0), new Vector2(1, 1), 0))
                        {
                            messages.Add(new OpenUintWindowMessage(typeof(RetainerTaskWindow),
                                ventureUse.RetainerTaskRow.RowId));
                        }
                    }
                    else if (item is ItemExplorationVentureSource explorationVentureUse)
                    {
                        if (ImGui.ImageButton(useIcon.ImGuiHandle,
                                new Vector2(filterConfiguration.TableHeight, filterConfiguration.TableHeight) *
                                ImGui.GetIO().FontGlobalScale, new Vector2(0, 0), new Vector2(1, 1), 0))
                        {
                            messages.Add(new OpenUintWindowMessage(typeof(RetainerTaskWindow),
                                explorationVentureUse.RetainerTaskRow.RowId));
                        }
                    }
                    else
                    {
                        if (ImGui.ImageButton(useIcon.ImGuiHandle,
                                new Vector2(filterConfiguration.TableHeight, filterConfiguration.TableHeight) *
                                ImGui.GetIO().FontGlobalScale, new Vector2(0, 0), new Vector2(1, 1), 0))
                        {
                            messages.Add(new OpenUintWindowMessage(typeof(ItemWindow), item.Item.RowId));
                        }

                        if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled &
                                                ImGuiHoveredFlags.AllowWhenOverlapped &
                                                ImGuiHoveredFlags.AllowWhenBlockedByPopup &
                                                ImGuiHoveredFlags.AllowWhenBlockedByActiveItem &
                                                ImGuiHoveredFlags.AnyWindow) &&
                            ImGui.IsMouseReleased(ImGuiMouseButton.Right))
                        {
                            ImGui.OpenPopup("RightClick" + item.Item.RowId);
                        }

                        using (var popup = ImRaii.Popup("RightClick" + item.Item.RowId))
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
                    // else
                    // {
                    //     if (ImGui.ImageButton(useIcon.ImGuiHandle,
                    //             new Vector2(filterConfiguration.TableHeight, filterConfiguration.TableHeight) *
                    //             ImGui.GetIO().FontGlobalScale, new Vector2(0, 0), new Vector2(1, 1), 0))
                    //     {
                    //
                    //     }
                    // }

                    if (ImGui.IsItemHovered())
                    {
                        using var tt = ImRaii.Tooltip();
                        _itemInfoRenderer.DrawUse(itemList);
                    }

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
            return String.Join(", ", searchResult.Item.Uses.Select(c => _itemInfoRenderer.RenderUseName(c)));
        }

        public override dynamic? JsonExport(ColumnConfiguration columnConfiguration, SearchResult searchResult)
        {
            return String.Join(", ", searchResult.Item.Uses.Select(c => _itemInfoRenderer.RenderUseName(c)));
        }

        public override float Width { get; set; } = 250;
        public override string Name { get; set; } = "Uses";

        public override string HelpText { get; set; } =
            "Shows icons indicating what the items drop/can be used for";
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

                return currentValue.SelectMany(d => d).Any(e => _itemInfoRenderer.GetSourceTypeName(e.Type).Singular.ToLower().PassesFilterComparisonText(columnConfiguration.FilterComparisonText) || _itemInfoRenderer.RenderUseName(e).ToLower().PassesFilterComparisonText(columnConfiguration.FilterComparisonText));
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