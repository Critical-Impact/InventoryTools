using System.Collections.Generic;
using System.Numerics;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services.Mediator;
using CriticalCommonLib.Sheets;
using ImGuiNET;
using InventoryTools.Extensions;
using InventoryTools.Logic.Columns.Abstract;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Plugin.Services;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns
{
    public class RightClickColumn : TextColumn
    {
        public RightClickColumn(ILogger<RightClickColumn> logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }
        public override ColumnCategory ColumnCategory => ColumnCategory.Tools;

        public override string? CurrentValue(ColumnConfiguration columnConfiguration, InventoryItem item)
        {
            return null;
        }

        public override string? CurrentValue(ColumnConfiguration columnConfiguration, ItemEx item)
        {
            return null;
        }

        public override string? CurrentValue(ColumnConfiguration columnConfiguration, SortingResult item)
        {
            return null;
        }

        public override string Name { get; set; } = "Right Click";
        public override float Width { get; set; } = 1.0f;

        public override string HelpText { get; set; } =
            "You shouldn't see this, but if you do it's the column that adds in the right click functionality.";
        public override bool HasFilter { get; set; } = false;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;

        public override List<MessageBase>? Draw(FilterConfiguration configuration,
            ColumnConfiguration columnConfiguration,
            InventoryItem item, int rowIndex)
        {
            return Draw(configuration, columnConfiguration, item.Item, rowIndex);
        }

        public override List<MessageBase>? Draw(FilterConfiguration configuration,
            ColumnConfiguration columnConfiguration,
            SortingResult item, int rowIndex)
        {
            return Draw(configuration, columnConfiguration, item.InventoryItem.Item, rowIndex);
        }

        public override List<MessageBase>? Draw(FilterConfiguration configuration,
            ColumnConfiguration columnConfiguration,
            InventoryChange item, int rowIndex)
        {
            return Draw(configuration, columnConfiguration, item.InventoryItem, rowIndex);
        }

        public override List<MessageBase>? Draw(FilterConfiguration configuration,
            ColumnConfiguration columnConfiguration,
            CraftItem item, int rowIndex)
        {
            var messages = new List<MessageBase>();
            var hoveredRow = -1;
            ImGui.Selectable("", false, ImGuiSelectableFlags.SpanAllColumns, new Vector2(0, configuration.TableHeight) * ImGui.GetIO().FontGlobalScale);
            if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled & ImGuiHoveredFlags.AllowWhenOverlapped & ImGuiHoveredFlags.AllowWhenBlockedByPopup & ImGuiHoveredFlags.AllowWhenBlockedByActiveItem & ImGuiHoveredFlags.AnyWindow)) {
                hoveredRow = rowIndex;
            }
            if (hoveredRow == rowIndex && ImGui.IsMouseReleased(ImGuiMouseButton.Right))
            {
                ImGui.OpenPopup("RightClick" + rowIndex);
            }

            using (var popup = ImRaii.Popup("RightClick" + rowIndex))
            {
                if (popup.Success)
                {
                    ImGuiService.RightClickService.DrawRightClickPopup(item, configuration, messages);
                }
            }

            return messages;
        }

        

        public override List<MessageBase>? Draw(FilterConfiguration configuration,
            ColumnConfiguration columnConfiguration,
            ItemEx item, int rowIndex)
        {
            var messages = new List<MessageBase>();
            var hoveredRow = -1;
            ImGui.Selectable("", false, ImGuiSelectableFlags.SpanAllColumns, new Vector2(0, configuration.TableHeight) * ImGui.GetIO().FontGlobalScale);
            if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled & ImGuiHoveredFlags.AllowWhenOverlapped & ImGuiHoveredFlags.AllowWhenBlockedByPopup & ImGuiHoveredFlags.AllowWhenBlockedByActiveItem & ImGuiHoveredFlags.AnyWindow)) {
                hoveredRow = rowIndex;
            }
            if (hoveredRow == rowIndex && ImGui.IsMouseReleased(ImGuiMouseButton.Right))
            {
                ImGui.OpenPopup("RightClick" + rowIndex);
            }

            using (var popup = ImRaii.Popup("RightClick" + rowIndex))
            {
                using var _ = ImRaii.PushId("RightClick" + rowIndex);
                if (popup.Success)
                {
                    ImGuiService.RightClickService.DrawRightClickPopup(item, messages);
                }
            }

            return messages;
        }
    }
}