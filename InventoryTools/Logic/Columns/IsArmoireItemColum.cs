using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns
{
    public class IsArmoireItem : CheckboxColumn
    {
        public IsArmoireItem(ILogger<IsArmoireItem> logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }
        public override ColumnCategory ColumnCategory => ColumnCategory.Basic;
        public override bool? CurrentValue(ColumnConfiguration columnConfiguration, InventoryItem item)
        {
            return CurrentValue(columnConfiguration, item.Item);
        }

        public override bool? CurrentValue(ColumnConfiguration columnConfiguration, ItemEx item)
        {
            return item.CabinetCategory != 0;
        }

        public override bool? CurrentValue(ColumnConfiguration columnConfiguration, SortingResult item)
        {
            return CurrentValue(columnConfiguration, item.InventoryItem);
        }

        public override string Name { get; set; } = "Is Armoire Item?";
        public override string RenderName => "Is Armoire?";
        public override float Width { get; set; } = 100;
        public override string HelpText { get; set; } = "Shows if the item belongs in the armoire.";
        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
    }
}