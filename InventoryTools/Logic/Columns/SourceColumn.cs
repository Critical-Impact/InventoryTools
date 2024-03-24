using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using CriticalCommonLib.Sheets;
using Dalamud.Plugin.Services;
using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns
{
    public class SourceColumn : TextColumn
    {
        private readonly ICharacterMonitor _characterMonitor;

        public SourceColumn(ILogger<SourceColumn> logger, ImGuiService imGuiService, ICharacterMonitor characterMonitor) : base(logger, imGuiService)
        {
            _characterMonitor = characterMonitor;
        }
        public override ColumnCategory ColumnCategory => ColumnCategory.Inventory;
        public override string? CurrentValue(ColumnConfiguration columnConfiguration, InventoryItem item)
        {
            return _characterMonitor.Characters.ContainsKey(item.RetainerId) ?  _characterMonitor.Characters[item.RetainerId].FormattedName : "Unknown (" + item.RetainerId + ")";
        }

        public override string? CurrentValue(ColumnConfiguration columnConfiguration, ItemEx item)
        {
            return null;
        }

        public override string? CurrentValue(ColumnConfiguration columnConfiguration, SortingResult item)
        {
            return CurrentValue(columnConfiguration, item.InventoryItem);
        }

        public override string Name { get; set; } = "Source";
        public override float Width { get; set; } = 100.0f;
        public override string HelpText { get; set; } = "Shows the character/retainer an item is located in.";
        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
    }
}