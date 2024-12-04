using CriticalCommonLib.Models;
using CriticalCommonLib.Services;

using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns
{
    public class AcquiredColumn : CheckboxColumn
    {
        private readonly IUnlockTrackerService _unlockTrackerService;

        public AcquiredColumn(ILogger<AcquiredColumn> logger, ImGuiService imGuiService, IUnlockTrackerService unlockTrackerService) : base(logger, imGuiService)
        {
            _unlockTrackerService = unlockTrackerService;
        }
        public override ColumnCategory ColumnCategory => ColumnCategory.Basic;

        public override bool? CurrentValue(ColumnConfiguration columnConfiguration, SearchResult searchResult)
        {
            var action = searchResult.Item.Base.ItemAction.ValueNullable;
            if (!ActionTypeExt.IsValidAction(action)) {
                return null;
            }
            return _unlockTrackerService.IsUnlocked(searchResult.Item);
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