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
using Dalamud.Plugin.Services;
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
        public override List<IItemSource>? CurrentValue(ColumnConfiguration columnConfiguration, InventoryItem item)
        {
            return CurrentValue(columnConfiguration, item.Item);
        }

        public override List<IItemSource>? CurrentValue(ColumnConfiguration columnConfiguration, ItemEx item)
        {
            return item.Sources;
        }

        public override List<IItemSource>? CurrentValue(ColumnConfiguration columnConfiguration, SortingResult item)
        {
            return CurrentValue(columnConfiguration, item.InventoryItem);
        }

        public override List<IItemSource>? CurrentValue(ColumnConfiguration columnConfiguration, CraftItem item)
        {
            return item.Item.Sources;
        }
        
        public override List<IItemSource>? CurrentValue(ColumnConfiguration columnConfiguration,
            InventoryChange currentValue)
        {
            return CurrentValue(columnConfiguration, currentValue.InventoryItem);
        }

        public override List<MessageBase>? DoDraw(IItem item1, List<IItemSource>? currentValue, int rowIndex,
            FilterConfiguration filterConfiguration, ColumnConfiguration columnConfiguration)
        {
            var messages = new List<MessageBase>();
            ImGui.TableNextColumn();
            if (currentValue != null)
            {
                var itemSources = FilterText != "" ? currentValue.Where(c => c.FormattedName.ToLower().PassesFilter(FilterText)) : currentValue;
                ImGuiService.WrapTableColumnElements("SourceIconContainer" + rowIndex,itemSources, filterConfiguration.TableHeight * ImGui.GetIO().FontGlobalScale - ImGui.GetStyle().FramePadding.X, item =>
                {
                    var sourceIcon = ImGuiService.IconService[item.Icon];
                    if (sourceIcon != null)
                    {
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


        public override string CsvExport(ColumnConfiguration columnConfiguration, InventoryItem item)
        {
            return CsvExport(columnConfiguration, item.Item);
        }

        public override string CsvExport(ColumnConfiguration columnConfiguration, SortingResult item)
        {
            return CsvExport(columnConfiguration, item.InventoryItem);
        }

        public override string CsvExport(ColumnConfiguration columnConfiguration, ItemEx item)
        {
            return String.Join(", ", item.Sources.Select(c => c.Name));
        }
        
        public override dynamic? JsonExport(ColumnConfiguration columnConfiguration, ItemEx item)
        {
            return String.Join(", ", item.Sources.Select(c => c.Name));
        }
        
        public override dynamic? JsonExport(ColumnConfiguration columnConfiguration, InventoryItem item)
        {
            return JsonExport(columnConfiguration, item.Item);
        }
        
        public override dynamic? JsonExport(ColumnConfiguration columnConfiguration, CraftItem item)
        {
            return JsonExport(columnConfiguration, item.Item);
        }
        
        public override dynamic? JsonExport(ColumnConfiguration columnConfiguration, SortingResult item)
        {
            return JsonExport(columnConfiguration, item.InventoryItem);
        }

        public override string Name { get; set; } = "Acqusition";
        public override float Width { get; set; } = 250;

        public override string HelpText { get; set; } =
            "Shows icons indicating what items can be obtained with(gathering, crafting, currency, etc)";
        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
        public override IEnumerable<InventoryItem> Filter(ColumnConfiguration columnConfiguration,
            IEnumerable<InventoryItem> items)
        {
            return FilterText == "" ? items : items.Where(c =>
            {
                var currentValue = CurrentValue(columnConfiguration, c);
                if (currentValue == null)
                {
                    return false;
                }

                return currentValue.Any(c => c.FormattedName.ToLower().PassesFilter(FilterComparisonText));
            });
        }
        public override IEnumerable<SortingResult> Filter(ColumnConfiguration columnConfiguration,
            IEnumerable<SortingResult> items)
        {
            return FilterText == "" ? items : items.Where(c =>
            {
                var currentValue = CurrentValue(columnConfiguration, c);
                if (currentValue == null)
                {
                    return false;
                }

                return currentValue.Any(c => c.FormattedName.ToLower().PassesFilter(FilterComparisonText));
            });
        }

        public override IEnumerable<ItemEx> Filter(ColumnConfiguration columnConfiguration, IEnumerable<ItemEx> items)
        {
            return FilterText == "" ? items : items.Where(c =>
            {
                var currentValue = CurrentValue(columnConfiguration, c);
                if (currentValue == null)
                {
                    return false;
                }

                return currentValue.Any(c => c.FormattedName.ToLower().PassesFilter(FilterComparisonText));
            });
        }

        public override IEnumerable<CraftItem> Filter(ColumnConfiguration columnConfiguration,
            IEnumerable<CraftItem> items)
        {
            return FilterText == "" ? items : items.Where(c =>
            {
                var currentValue = CurrentValue(columnConfiguration, c);
                if (currentValue == null)
                {
                    return false;
                }

                return currentValue.Any(c => c.FormattedName.ToLower().PassesFilter(FilterComparisonText));
            });
        }
        
        public override IEnumerable<InventoryChange> Filter(ColumnConfiguration columnConfiguration,
            IEnumerable<InventoryChange> items)
        {
            return FilterText == "" ? items : items.Where(c =>
            {
                var currentValue = CurrentValue(columnConfiguration, c.InventoryItem);
                if (currentValue == null)
                {
                    return false;
                }

                return currentValue.Any(c => c.FormattedName.ToLower().PassesFilter(FilterComparisonText));
            });
        }

        public override IEnumerable<InventoryItem> Sort(ColumnConfiguration columnConfiguration,
            ImGuiSortDirection direction, IEnumerable<InventoryItem> items)
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

        public override IEnumerable<SortingResult> Sort(ColumnConfiguration columnConfiguration,
            ImGuiSortDirection direction, IEnumerable<SortingResult> items)
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

        public override IEnumerable<ItemEx> Sort(ColumnConfiguration columnConfiguration, ImGuiSortDirection direction,
            IEnumerable<ItemEx> items)
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

        public override IEnumerable<CraftItem> Sort(ColumnConfiguration columnConfiguration,
            ImGuiSortDirection direction, IEnumerable<CraftItem> items)
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
        
        public override IEnumerable<InventoryChange> Sort(ColumnConfiguration columnConfiguration,
            ImGuiSortDirection direction, IEnumerable<InventoryChange> items)
        {
            return direction == ImGuiSortDirection.Ascending ? items.OrderBy(item =>
            {
                var currentValue = CurrentValue(columnConfiguration, item.InventoryItem);
                return currentValue?.Count ?? 0;
            }) : items.OrderByDescending(item =>
            {
                var currentValue = CurrentValue(columnConfiguration, item.InventoryItem);
                return currentValue?.Count ?? 0;
            });
        }

        public override List<MessageBase>? Draw(FilterConfiguration configuration, ColumnConfiguration columnConfiguration,
            InventoryItem item, int rowIndex)
        {
            return DoDraw(item, CurrentValue(columnConfiguration, item), rowIndex, configuration, columnConfiguration);
        }

        public override List<MessageBase>? Draw(FilterConfiguration configuration,
            ColumnConfiguration columnConfiguration,
            SortingResult item, int rowIndex)
        {
            return DoDraw(item, CurrentValue(columnConfiguration, item), rowIndex, configuration, columnConfiguration);
        }

        public override List<MessageBase>? Draw(FilterConfiguration configuration,
            ColumnConfiguration columnConfiguration,
            ItemEx item, int rowIndex)
        {
            return DoDraw(item, CurrentValue(columnConfiguration, item), rowIndex, configuration, columnConfiguration);
        }
        
        public override List<MessageBase>? Draw(FilterConfiguration configuration,
            ColumnConfiguration columnConfiguration,
            CraftItem item, int rowIndex)
        {
            return DoDraw(item, CurrentValue(columnConfiguration, item), rowIndex, configuration, columnConfiguration);
        }
    }
}