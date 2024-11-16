using AllaganLib.GameSheets.Caches;
using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns
{
    public class LeveIsCraftLevelColumn : CheckboxColumn
    {

        public LeveIsCraftLevelColumn(ILogger<LeveIsCraftLevelColumn> logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }

        public override ColumnCategory ColumnCategory => ColumnCategory.Basic;
        public override bool? CurrentValue(ColumnConfiguration columnConfiguration, SearchResult searchResult)
        {
            return searchResult.Item.HasSourcesByType(ItemInfoType.CraftLeve);
        }
        public override string Name { get; set; } = "Is Leve(Craft) Item?";
        public override string RenderName => "Leve (Craft)";
        public override float Width { get; set; } = 100.0f;
        public override string HelpText { get; set; } = "Is this item used in a craft leve?";
        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Boolean;
    }
}