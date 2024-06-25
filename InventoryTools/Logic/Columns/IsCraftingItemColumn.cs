using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns
{
    public class IsCraftingItemColumn : CheckboxColumn
    {
        private readonly ExcelCache _excelCache;

        public IsCraftingItemColumn(ILogger<IsCraftingItemColumn> logger, ImGuiService imGuiService, ExcelCache excelCache) : base(logger, imGuiService)
        {
            _excelCache = excelCache;
        }
        public override ColumnCategory ColumnCategory => ColumnCategory.Basic;

        public override bool? CurrentValue(ColumnConfiguration columnConfiguration, SearchResult searchResult)
        {
            return _excelCache.IsCraftItem(searchResult.Item.ItemUICategory.Row);
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