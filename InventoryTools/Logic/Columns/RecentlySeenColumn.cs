using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using InventoryTools.Extensions;
using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns
{
    public class RecentlySeenColumn : TextColumn
    {
        private readonly PluginLogic _pluginLogic;

        public RecentlySeenColumn(ILogger<RecentlySeenColumn> logger, ImGuiService imGuiService, PluginLogic pluginLogic) : base(logger, imGuiService)
        {
            _pluginLogic = pluginLogic;
        }
        public override ColumnCategory ColumnCategory => ColumnCategory.Tools;

        public override string? CurrentValue(ColumnConfiguration columnConfiguration, SearchResult searchResult)
        {
            return _pluginLogic.GetLastSeenTime(searchResult.Item.ItemId)?.ToHumanReadableString() ?? "";
        }
        public override string Name { get; set; } = "Last Seen Date/Time";
        public override string RenderName => "Last Seen";
        public override float Width { get; set; } = 100;
        public override string HelpText { get; set; } = "Displays the last time an item was seen.";
        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
    }
}