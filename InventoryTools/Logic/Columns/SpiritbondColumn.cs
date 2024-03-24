using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using Dalamud.Plugin.Services;
using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns
{
    public class SpiritbondColumn : TextColumn
    {
        public SpiritbondColumn(ILogger<SpiritbondColumn> logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }
        public override ColumnCategory ColumnCategory => ColumnCategory.Basic;
        public override string? CurrentValue(ColumnConfiguration columnConfiguration, InventoryItem item)
        {
            return item.ActualSpiritbond + "%%";

        }

        public override string? CurrentValue(ColumnConfiguration columnConfiguration, ItemEx item)
        {
            return null;
        }

        public override string? CurrentValue(ColumnConfiguration columnConfiguration, SortingResult item)
        {
            return CurrentValue(columnConfiguration, item.InventoryItem);
        }

        public override FilterType AvailableIn => Logic.FilterType.SearchFilter | Logic.FilterType.SortingFilter;

        public override string Name { get; set; } = "Spiritbond";
        public override float Width { get; set; } = 90.0f;
        public override string HelpText { get; set; } = "Shows the spiritbond % of the item.";
        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
    }
}