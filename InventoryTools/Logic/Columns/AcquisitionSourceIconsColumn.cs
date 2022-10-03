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

namespace InventoryTools.Logic.Columns
{
    public class AcquisitionSourceIconsColumn : Column<List<ItemSource>?>
    {
        public override List<ItemSource>? CurrentValue(InventoryItem item)
        {
            return CurrentValue(item.Item);
        }

        public override List<ItemSource>? CurrentValue(ItemEx item)
        {
            return item.Sources;
        }

        public override List<ItemSource>? CurrentValue(SortingResult item)
        {
            return CurrentValue(item.InventoryItem);
        }

        public override List<ItemSource>? CurrentValue(CraftItem item)
        {
            return item.Item.Sources;
        }

        public override IColumnEvent? DoDraw(List<ItemSource>? currentValue, int rowIndex,
            FilterConfiguration filterConfiguration)
        {
            ImGui.TableNextColumn();
            if (currentValue != null)
            {
                ImGui.BeginChild("scrolling" + rowIndex, new Vector2(210, filterConfiguration.TableHeight + ImGui.GetStyle().CellPadding.Y) * ImGui.GetIO().FontGlobalScale, false);
                for (var index = 0; index < currentValue.Count; index++)
                {
                    var item = currentValue[index];
                    var sourceIcon = PluginService.IconStorage[item.Icon];
                    ImGui.Image(sourceIcon.ImGuiHandle,
                        new Vector2(filterConfiguration.TableHeight , filterConfiguration.TableHeight ) * ImGui.GetIO().FontGlobalScale);
                    if (item.HasItem && item.Item != null)
                    {
                        if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled &
                                                ImGuiHoveredFlags.AllowWhenOverlapped &
                                                ImGuiHoveredFlags.AllowWhenBlockedByPopup &
                                                ImGuiHoveredFlags.AllowWhenBlockedByActiveItem &
                                                ImGuiHoveredFlags.AnyWindow) &&
                            ImGui.IsMouseReleased(ImGuiMouseButton.Right))
                        {
                            ImGui.OpenPopup("RightClick" + item.ItemId);
                        }
                        if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled &
                                                ImGuiHoveredFlags.AllowWhenOverlapped &
                                                ImGuiHoveredFlags.AllowWhenBlockedByPopup &
                                                ImGuiHoveredFlags.AllowWhenBlockedByActiveItem &
                                                ImGuiHoveredFlags.AnyWindow) &&
                            ImGui.IsMouseReleased(ImGuiMouseButton.Left) && item.ItemId != null)
                        {
                            PluginService.WindowService.OpenItemWindow(item.ItemId.Value);
                        }

                        if (ImGui.BeginPopup("RightClick" + item.ItemId))
                        {
                            item.Item.DrawRightClickPopup();
                            ImGui.EndPopup();
                        }
                    }

                    ImGuiUtil.HoverTooltip(item.FormattedName);
                    if ((index + 1) % 5 != 0)
                    {
                        ImGui.SameLine();
                    }
                }
                ImGui.EndChild();
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

        public override string Name { get; set; } = "Acqusition";
        public override float Width { get; set; } = 210;

        public override string HelpText { get; set; } =
            "Shows icons indicating what items can be obtained with(gathering, crafting, currency, etc)";
        public override string FilterText { get; set; } = "";
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