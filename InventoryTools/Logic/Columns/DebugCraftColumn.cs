using System.Collections.Generic;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services.Mediator;
using CriticalCommonLib.Sheets;
using ImGuiNET;
using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns
{
    public class DebugCraftColumn : TextColumn
    {
        public DebugCraftColumn(ILogger<DebugCraftColumn> logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }
        public override ColumnCategory ColumnCategory => ColumnCategory.Debug;
        public override string? CurrentValue(ColumnConfiguration columnConfiguration, InventoryItem item)
        {
            return CurrentValue(columnConfiguration, item.Item);
        }

        public override string? CurrentValue(ColumnConfiguration columnConfiguration, ItemEx item)
        {
            return "";
        }

        public override string? CurrentValue(ColumnConfiguration columnConfiguration, SortingResult item)
        {
            return CurrentValue(columnConfiguration, item.InventoryItem);
        }

        public override List<MessageBase>? Draw(FilterConfiguration configuration,
            ColumnConfiguration columnConfiguration,
            CraftItem item, int rowIndex, int columnIndex)
        {
            ImGui.TableNextColumn();
            if (!ImGui.TableGetColumnFlags().HasFlag(ImGuiTableColumnFlags.IsEnabled)) return null;
            ImGui.Text("Required: " +  item.QuantityRequired);
            ImGui.Text("Needed: " +  item.QuantityNeeded);
            ImGui.Text("Needed Pre Update: " +  item.QuantityNeededPreUpdate);
            ImGui.Text("Available: " +  item.QuantityAvailable);
            ImGui.Text("Ready: " +  item.QuantityReady);
            ImGui.Text("Can Craft: " +  item.QuantityCanCraft);
            ImGui.Text("Will Retrieve: " + item.QuantityWillRetrieve);
            return null;
        }

        public override string Name { get; set; } = "Debug - Craft";
        public override float Width { get; set; } = 200;
        public override string HelpText { get; set; } = "Shows craft debug information";
        public override bool HasFilter { get; set; } = true;
        public override bool IsDebug { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
    }
}