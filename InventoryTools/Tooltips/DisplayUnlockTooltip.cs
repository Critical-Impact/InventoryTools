using System.Collections.Generic;
using System.Linq;
using AllaganLib.GameSheets.Sheets;
using CriticalCommonLib.Enums;
using CriticalCommonLib.Services;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Component.GUI;
using InventoryTools.Logic.Settings;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Tooltips;

public class DisplayUnlockTooltip : BaseTooltip
{
    private readonly TooltipItemUnlockStatusColorSetting _colorSetting;
    private readonly TooltipDisplayUnlockSetting _tooltipDisplayUnlockSetting;
    private readonly TooltipDisplayUnlockDisplayModeSetting _displayModeSetting;
    private readonly TooltipDisplayUnlockHideUnlockedSetting _hideUnlockedSetting;
    private readonly ShowTooltipsSetting _showTooltipsSetting;
    private readonly TooltipDisplayUnlockCharacterSetting _tooltipDisplayUnlockCharacterSetting;
    private readonly ICharacterMonitor _characterMonitor;
    private readonly IUnlockTrackerService _unlockTrackerService;

    public DisplayUnlockTooltip(ILogger<DisplayUnlockTooltip> logger, TooltipItemUnlockStatusColorSetting colorSetting, TooltipDisplayUnlockSetting tooltipDisplayUnlockSetting, TooltipDisplayUnlockDisplayModeSetting displayModeSetting, TooltipDisplayUnlockHideUnlockedSetting hideUnlockedSetting, ShowTooltipsSetting showTooltipsSetting, TooltipDisplayUnlockCharacterSetting tooltipDisplayUnlockCharacterSetting, ItemSheet itemSheet, InventoryToolsConfiguration configuration, IGameGui gameGui, ICharacterMonitor characterMonitor, IDalamudPluginInterface pluginInterface, IUnlockTrackerService unlockTrackerService) : base(6905, logger, itemSheet, configuration, gameGui, pluginInterface)
    {
        _colorSetting = colorSetting;
        _tooltipDisplayUnlockSetting = tooltipDisplayUnlockSetting;
        _displayModeSetting = displayModeSetting;
        _hideUnlockedSetting = hideUnlockedSetting;
        _showTooltipsSetting = showTooltipsSetting;
        _tooltipDisplayUnlockCharacterSetting = tooltipDisplayUnlockCharacterSetting;
        _characterMonitor = characterMonitor;
        _unlockTrackerService = unlockTrackerService;
    }

    public override bool IsEnabled => _showTooltipsSetting.CurrentValue(Configuration) && _tooltipDisplayUnlockSetting.CurrentValue(Configuration);
    public override unsafe void OnGenerateItemTooltip(NumberArrayData* numberArrayData, StringArrayData* stringArrayData)
    {
        if (!ShouldShow()) return;
        var item = HoverItem;
        if (item != null && item.CanBeAcquired)
        {
            _unlockTrackerService.IsUnlocked(item);
            TooltipService.ItemTooltipField itemTooltipField = TooltipService.ItemTooltipField.ItemDescription;
            SeString? seStr = null;
            if (GetTooltipVisibility(ItemTooltipFieldVisibility.Description))
            {
                itemTooltipField = TooltipService.ItemTooltipField.ItemDescription;
                seStr = GetTooltipString(stringArrayData, itemTooltipField);
            }

            if (seStr == null && GetTooltipVisibility(ItemTooltipFieldVisibility.Effects))
            {
                itemTooltipField = TooltipService.ItemTooltipField.Effects;
                seStr = GetTooltipString(stringArrayData, itemTooltipField);
            }

            if (seStr == null && GetTooltipVisibility(ItemTooltipFieldVisibility.Levels))
            {
                itemTooltipField = TooltipService.ItemTooltipField.Levels;
                seStr = GetTooltipString(stringArrayData, itemTooltipField);
            }

            if(seStr == null)
            {
                return;
            }

            if (seStr.Payloads.Any(payload =>
                    payload is DalamudLinkPayload linkPayload && linkPayload.CommandId == TooltipIdentifier))
            {
                return;
            }
            seStr.Payloads.Add(GetLinkPayload());
            seStr.Payloads.Add(RawPayload.LinkTerminator);

            var characterSetting = _tooltipDisplayUnlockCharacterSetting.CurrentValue(Configuration);
            var hideUnlockedSetting = _hideUnlockedSetting.CurrentValue(Configuration);
            var displayModeSetting = _displayModeSetting.CurrentValue(Configuration);


            var unlockStatuses = Configuration.AcquiredItems
                .Where(c => characterSetting.Count == 0 || characterSetting.Contains(c.Key))
                .Where(c => _characterMonitor.Characters.ContainsKey(c.Key))
                .Select(c => (c.Key, c.Value.Contains(item.RowId)))
                .Where(c => !hideUnlockedSetting || !c.Item2).ToList();

            var newText = "";

            if (displayModeSetting == TooltipDisplayUnlockDisplayMode.CharacterPerLine)
            {
                var textLines = unlockStatuses.Select(c => _characterMonitor.GetCharacterById(c.Key)!.FormattedName + " - " + (c.Item2 ? "Acquired" : "Not Acquired") + "\n").OrderBy(c => c).ToList();
                if (textLines.Count != 0)
                {
                    newText += "\n";
                    for (var index = 0; index < textLines.Count; index++)
                    {
                        var line = textLines[index];
                        if (index == textLines.Count)
                        {
                            line = line.TrimEnd('\n');
                        }
                        newText += line;
                    }
                }
            }
            else
            {
                var unlocked = unlockStatuses.Where(c => c.Item2).ToList();
                var locked = unlockStatuses.Where(c => !c.Item2).ToList();
                if (locked.Count != 0)
                {
                    newText += "Not Acquired:\n";
                    foreach (var lockedItem in locked)
                    {
                        newText += _characterMonitor.GetCharacterById(lockedItem.Key)!.FormattedName + "\n";
                    }
                }

                if (unlocked.Count != 0)
                {
                    newText += "Acquired:\n";
                    foreach (var lockedItem in unlocked)
                    {
                        newText += _characterMonitor.GetCharacterById(lockedItem.Key)!.FormattedName + "\n";
                    }
                }

                if (locked.Count != 0 || unlocked.Count != 0)
                {
                    newText = "\n" + newText;
                }
            }

            newText = newText.TrimEnd('\n');
            if (newText != "")
            {
                var lines = new List<Payload>()
                {
                    new UIForegroundPayload((ushort)(_colorSetting.CurrentValue(Configuration) ?? Configuration.TooltipColor ?? 1)),
                    new UIGlowPayload(0),
                    new TextPayload(newText),
                    new UIGlowPayload(0),
                    new UIForegroundPayload(0),

                };
                foreach (var line in lines)
                {
                    seStr.Payloads.Add(line);
                }
                SetTooltipString(stringArrayData, itemTooltipField, seStr);
            }
        }
    }

    public override uint Order => 1;
}