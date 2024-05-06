using System;
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
    public class CraftAmountUnavailableColumn : IntegerColumn
    {
        public CraftAmountUnavailableColumn(ILogger<CraftAmountUnavailableColumn> logger, ImGuiService imGuiService) : base(logger, imGuiService)
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
            return Math.Max(0, (int)currentValue.QuantityMissingOverall);
        }
        
        public override List<MessageBase>? Draw(FilterConfiguration configuration,
            ColumnConfiguration columnConfiguration,
            CraftItem item, int rowIndex)
        {
            if (CurrentValue(columnConfiguration, item) > 0)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudRed);
            }

            base.Draw(configuration, columnConfiguration, item, rowIndex);
            if (CurrentValue(columnConfiguration, item) > 0)
            {
                ImGui.PopStyleColor();
            }
            return null;
        }
        public override FilterType AvailableIn { get; } = Logic.FilterType.CraftFilter;
        public override string Name { get; set; } = "Amount Missing";
        public override string RenderName => "Missing";
        public override float Width { get; set; } = 60;
        public override bool? CraftOnly => true;
        public override string HelpText { get; set; } =
            "This is the amount that needs to be sourced from MB/gathering excluding potential items to be withdrawn from retainers.";
        public override bool HasFilter { get; set; } = false;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
    }
}