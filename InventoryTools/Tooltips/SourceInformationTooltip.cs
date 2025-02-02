using System.Collections.Generic;
using System.Linq;
using AllaganLib.GameSheets.Sheets;
using CriticalCommonLib.Enums;
using CriticalCommonLib.Services;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Component.GUI;
using InventoryTools.Logic.Settings;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Tooltips;

public class SourceInformationTooltip : BaseTooltip
{
    private readonly TooltipSourceInformationColorSetting _colorSetting;
    private readonly ItemInfoRenderService _itemInfoRenderService;
    private readonly ShowTooltipsSetting _showTooltipsSetting;
    private readonly TooltipSourceInformationSetting _sourceInformationSetting;
    private readonly TooltipSourceInformationEnabledSetting _enabledSetting;
    private readonly TooltipSourceInformationModifierSetting _modifierSetting;
    private readonly IKeyState _keyState;
    private readonly IUnlockTrackerService _unlockTrackerService;

    public SourceInformationTooltip(ILogger<SourceInformationTooltip> logger, TooltipSourceInformationColorSetting colorSetting, ItemInfoRenderService itemInfoRenderService, ShowTooltipsSetting showTooltipsSetting, TooltipSourceInformationSetting sourceInformationSetting, TooltipSourceInformationEnabledSetting enabledSetting, TooltipSourceInformationModifierSetting modifierSetting, IKeyState keyState, ItemSheet itemSheet, InventoryToolsConfiguration configuration, IGameGui gameGui, IDalamudPluginInterface pluginInterface, IUnlockTrackerService unlockTrackerService) : base(6906, logger, itemSheet, configuration, gameGui, pluginInterface)
    {
        _colorSetting = colorSetting;
        _itemInfoRenderService = itemInfoRenderService;
        _showTooltipsSetting = showTooltipsSetting;
        _sourceInformationSetting = sourceInformationSetting;
        _enabledSetting = enabledSetting;
        _modifierSetting = modifierSetting;
        _keyState = keyState;
        _unlockTrackerService = unlockTrackerService;
    }

    public override bool IsEnabled => _showTooltipsSetting.CurrentValue(Configuration) && _enabledSetting.CurrentValue(Configuration);
    public override unsafe void OnGenerateItemTooltip(NumberArrayData* numberArrayData, StringArrayData* stringArrayData)
    {
        if (!ShouldShow()) return;
        var modifier = _modifierSetting.CurrentValue(Configuration);
        if (modifier == TooltipSourceModifier.Shift && !_keyState[VirtualKey.SHIFT])
        {
            return;
        }
        if (modifier == TooltipSourceModifier.Control && !_keyState[VirtualKey.CONTROL])
        {
            return;
        }
        var item = HoverItem;
        if (item != null)
        {
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

            var currentValue = _sourceInformationSetting.CurrentValue(Configuration);

            var groupedLines = item.Sources
                .Where(c => !currentValue.ContainsKey(c.Type) || currentValue[c.Type].Show)
                .OrderBy(c => currentValue.TryGetValue(c.Type, out var value) ? value.Order : 999)
                .GroupBy(c => c.Type);

            var textLines = new List<string>();

            foreach (var groupedLine in groupedLines)
            {
                if (currentValue[groupedLine.Key].Group == false)
                {
                    foreach (var line in groupedLine)
                    {
                        textLines.Add(_itemInfoRenderService.GetSourceName(line));
                    }
                }
                else
                {
                    textLines.Add(_itemInfoRenderService.GetSourceTypeName(groupedLine.Key).Singular);
                }
            }

            var newText = "";
            if (textLines.Count != 0)
            {
                newText = "Sources: " + string.Join(", ", textLines.Distinct());
            }

            if (newText != "")
            {
                newText += "\n";
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