using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using Dalamud.Plugin.Services;
using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Misc;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns
{
    public class IsHousingItemColumn : CheckboxColumn
    {
        public IsHousingItemColumn(ILogger<IsHousingItemColumn> logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }
        public override ColumnCategory ColumnCategory => ColumnCategory.Basic;
        public override bool? CurrentValue(ColumnConfiguration columnConfiguration, InventoryItem item)
        {
            return item.Item == null ? false : CurrentValue(columnConfiguration, item.Item);
        }

        public override bool? CurrentValue(ColumnConfiguration columnConfiguration, ItemEx item)
        {
            return Helpers.HousingCategoryIds.Contains(item.ItemUICategory.Row);
        }

        public override bool? CurrentValue(ColumnConfiguration columnConfiguration, SortingResult item)
        {
            return CurrentValue(columnConfiguration, item.InventoryItem);
        }

        public override string Name { get; set; } = "Is Housing Item?";
        public override string RenderName => "Is Housing?";
        public override float Width { get; set; } = 100;
        public override string HelpText { get; set; } = "Is this item a housing item? This might be slightly inaccurate for the time being.";
        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
    }
}