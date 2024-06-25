using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using CriticalCommonLib.Sheets;
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
        public override string? CurrentValue(ColumnConfiguration columnConfiguration, SearchResult searchResult)
        {
            if (searchResult.InventoryItem != null)
            {
                return _characterMonitor.Characters.TryGetValue(searchResult.InventoryItem.RetainerId, out var character)
                    ? character.FormattedName
                    : "Unknown (" + searchResult.InventoryItem.RetainerId + ")";
            }

            return null;
        }
        public override string Name { get; set; } = "Source";
        public override float Width { get; set; } = 100.0f;
        public override string HelpText { get; set; } = "Shows the character/retainer an item is located in.";
        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
        public override FilterType DefaultIn => Logic.FilterType.SearchFilter | Logic.FilterType.SortingFilter | Logic.FilterType.CraftFilter | Logic.FilterType.HistoryFilter;
    }
}