using AllaganLib.GameSheets.Caches;
using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns
{
    public class IsCraftingItemColumn : CheckboxColumn
    {

        public IsCraftingItemColumn(ILogger<IsCraftingItemColumn> logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }
        public override ColumnCategory ColumnCategory => ColumnCategory.Basic;

        public override bool? CurrentValue(ColumnConfiguration columnConfiguration, SearchResult searchResult)
        {
            return searchResult.Item.HasUsesByType(ItemInfoType.CraftRecipe);
        }
        public override string Name { get; set; } = "Is Craft Component?";
        public override string RenderName => "Is Craft Item?";
        public override float Width { get; set; } = 100;
        public override string HelpText { get; set; } = "Can this item be used to craft another item?";
        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
        public override FilterType DefaultIn => Logic.FilterType.GameItemFilter;
    }
}