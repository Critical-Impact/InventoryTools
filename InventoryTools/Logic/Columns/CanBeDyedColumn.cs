using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns
{
    public class CanBeDyedColumn : CheckboxColumn
    {
        public CanBeDyedColumn(ILogger<CanBeDyedColumn> logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }
        public override ColumnCategory ColumnCategory => ColumnCategory.Basic;
        public override bool? CurrentValue(ColumnConfiguration columnConfiguration, InventoryItem item)
        {
            return CurrentValue(columnConfiguration, item.Item);
        }

        public override bool? CurrentValue(ColumnConfiguration columnConfiguration, ItemEx item)
        {
            return item.IsDyeable;
        }

        public override bool? CurrentValue(ColumnConfiguration columnConfiguration, SortingResult item)
        {
            return CurrentValue(columnConfiguration, item.InventoryItem);
        }

        public override string Name { get; set; } = "Is Dyeable?";
        public override float Width { get; set; } = 80.0f;
        public override string HelpText { get; set; } = "Can the item be dyed?";
        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Boolean;
    }
}