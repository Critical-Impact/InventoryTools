using System;
using System.Collections.Generic;
using System.Linq;
using InventoryTools.Logic.Columns.Abstract.ColumnSettings;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns.ColumnSettings;

public class ButtonColumnSetting : MultiChoiceColumnSetting<ButtonType?>
{
    public ButtonColumnSetting(ILogger<ButtonColumnSetting> logger, ImGuiService imGuiService) : base(logger, imGuiService)
    {
    }

    public override List<ButtonType?> CurrentValue(ColumnConfiguration configuration)
    {
        configuration.GetSetting(Key, out List<ButtonType>? value);
        if (value == null)
        {
            return [];
        }

        return value.Select(c => (ButtonType?)c).ToList();
    }


    public override void ResetFilter(ColumnConfiguration configuration)
    {
        configuration.SetSetting<ButtonType>(Key, null);
    }


    public override void UpdateColumnConfiguration(ColumnConfiguration configuration, List<ButtonType?>? newValue)
    {
        configuration.SetSetting(Key, newValue?.Count == 0 ? null : newValue?.Select(c => c!.Value).ToList());
    }

    public override string Key { get; set; } = "ButtonTypes";
    public override string Name { get; set; } = "Button Types";
    public override string HelpText { get; set; } = "The buttons to display";

    public override List<ButtonType?> DefaultValue { get; set; } = new();
    public override List<ButtonType?> GetChoices(ColumnConfiguration configuration)
    {
        return [ButtonType.CraftLog, ButtonType.Buy, ButtonType.Gather, ButtonType.Action];
    }

    public override string GetFormattedChoice(ColumnConfiguration filterConfiguration, ButtonType? choice)
    {
        switch (choice)
        {
            case ButtonType.CraftLog:
                return "Craft Log";
            case ButtonType.Buy:
                return "Buy";
            case ButtonType.Gather:
                return "Gather Log";
            case ButtonType.Action:
                return "Action Button";
        }

        return choice.ToString() ?? "";
    }
}

public enum ButtonType
{
    CraftLog,
    Buy,
    Gather,
    Action
}