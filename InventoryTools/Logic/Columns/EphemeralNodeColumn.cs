using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns
{
    public class EphemeralNodeColumn : CheckboxColumn
    {

        public EphemeralNodeColumn(ILogger<EphemeralNodeColumn> logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }
        public override ColumnCategory ColumnCategory => ColumnCategory.Basic;

        public override bool? CurrentValue(ColumnConfiguration columnConfiguration, SearchResult searchResult)
        {
            return searchResult.Item.IsItemAvailableAtEphemeralNode;
        }

        public override string Name { get; set; } = "Is From Ephemeral Node?";
        public override string RenderName => "Ephemeral Node?";
        public override float Width { get; set; } = 125.0f;
        public override string HelpText { get; set; } = "Is this item available at a ephemeral node?";
        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Boolean;
    }
}