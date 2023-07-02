using System.Collections.Generic;
using System.Linq;
using CriticalCommonLib.Extensions;
using CriticalCommonLib.Models;
using InventoryTools.Logic.Settings.Abstract;

namespace InventoryTools.Logic.Settings;

public class HistoryTrackEventsSetting : MultipleChoiceSetting<InventoryChangeReason>
{
    public override List<InventoryChangeReason> DefaultValue { get; set; } = new()
    {
        InventoryChangeReason.Added,
        InventoryChangeReason.Moved,
        InventoryChangeReason.Removed,
        InventoryChangeReason.Transferred,
        InventoryChangeReason.MarketPriceChanged,
        InventoryChangeReason.QuantityChanged
    };
    public override List<InventoryChangeReason> CurrentValue(InventoryToolsConfiguration configuration)
    {
        return configuration.HistoryTrackReasons;
    }

    public override void UpdateFilterConfiguration(InventoryToolsConfiguration configuration, List<InventoryChangeReason> newValue)
    {
        configuration.HistoryTrackReasons = newValue.Distinct().ToList();
    }

    public override string Key { get; set; } = "HistoryTrackEvents";
    public override string Name { get; set; } = "History Track Events";
    public override string HelpText { get; set; } = "Which events should be tracked by the history module?";
    public override SettingCategory SettingCategory { get; set; } = SettingCategory.History;
    public override SettingSubCategory SettingSubCategory { get; } = SettingSubCategory.General;
    private Dictionary<InventoryChangeReason, string>? _changeReasons;
    public override Dictionary<InventoryChangeReason, string> GetChoices(InventoryToolsConfiguration configuration)
    {
        if (_changeReasons == null)
        {
            _changeReasons = new Dictionary<InventoryChangeReason, string>()
            {
                {InventoryChangeReason.Added, InventoryChangeReason.Added.FormattedName()},
                {InventoryChangeReason.Removed, InventoryChangeReason.Removed.FormattedName()},
                {InventoryChangeReason.Transferred, InventoryChangeReason.Transferred.FormattedName()},
                {InventoryChangeReason.QuantityChanged, InventoryChangeReason.QuantityChanged.FormattedName()},
                {InventoryChangeReason.MarketPriceChanged, InventoryChangeReason.MarketPriceChanged.FormattedName()},
                {InventoryChangeReason.FlagsChanged, InventoryChangeReason.FlagsChanged.FormattedName()},
                {InventoryChangeReason.ConditionChanged, InventoryChangeReason.ConditionChanged.FormattedName()},
                {InventoryChangeReason.GearsetsChanged, InventoryChangeReason.GearsetsChanged.FormattedName()},
                {InventoryChangeReason.GlamourChanged, InventoryChangeReason.GlamourChanged.FormattedName()},
                {InventoryChangeReason.MateriaChanged, InventoryChangeReason.MateriaChanged.FormattedName()},
                {InventoryChangeReason.StainChanged, InventoryChangeReason.StainChanged.FormattedName()},
                {InventoryChangeReason.SpiritbondChanged, InventoryChangeReason.SpiritbondChanged.FormattedName()},
            };
        }

        return _changeReasons;
    }

    public override bool HideAlreadyPicked { get; set; }
}