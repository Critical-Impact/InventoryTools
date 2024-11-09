using System.Collections.Generic;
using InventoryTools.Logic.Columns.Abstract.ColumnSettings;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns.ColumnSettings;

public enum StainColumnSettingEnum
{
    FirstStain,
    SecondStain,
    Both
}

public class StainColumnSetting : ChoiceColumnSetting<StainColumnSettingEnum?>
{
    public StainColumnSetting(ILogger<StainColumnSetting> logger, ImGuiService imGuiService) : base(logger, imGuiService)
    {
    }

    public override StainColumnSettingEnum? CurrentValue(ColumnConfiguration configuration)
    {
        configuration.GetSetting(Key, out uint? currentValue);
        if (currentValue == null)
        {
            return DefaultValue;
        }

        return (StainColumnSettingEnum)currentValue;
    }

    public override void ResetFilter(ColumnConfiguration configuration)
    {
        configuration.SetSetting(Key, (uint?)null);
    }

    public override void UpdateColumnConfiguration(ColumnConfiguration configuration, StainColumnSettingEnum? newValue)
    {
        configuration.SetSetting(Key, (uint?)newValue);
    }

    public override string Key { get; set; } = "Display Mode";
    public override string Name { get; set; } = "Display Mode";
    public override string HelpText { get; set; } = "Choose the display mode of the dye column";
    public override StainColumnSettingEnum? DefaultValue { get; set; } = StainColumnSettingEnum.Both;
    public override List<StainColumnSettingEnum?> GetChoices(ColumnConfiguration configuration)
    {
        return
        [
            StainColumnSettingEnum.FirstStain,
            StainColumnSettingEnum.SecondStain,
            StainColumnSettingEnum.Both
        ];
    }

    public override string GetFormattedChoice(ColumnConfiguration filterConfiguration, StainColumnSettingEnum? choice)
    {
        return choice switch
        {
            StainColumnSettingEnum.FirstStain => "First Dye Only",
            StainColumnSettingEnum.SecondStain => "Second Dye Only",
            StainColumnSettingEnum.Both => "Both Dyes",
            _ => "Not Set"
        };
    }
}