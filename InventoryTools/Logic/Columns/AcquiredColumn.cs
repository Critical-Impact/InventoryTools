using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns
{
    public class AcquiredColumn : CheckboxColumn
    {
        private readonly IGameInterface _gameInterface;

        public AcquiredColumn(ILogger<AcquiredColumn> logger, ImGuiService imGuiService, IGameInterface gameInterface) : base(logger, imGuiService)
        {
            _gameInterface = gameInterface;
        }
        public override ColumnCategory ColumnCategory => ColumnCategory.Basic;

        public override bool? CurrentValue(ColumnConfiguration columnConfiguration, SearchResult searchResult)
        {
            var action = searchResult.Item.ItemAction?.Value;
            if (!ActionTypeExt.IsValidAction(action)) {
                return null;
            }
            return _gameInterface.HasAcquired(searchResult.Item);
        }

        public override string Name { get; set; } = "Has Been Acquired?";
        public override string RenderName => "Acquired?";
        public override float Width { get; set; } = 125.0f;

        public override string HelpText { get; set; } =
            "If a item can be acquired(mounts, minions, etc) this shows whether or not it has been acquired on the currently logged in character.";
        
        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Boolean;
        public override FilterType DefaultIn => Logic.FilterType.GameItemFilter;
    }
}