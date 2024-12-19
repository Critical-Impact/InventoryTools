using System.Collections.Generic;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Plugin.Services;
using InventoryTools.Logic.Settings.Abstract;
using InventoryTools.Logic.Settings.Abstract.Generic;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Settings;

public enum CraftOverlayHide
{
    AlwaysShow,
    HideDuringDuties
}

public class CraftOverlayHideSetting : GenericEnumChoiceSetting<CraftOverlayHide>
{
    private readonly IClientState _clientState;
    private readonly ICondition _condition;

    public CraftOverlayHideSetting(ILogger<CraftOverlayHideSetting> logger, ImGuiService imGuiService, IClientState clientState, ICondition condition) : base("CraftOverlayHide", "Hide during duties?", "Should the craft overlay be hidden during duties/cutscenes/chocobo racing/etc?", CraftOverlayHide.HideDuringDuties, new Dictionary<CraftOverlayHide, string>()
    {
        { CraftOverlayHide.AlwaysShow, "Always show" },
        { CraftOverlayHide.HideDuringDuties, "Hide during duties" },
    }, SettingCategory.CraftOverlay, SettingSubCategory.General, "1.11.0.9", logger, imGuiService)
    {
        _clientState = clientState;
        _condition = condition;
    }

    public bool ShouldShow()
    {
        return !_clientState.IsPvPExcludingDen
           && !_condition[ConditionFlag.BoundByDuty]
           && !_condition[ConditionFlag.WatchingCutscene]
           && !_condition[ConditionFlag.WatchingCutscene78]
           && !_condition[ConditionFlag.BoundByDuty95]
           && !_condition[ConditionFlag.BoundByDuty56]
           && !_condition[ConditionFlag.InDeepDungeon]
           && !_condition[ConditionFlag.PlayingLordOfVerminion]
           && !_condition[ConditionFlag.ChocoboRacing];
    }


}