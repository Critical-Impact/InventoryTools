using System.Collections.Generic;
using System.Linq;
using AllaganLib.GameSheets.Sheets;
using CriticalCommonLib.Enums;
using CriticalCommonLib.Services;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Component.GUI;
using InventoryTools.Logic;
using InventoryTools.Logic.Settings;
using InventoryTools.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Tooltips;

public class CuratedListTooltip : BaseTooltip
{
    private readonly IListService _listService;
    private readonly ShowTooltipsSetting _showTooltipsSetting;
    private readonly TooltipDisplayCuratedListsSetting _displaySetting;
    private readonly TooltipCuratedListsSetting _listsSetting;
    private readonly TooltipCuratedListsMatchQualitySetting _matchQualitySetting;
    private readonly TooltipCuratedListsColorSetting _colorSetting;

    public CuratedListTooltip(ILogger<CuratedListTooltip> logger, IListService listService,
        ShowTooltipsSetting showTooltipsSetting, TooltipDisplayCuratedListsSetting displaySetting,
        TooltipCuratedListsSetting listsSetting, TooltipCuratedListsMatchQualitySetting matchQualitySetting,
        TooltipCuratedListsColorSetting colorSetting, ItemSheet itemSheet, InventoryToolsConfiguration configuration,
        IGameGui gameGui, IChatGui chatGui) : base(6911, logger, itemSheet, configuration, gameGui, chatGui)
    {
        _listService = listService;
        _showTooltipsSetting = showTooltipsSetting;
        _displaySetting = displaySetting;
        _listsSetting = listsSetting;
        _matchQualitySetting = matchQualitySetting;
        _colorSetting = colorSetting;
    }

    public override bool IsEnabled =>
        _showTooltipsSetting.CurrentValue(Configuration) && _displaySetting.CurrentValue(Configuration);

    public override unsafe void OnGenerateItemTooltip(NumberArrayData* numberArrayData,
        StringArrayData* stringArrayData)
    {
        if (!ShouldShow()) return;
        var item = HoverItem;
        if (item == null) return;

        var selectedLists = _listsSetting.CurrentValue(Configuration);
        var matchQuality = _matchQualitySetting.CurrentValue(Configuration);
        var itemId = HoverItemId;
        var itemFlags = HoverItemFlags;

        var matchedLists = _listService.Lists
            .Where(c => c.FilterType == FilterType.CuratedList)
            .Where(c => selectedLists.Count == 0 || selectedLists.Contains(c.Key))
            .Where(c => c.CuratedItems?.Any(curatedItem =>
                curatedItem.ItemId == itemId && (!matchQuality || curatedItem.ItemFlags == itemFlags)) ?? false)
            .Select(c => c.NameFormatted)
            .ToList();

        if (matchedLists.Count == 0)
        {
            return;
        }

        var itemTooltipField = TooltipService.ItemTooltipField.ItemDescription;
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

        if (seStr == null) return;

        if (seStr.Payloads.Any(payload =>
                payload is DalamudLinkPayload linkPayload && linkPayload.CommandId == TooltipIdentifier))
        {
            return;
        }

        seStr.Payloads.Add(GetLinkPayload());
        seStr.Payloads.Add(RawPayload.LinkTerminator);

        var newText = "\nCurated Lists: " + string.Join(", ", matchedLists);

        var lines = new List<Payload>
        {
            new UIForegroundPayload((ushort)(_colorSetting.CurrentValue(Configuration) ??
                                             Configuration.TooltipColor ?? 1)),
            new UIGlowPayload(0),
            new TextPayload(newText),
            new UIGlowPayload(0),
            new UIForegroundPayload(0)
        };

        foreach (var line in lines)
        {
            seStr.Payloads.Add(line);
        }

        SetTooltipString(stringArrayData, itemTooltipField, seStr);
    }

    public override uint Order => 6;
}