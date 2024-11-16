using AllaganLib.GameSheets.Caches;
using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns
{
    public class HiddenNodeColumn : CheckboxColumn
    {

        public HiddenNodeColumn(ILogger<HiddenNodeColumn> logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }
        public override ColumnCategory ColumnCategory => ColumnCategory.Basic;
        public override bool? CurrentValue(ColumnConfiguration columnConfiguration, SearchResult searchResult)
        {
            return searchResult.Item.HasSourcesByCategory(ItemInfoCategory.HiddenGathering);
        }
        public override string Name { get; set; } = "Is From Hidden Node?";
        public override string RenderName => "Hidden Node?";
        public override float Width { get; set; } = 125.0f;
        public override string HelpText { get; set; } = "Is this item available at a hidden node?";
        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Boolean;
    }
}