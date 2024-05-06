using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using Dalamud.Utility;
using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns
{
    public class UiCategoryColumn : TextColumn
    {
        public UiCategoryColumn(ILogger<UiCategoryColumn> logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }
        public override ColumnCategory ColumnCategory => ColumnCategory.Basic;
        public override string? CurrentValue(ColumnConfiguration columnConfiguration, InventoryItem item)
        {
            if (item.ItemUICategory == null)
            {
                return null;
            }

            return item.FormattedUiCategory;
        }

        public override string? CurrentValue(ColumnConfiguration columnConfiguration, ItemEx item)
        {
            var itemItemUiCategory = item.ItemUICategory;
            if (itemItemUiCategory == null)
            {
                return null;
            }

            return itemItemUiCategory.Value?.Name.ToDalamudString().ToString() ?? "";
        }

        public override string? CurrentValue(ColumnConfiguration columnConfiguration, SortingResult item)
        {
            return CurrentValue(columnConfiguration, item.InventoryItem);
        }

        public override string Name { get; set; } = "Category (Basic)";
        public override string RenderName => "Category";
        public override float Width { get; set; } = 200.0f;
        public override string HelpText { get; set; } = "The category of the item.";
        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
        public override FilterType DefaultIn => Logic.FilterType.GameItemFilter;
    }
}