using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
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
        public override bool? CurrentValue(ColumnConfiguration columnConfiguration, SearchResult searchResult)
        {
            return Helpers.HousingCategoryIds.Contains(searchResult.Item.ItemUICategory.Row);
        }
        public override string Name { get; set; } = "Is Housing Item?";
        public override string RenderName => "Is Housing?";
        public override float Width { get; set; } = 100;
        public override string HelpText { get; set; } = "Is this item a housing item? This might be slightly inaccurate for the time being.";
        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
    }
}