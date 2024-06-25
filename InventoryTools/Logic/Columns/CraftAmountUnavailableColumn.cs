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

        public override int? CurrentValue(ColumnConfiguration columnConfiguration, SearchResult searchResult)
        {
            if (searchResult.CraftItem == null) return 0;
            return Math.Max(0, (int)searchResult.CraftItem.QuantityMissingOverall);
        }
        
        public override List<MessageBase>? Draw(FilterConfiguration configuration,
            ColumnConfiguration columnConfiguration,
            SearchResult searchResult, int rowIndex, int columnIndex)
        {
            if (searchResult.CraftItem == null) return null;
            
            if (CurrentValue(columnConfiguration, searchResult) > 0)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudRed);
            }

            base.Draw(configuration, columnConfiguration, searchResult, rowIndex, columnIndex);
            if (CurrentValue(columnConfiguration, searchResult) > 0)
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