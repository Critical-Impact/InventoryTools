using System;
using System.Collections.Generic;
using System.Linq;
using AllaganLib.GameSheets.Sheets.Rows;
using AllaganLib.Interface.FormFields;
using CriticalCommonLib.Models;
using InventoryTools.Localizers;
using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters;

public class GroupedGroupByFilter : FlagsEnumFilter<GroupedItemGroup>
{
    private readonly GroupedItemLocalizer _groupedItemLocalizer;

    public GroupedGroupByFilter(ILogger<GroupedGroupByFilter> logger, ImGuiService imGuiService, GroupedItemLocalizer groupedItemLocalizer) : base(logger, imGuiService)
    {
        _groupedItemLocalizer = groupedItemLocalizer;
    }

    public override GroupedItemGroup DefaultValue { get; set; } = GroupedItemGroup.Character;

    public override bool? FilterItem(FilterConfiguration configuration, InventoryItem item)
    {
        return null;
    }

    public override bool? FilterItem(FilterConfiguration configuration, ItemRow item)
    {
        return null;
    }


    public override GroupedItemGroup CurrentValue(FilterConfiguration configuration)
    {
        return configuration.Get(this.Key, DefaultValue) ?? DefaultValue;
    }

    public override void ResetFilter(FilterConfiguration configuration)
    {
        UpdateFilterConfiguration(configuration, DefaultValue);
    }

    public override void UpdateFilterConfiguration(FilterConfiguration configuration, GroupedItemGroup newValue)
    {
        configuration.Set(this.Key, newValue);
    }

    public override string Key { get; set; } = "GroupedGroupBy";
    public override string Name { get; set; } = "Group By?";
    public override string HelpText { get; set; } = "What should the grouped list be grouped by?";
    public override FilterCategory FilterCategory { get; set; } = FilterCategory.Grouping;

    public override GroupedItemGroup AddFlag(GroupedItemGroup currentFlags, GroupedItemGroup newFlag)
    {
        return currentFlags | newFlag;
    }

    public override GroupedItemGroup RemoveFlag(GroupedItemGroup currentFlags, GroupedItemGroup existingFlag)
    {
        return currentFlags & ~existingFlag;
    }

    public override bool FlagEmpty(GroupedItemGroup flag)
    {
        return flag == GroupedItemGroup.None;
    }

    public override string GetComboLabel(FilterConfiguration configuration)
    {
        var currentValue = this.CurrentValue(configuration);
        var choices = this.GetChoices(configuration);
        return "Group by " + string.Join(
            ", ",
            choices.Where(
                    c => (c.Key != GroupedItemGroup.None && currentValue.HasFlag(c.Key)) ||
                         (currentValue == GroupedItemGroup.None && c.Key == GroupedItemGroup.None))
                .Select(c => c.Value));
    }

    public override Dictionary<GroupedItemGroup, string> GetChoices(FilterConfiguration configuration)
    {
        var values = Enum.GetValues<GroupedItemGroup>();
        return values.ToDictionary(c => c, c => _groupedItemLocalizer.FormattedName(c));
    }

    public override bool HideAlreadyPicked { get; set; } = false;
}