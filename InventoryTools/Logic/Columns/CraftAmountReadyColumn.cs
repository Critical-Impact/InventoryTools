using System.Collections.Generic;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services.Mediator;
using CriticalCommonLib.Sheets;
using Dalamud.Interface.Colors;
using ImGuiNET;
using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns
{
    public class CraftAmountReadyColumn : IntegerColumn
    {
        public CraftAmountReadyColumn(ILogger<CraftAmountReadyColumn> logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }
        public override ColumnCategory ColumnCategory => ColumnCategory.Crafting;
        public override int? CurrentValue(ColumnConfiguration columnConfiguration, InventoryItem item)
        {
            return 0;
        }

        public override int? CurrentValue(ColumnConfiguration columnConfiguration, ItemEx item)
        {
            return 0;
        }

        public override int? CurrentValue(ColumnConfiguration columnConfiguration, SortingResult item)
        {
            return 0;
        }

        public override int? CurrentValue(ColumnConfiguration columnConfiguration, CraftItem currentValue)
        {
            if (currentValue.IsOutputItem)
            {
                return 0;
            }
            return (int?) currentValue.QuantityReady;
        }
        
        public override List<MessageBase>? Draw(FilterConfiguration configuration,
            ColumnConfiguration columnConfiguration,
            CraftItem item, int rowIndex)
        {
            if (item.IsOutputItem)
            {
                ImGui.TableNextColumn();
                return null;
            }
            if(item.QuantityReady >= item.QuantityNeeded)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.HealerGreen);
            }

            base.Draw(configuration, columnConfiguration, item, rowIndex);
            if(item.QuantityReady >= item.QuantityNeeded)
            {
                ImGui.PopStyleColor();
            }
            return null;
        }

        public override string Name { get; set; } = "Amount in Character Inventory";
        public override string RenderName => "Inventory";
        public override float Width { get; set; } = 60;
        public override bool? CraftOnly => true;
        public override FilterType AvailableIn { get; } = Logic.FilterType.CraftFilter;
        public override string HelpText { get; set; } =
            "This is the amount available within your filtered inventories available to complete the craft.";
        public override bool HasFilter { get; set; } = false;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
    }
}