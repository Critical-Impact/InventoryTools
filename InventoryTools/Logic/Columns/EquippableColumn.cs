using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns
{
    public class EquippableColumn : TextColumn
    {
        public EquippableColumn(ILogger<EquippableColumn> logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }
        public override ColumnCategory ColumnCategory => ColumnCategory.Basic;
        
        public override string? CurrentValue(ColumnConfiguration columnConfiguration, SearchResult searchResult)
        {
            return searchResult.Item.ClassJobCategory.Value?.Name ?? "";
        }
        public override string Name { get; set; } = "Equipped By (Class/Job)";
        public override float Width { get; set; } = 200;
        public override string HelpText { get; set; } = "Shows what class/job an item can be equipped by";
        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
    }
}