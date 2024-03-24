using System.Collections.Generic;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services.Mediator;
using CriticalCommonLib.Sheets;
using Dalamud.Interface.Colors;
using Dalamud.Plugin.Services;
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
            return item.Quantity;
        }

        public override int? CurrentValue(ColumnConfiguration columnConfiguration, CraftItem currentValue)
        {
            if (currentValue.IsOutputItem)
            {
                return 0;
            }
            return (int)currentValue.QuantityWillRetrieve;
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
            if (item.QuantityWillRetrieve != 0)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.ParsedBlue);
            }

            base.Draw(configuration, columnConfiguration, item, rowIndex);

            if (item.QuantityWillRetrieve != 0)
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
    }
}