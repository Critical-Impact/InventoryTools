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
    public class CraftAmountAvailableColumn : IntegerColumn
    {
        public CraftAmountAvailableColumn(ILogger<CraftAmountAvailableColumn> logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }
        public override ColumnCategory ColumnCategory => ColumnCategory.Crafting;

        public override int? CurrentValue(ColumnConfiguration columnConfiguration, SearchResult searchResult)
        {
            if (searchResult.CraftItem != null)
            {
                if (searchResult.CraftItem.IsOutputItem)
                {
                    return 0;
                }

                return (int) searchResult.CraftItem.QuantityWillRetrieve;
            }

            if (searchResult.SortingResult != null)
            {
                return searchResult.SortingResult.Quantity;
            }

            return 0;
        }

        public override List<MessageBase>? Draw(FilterConfiguration configuration,
            ColumnConfiguration columnConfiguration,
            SearchResult searchResult, int rowIndex, int columnIndex)
        {
            var craftItem = searchResult.CraftItem;
            if (craftItem?.IsOutputItem ?? false)
            {
                ImGui.TableNextColumn();
                return null;
            }
            if (craftItem != null && craftItem.QuantityWillRetrieve != 0)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.ParsedBlue);
            }

            base.Draw(configuration, columnConfiguration, searchResult, rowIndex, columnIndex);

            if (craftItem != null &&craftItem.QuantityWillRetrieve != 0)
            {
                ImGui.PopStyleColor();
            }
            return null;
        }

        public override string Name { get; set; } = "Amount to Retrieve";
        public override string RenderName => "Retrieve";
        public override float Width { get; set; } = 60;
        public override bool? CraftOnly => false;

        public override string HelpText { get; set; } =
            "This is the amount to retrieve from retainers.";
        public override FilterType AvailableIn { get; } = Logic.FilterType.CraftFilter | Logic.FilterType.SortingFilter;
        public override bool HasFilter { get; set; } = false;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
        public override FilterType DefaultIn => Logic.FilterType.CraftFilter;
    }
}