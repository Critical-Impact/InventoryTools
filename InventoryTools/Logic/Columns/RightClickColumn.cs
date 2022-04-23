using System.Numerics;
using CriticalCommonLib.Extensions;
using CriticalCommonLib.Models;
using Dalamud.Logging;
using ImGuiNET;
using InventoryTools.Extensions;
using Lumina.Excel.GeneratedSheets;

namespace InventoryTools.Logic.Columns
{
    public class RightClickColumn : TextColumn
    {
        public override string? CurrentValue(InventoryItem item)
        {
            return null;
        }

        public override string? CurrentValue(Item item)
        {
            return null;
        }

        public override string? CurrentValue(SortingResult item)
        {
            return null;
        }

        public override string Name { get; set; } = "Right Click";
        public override float Width { get; set; } = 1.0f;
        public override string FilterText { get; set; } = "";
        public override bool HasFilter { get; set; } = false;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;

        public override void Draw(InventoryItem item, int rowIndex)
        {
            if (item.Item == null)
            {
                return;
            }
            Draw(item.Item, rowIndex);
        }

        public override void Draw(SortingResult item, int rowIndex)
        {
            if (item.InventoryItem.Item == null)
            {
                return;
            }
            Draw(item.InventoryItem.Item, rowIndex);
        }

        public override void Draw(Item item, int rowIndex)
        {
            var hoveredRow = -1;
            ImGui.Selectable("", false, ImGuiSelectableFlags.SpanAllColumns, new Vector2(0, 32));
            if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled & ImGuiHoveredFlags.AllowWhenOverlapped & ImGuiHoveredFlags.AllowWhenBlockedByPopup & ImGuiHoveredFlags.AllowWhenBlockedByActiveItem & ImGuiHoveredFlags.AnyWindow)) {
                hoveredRow = rowIndex;
            }
            if (hoveredRow == rowIndex && ImGui.IsMouseReleased(ImGuiMouseButton.Right))
            {
                ImGui.OpenPopup("RightClick" + rowIndex);
            }

            if (ImGui.BeginPopup("RightClick" + rowIndex))
            {
                ImGui.Text(item.Name);
                ImGui.Separator();
                if (ImGui.Selectable("Open in Garland Tools"))
                {
                    $"https://www.garlandtools.org/db/#item/{item.RowId}".OpenBrowser();
                }
                if (ImGui.Selectable("Open in Teamcraft"))
                {
                                        
                }
                if (item.CanTryOn() && ImGui.Selectable("Try On"))
                {
                    if (PluginService.TryOn.CanUseTryOn)
                    {
                        PluginService.TryOn.TryOnItem(item);
                    }
                }

                if (item.CanBeCrafted() && ImGui.Selectable("View Requirements"))
                {
                    PluginLogic.ShowCraftRequirementsWindow(item);   
                }
                ImGui.EndPopup();
            }
        }
    }
}