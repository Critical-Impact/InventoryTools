using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns
{
    public class TimedNodeColumn : CheckboxColumn
    {
        private readonly ExcelCache _excelCache;

        public TimedNodeColumn(ILogger<TimedNodeColumn> logger, ImGuiService imGuiService, ExcelCache excelCache) : base(logger, imGuiService)
        {
            _excelCache = excelCache;
        }
        public override ColumnCategory ColumnCategory => ColumnCategory.Basic;
        public override bool? CurrentValue(ColumnConfiguration columnConfiguration, SearchResult searchResult)
        {
            return _excelCache.IsItemAvailableAtTimedNode(searchResult.Item.RowId);
        }
        public override string Name { get; set; } = "Is From Timed Node?";
        public override string RenderName => "Timed Node?";
        public override float Width { get; set; } = 125.0f;
        public override string HelpText { get; set; } = "Is this item available at a timed node?";
        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Boolean;
    }
}