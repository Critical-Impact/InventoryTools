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
    private readonly TooltipDisplayUnlockSetting _tooltipDisplayUnlockSetting;
    private readonly ShowTooltipsSetting _showTooltipsSetting;
    private readonly TooltipDisplayUnlockCharacterSetting _tooltipDisplayUnlockCharacterSetting;
    private readonly ICharacterMonitor _characterMonitor;
    private readonly IUnlockTrackerService _unlockTrackerService;

    public DisplayUnlockTooltip(ILogger<DisplayUnlockTooltip> logger, TooltipDisplayUnlockSetting tooltipDisplayUnlockSetting, ShowTooltipsSetting showTooltipsSetting, TooltipDisplayUnlockCharacterSetting tooltipDisplayUnlockCharacterSetting, ItemSheet itemSheet, InventoryToolsConfiguration configuration, IGameGui gameGui, ICharacterMonitor characterMonitor, IDalamudPluginInterface pluginInterface, IUnlockTrackerService unlockTrackerService) : base(6905, logger, itemSheet, configuration, gameGui, pluginInterface)
    {
        _tooltipDisplayUnlockSetting = tooltipDisplayUnlockSetting;
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

            var textLines = Configuration.AcquiredItems.
                Where(c => characterSetting.Count == 0 || characterSetting.Contains(c.Key)).
                Where(c => _characterMonitor.Characters.ContainsKey(c.Key)).
                Select(c => _characterMonitor.GetCharacterById(c.Key)!.FormattedName + " - " + (c.Value.Contains(item.RowId) ? "Acquired" : "Not Acquired") + "\n").OrderBy(c => c).ToList();

            var newText = "";
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

            if (newText != "")
            {
                var lines = new List<Payload>()
                {
                    new UIForegroundPayload((ushort)(Configuration.TooltipColor ?? 1)),
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