using System;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using Dalamud.Interface.Colors;
using ImGuiNET;
using InventoryTools.Logic.Columns.Abstract;

namespace InventoryTools.Logic.Columns
{
    public class CraftSimpleColumn : TextColumn
    {
        public override string? CurrentValue(CraftItem currentValue)
        {
            return "";
        }

        public override void Draw(FilterConfiguration configuration, CraftItem item, int rowIndex)
        {
            ImGui.TableNextColumn();
            var unavailable = Math.Max(0, (int)item.RequiredQuantityUnavailable - (int)item.QuantityCanCraft);
            if (unavailable != 0)
            {
                if (item.Item.ObtainedGathering)
                {
                    ImGui.TextColored(ImGuiColors.DalamudYellow, "Gather " + unavailable);
                }
                else if (item.Item.ObtainedGil)
                {
                    ImGui.TextColored(ImGuiColors.DalamudYellow, "Buy " + unavailable);
                }
                else
                {
                    ImGui.TextColored(ImGuiColors.DalamudRed, "Missing " + unavailable);
                }

                return;
            }
            var canCraft = item.QuantityCanCraft;
            if (canCraft != 0)
            {
                if (item.Item.CanBeCrafted)
                {
                    ImGui.TextColored(ImGuiColors.ParsedBlue, "Craft " + (uint)Math.Ceiling((double)canCraft / item.Yield));
                }
                return;
            }

            var retrieve = (int)item.QuantityWillRetrieve;
            if (!item.IsOutputItem && retrieve != 0)
            {
                ImGui.TextColored(ImGuiColors.DalamudOrange, "Retrieve " + retrieve);
                return;
            }

            if (item.IsOutputItem)
            {
                ImGui.TextColored(ImGuiColors.DalamudWhite, "Waiting");
                return;
            }
            ImGui.TextColored(ImGuiColors.HealerGreen, "Done");
        }

        public override string? CurrentValue(InventoryItem item)
        {
            return "";
        }

        public override string? CurrentValue(ItemEx item)
        {
            return "";
        }

        public override string? CurrentValue(SortingResult item)
        {
            return "";
        }

        public override string Name { get; set; } = "Next Step";
        public override float Width { get; set; } = 100;
        public override bool? CraftOnly => true;

        public override string HelpText { get; set; } =
            "Shows a simplified version of what you should do next in your craft";
        public override bool HasFilter { get; set; } = false;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
        public override FilterType AvailableIn { get; } = Logic.FilterType.CraftFilter;
    }
}