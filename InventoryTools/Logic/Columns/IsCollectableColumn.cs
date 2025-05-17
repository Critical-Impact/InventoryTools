using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns
{
    public class IsCollectableColumn : CheckboxColumn
    {
        public IsCollectableColumn(ILogger<IsCollectableColumn> logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }
        public override ColumnCategory ColumnCategory => ColumnCategory.Basic;
        public override bool? CurrentValue(ColumnConfiguration columnConfiguration, SearchResult searchResult)
        {
            return searchResult.Item.IsCollectable;
        }
        public override string Name { get; set; } = "Is Collectable?";
        public override float Width { get; set; } = 90.0f;
        public override string HelpText { get; set; } = "Is the item collectable?";
        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Boolean;
    }
}