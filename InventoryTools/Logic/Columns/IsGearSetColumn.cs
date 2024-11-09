using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns
{
    public class IsGearSetColumn : CheckboxColumn
    {
        public IsGearSetColumn(ILogger<IsGearSetColumn> logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }
        public override ColumnCategory ColumnCategory => ColumnCategory.Basic;
        public override bool? CurrentValue(ColumnConfiguration columnConfiguration, SearchResult searchResult)
        {
            if (searchResult.InventoryItem != null)
            {
                if (searchResult.InventoryItem.GearSets == null)
                {
                    return false;
                }

                return searchResult.InventoryItem.GearSets.Length != 0;
            }

            return false;

        }
        public override string Name { get; set; } = "In Gearset?";
        public override float Width { get; set; } = 80;
        public override string HelpText { get; set; } = "Is this item part of a gearset?";
        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Boolean;
    }
}