using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns
{
    public class SearchCategoryColumn : TextColumn
    {
        public SearchCategoryColumn(ILogger<SearchCategoryColumn> logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }
        public override ColumnCategory ColumnCategory => ColumnCategory.Basic;
        public override string? CurrentValue(ColumnConfiguration columnConfiguration, SearchResult searchResult)
        {
            if (searchResult.Item.ItemSearchCategory != null)
            {
                return searchResult.Item.FormattedSearchCategory;
            }

            return "";
        }
        public override string Name { get; set; } = "Category (Marketboard)";
        public override string RenderName => "MB Category";
        public override float Width { get; set; } = 200.0f;

        public override string HelpText { get; set; } =
            "The category of the item based off the market board search categories.";
        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
        public override FilterType DefaultIn => Logic.FilterType.GameItemFilter;
    }
}