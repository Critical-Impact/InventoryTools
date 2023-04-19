using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using ImGuiNET;
using InventoryTools.Extensions;
using InventoryTools.Logic.Columns.Abstract;
using OtterGui;
using OtterGui.Raii;

namespace InventoryTools.Logic.Columns
{
    public class AcquisitionSourceIconsColumn : Column<List<IItemSource>?>
    {
        public override List<IItemSource>? CurrentValue(InventoryItem item)
        {
            return CurrentValue(item.Item);
        }

        public override List<IItemSource>? CurrentValue(ItemEx item)
        {
            return item.Sources;
        }

        public override List<IItemSource>? CurrentValue(SortingResult item)
        {
            return CurrentValue(item.InventoryItem);
        }

        public override List<IItemSource>? CurrentValue(CraftItem item)
        {
            return item.Item.Sources;
        }

        public override IColumnEvent? DoDraw(List<IItemSource>? currentValue, int rowIndex,
            FilterConfiguration filterConfiguration)
        {
            ImGui.TableNextColumn();
            if (currentValue != null)
            {
                UiHelpers.WrapTableColumnElements("ScrollContainer" + rowIndex,currentValue, filterConfiguration.TableHeight * ImGui.GetIO().FontGlobalScale - ImGui.GetStyle().FramePadding.X, item =>
                {
                    var sourceIcon = PluginService.IconStorage[item.Icon];
                    if (item is ItemSource source && source.ItemId != null && source.HasItem && source.Item != null)
                    {
                        if (ImGui.ImageButton(sourceIcon.ImGuiHandle, new Vector2(filterConfiguration.TableHeight, filterConfiguration.TableHeight) * ImGui.GetIO().FontGlobalScale, new Vector2(0, 0),new Vector2(1, 1), 0))
                        {
                            PluginService.WindowService.OpenItemWindow(source.ItemId.Value);
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
                                source.Item?.DrawRightClickPopup();
                            }
                        }
                    }
                    else if (item is DutySource dutySource)
                    {
                        if (ImGui.ImageButton(sourceIcon.ImGuiHandle,
                                new Vector2(filterConfiguration.TableHeight, filterConfiguration.TableHeight) * ImGui.GetIO().FontGlobalScale, new Vector2(0, 0), new Vector2(1, 1), 0))
                        {
                            PluginService.WindowService.OpenDutyWindow(dutySource.ContentFinderConditionId);
                        }
                    }
                    else if (item is AirshipSource airshipSource)
                    {
                        if (ImGui.ImageButton(sourceIcon.ImGuiHandle,
                                new Vector2(filterConfiguration.TableHeight, filterConfiguration.TableHeight) * ImGui.GetIO().FontGlobalScale, new Vector2(0, 0), new Vector2(1, 1), 0))
                        {
                            PluginService.WindowService.OpenAirshipWindow(airshipSource
                                .AirshipExplorationPointExId);
                        }
                    }
                    else if (item is SubmarineSource submarineSource)
                    {
                        if (ImGui.ImageButton(sourceIcon.ImGuiHandle,
                                new Vector2(filterConfiguration.TableHeight, filterConfiguration.TableHeight) * ImGui.GetIO().FontGlobalScale, new Vector2(0, 0), new Vector2(1, 1), 0))
                        {
                            PluginService.WindowService.OpenSubmarineWindow(
                                submarineSource.SubmarineExplorationExId);
                        }
                    }
                    else if (item is VentureSource ventureSource)
                    {
                        if (ImGui.ImageButton(sourceIcon.ImGuiHandle,
                                new Vector2(filterConfiguration.TableHeight, filterConfiguration.TableHeight) * ImGui.GetIO().FontGlobalScale, new Vector2(0, 0), new Vector2(1, 1), 0))
                        {
                            PluginService.WindowService.OpenRetainerTaskWindow(ventureSource.RetainerTask.RowId);
                        }
                    }
                    else
                    {
                        if (ImGui.ImageButton(sourceIcon.ImGuiHandle, new Vector2(filterConfiguration.TableHeight, filterConfiguration.TableHeight) * ImGui.GetIO().FontGlobalScale, new Vector2(0, 0), new Vector2(1, 1), 0))
                        {
                            
                        }
                    }
                    ImGuiUtil.HoverTooltip(item.FormattedName);
                    return true;
                });
            }
            return null;
        }

        public override void Setup(int columnIndex)
        {
            ImGui.TableSetupColumn(Name, ImGuiTableColumnFlags.WidthFixed, Width, (uint)columnIndex);
        }


        public override string CsvExport(InventoryItem item)
        {
            return CsvExport(item.Item);
        }

        public override string CsvExport(SortingResult item)
        {
            return CsvExport(item.InventoryItem);
        }

        public override string CsvExport(ItemEx item)
        {
            return String.Join(", ", item.Sources.Select(c => c.Name));
        }
        
        public override dynamic JsonExport(ItemEx item)
        {
            return String.Join(", ", item.Sources.Select(c => c.Name));
        }
        
        public override dynamic JsonExport(InventoryItem item)
        {
            return JsonExport(item.Item);
        }
        
        public override dynamic JsonExport(CraftItem item)
        {
            return JsonExport(item.Item);
        }
        
        public override dynamic JsonExport(SortingResult item)
        {
            return JsonExport(item.InventoryItem);
        }

        public override string Name { get; set; } = "Acqusition";
        public override float Width { get; set; } = 210;

        public override string HelpText { get; set; } =
            "Shows icons indicating what items can be obtained with(gathering, crafting, currency, etc)";
        public override bool HasFilter { get; set; } = false;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
        public override IEnumerable<InventoryItem> Filter(IEnumerable<InventoryItem> items)
        {
            return items;
        }

        public override IEnumerable<SortingResult> Filter(IEnumerable<SortingResult> items)
        {
            return items;
        }

        public override IEnumerable<ItemEx> Filter(IEnumerable<ItemEx> items)
        {
            //return implement me
            return items;
        }

        public override IEnumerable<CraftItem> Filter(IEnumerable<CraftItem> items)
        {
            return items;
        }

        public override IEnumerable<InventoryItem> Sort(ImGuiSortDirection direction, IEnumerable<InventoryItem> items)
        {
            return items;
        }

        public override IEnumerable<SortingResult> Sort(ImGuiSortDirection direction, IEnumerable<SortingResult> items)
        {
            return items;
        }

        public override IEnumerable<ItemEx> Sort(ImGuiSortDirection direction, IEnumerable<ItemEx> items)
        {
            return items;
        }

        public override IEnumerable<CraftItem> Sort(ImGuiSortDirection direction, IEnumerable<CraftItem> items)
        {
            return items;
        }

        public override void Draw(FilterConfiguration configuration, InventoryItem item, int rowIndex)
        {
            DoDraw(CurrentValue(item), rowIndex, configuration);
        }

        public override void Draw(FilterConfiguration configuration, SortingResult item, int rowIndex)
        {
            DoDraw(CurrentValue(item), rowIndex, configuration);

        }

        public override void Draw(FilterConfiguration configuration, ItemEx item, int rowIndex)
        {
            DoDraw(CurrentValue(item), rowIndex, configuration);
        }
        
        public override void Draw(FilterConfiguration configuration, CraftItem item, int rowIndex)
        {
            DoDraw(CurrentValue(item), rowIndex, configuration);
        }
    }
}