using System.Collections.Generic;
using System.Linq;
using CriticalCommonLib.Services;
using InventoryTools.Logic.Settings.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Settings;

public class TooltipDisplayUnlockCharacterSetting : MultipleChoiceSetting<ulong>
{
    private readonly ICharacterMonitor _characterMonitor;

    public TooltipDisplayUnlockCharacterSetting(ICharacterMonitor characterMonitor, ILogger<TooltipDisplayUnlockCharacterSetting> logger, ImGuiService imGuiService) : base(logger, imGuiService)
    {
        _characterMonitor = characterMonitor;
    }

    public override List<ulong> DefaultValue { get; set; } = new();
    public override List<ulong> CurrentValue(InventoryToolsConfiguration configuration)
    {
        return configuration.TooltipDisplayUnlockCharacters ?? DefaultValue;
    }

    public override void UpdateFilterConfiguration(InventoryToolsConfiguration configuration, List<ulong> newValue)
    {
        configuration.TooltipDisplayUnlockCharacters = newValue;
    }

    public override string Key { get; set; } = "TooltipDisplayUnlockCharacter";
    public override string Name { get; set; } = "Add Item Unlock Status (Characters)";
    public override string HelpText { get; set; } = "When showing the unlock status on items, these characters will be displayed. Leave empty to display all characters.";
    public override SettingCategory SettingCategory { get; set; } = SettingCategory.ToolTips;
    public override SettingSubCategory SettingSubCategory { get; } = SettingSubCategory.ItemUnlockStatus;
    public override string Version { get; } = "1.11.0.4";
    public override Dictionary<ulong, string> GetChoices(InventoryToolsConfiguration configuration)
    {
        return _characterMonitor.GetPlayerCharacters().ToDictionary(c => c.Key, c => c.Value.FormattedName);
    }

    public override string GetPreviewValue(List<ulong> items)
    {
        if (items.Count == 0)
        {
            return "All characters will be shown.";
        }
        else
        {
            return $"{items.Count} characters will be shown..";
        }
    }

    public override bool HideAlreadyPicked { get; set; } = false;
}