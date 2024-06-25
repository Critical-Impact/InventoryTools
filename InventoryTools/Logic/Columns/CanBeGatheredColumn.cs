using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns
{
    public class CanBeGatheredColumn : CheckboxColumn
    {
        public CanBeGatheredColumn(ILogger<CanBeGatheredColumn> logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }
        public override ColumnCategory ColumnCategory => ColumnCategory.Basic;
        public override bool? CurrentValue(ColumnConfiguration columnConfiguration, SearchResult searchResult)
        {
            return searchResult.Item.CanBeGathered || searchResult.Item.ObtainedFishing;
        }
        public override string Name { get; set; } = "Is Gatherable?";
        public override float Width { get; set; } = 80.0f;
        public override string HelpText { get; set; } = "Can the item be gathered?";
        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Boolean;
        public override FilterType DefaultIn => Logic.FilterType.GameItemFilter;
    }
}