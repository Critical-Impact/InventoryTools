using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns
{
    public class IsIshgardCraftColumn : CheckboxColumn
    {
        public IsIshgardCraftColumn(ILogger<IsIshgardCraftColumn> logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }
        public override ColumnCategory ColumnCategory => ColumnCategory.Basic;
        public override bool? CurrentValue(ColumnConfiguration columnConfiguration, SearchResult searchResult)
        {
            return searchResult.Item.IsIshgardCraft;
        }
        public override string Name { get; set; } = "Is Ishgardian Craft?";
        public override float Width { get; set; } = 100;
        public override string HelpText { get; set; } = "Is this item a Ishgardian Restoration craft item?";
        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
    }
}