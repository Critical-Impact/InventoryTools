using System;
using System.Collections.Generic;
using System.Linq;
using AllaganLib.GameSheets.Sheets;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Enums;
using CriticalCommonLib.Services;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Component.GUI;
using InventoryTools.Logic;
using InventoryTools.Logic.Settings;
using InventoryTools.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Tooltips;

public class LocationDisplayTooltip : BaseTooltip
{
    private readonly TooltipAmountToRetrieveColorSetting _colorSetting;
    private readonly IListService _listService;

    public LocationDisplayTooltip(ILogger<LocationDisplayTooltip> logger, TooltipAmountToRetrieveColorSetting colorSetting, ItemSheet itemSheet, InventoryToolsConfiguration configuration, IGameGui gameGui, IListService listService, IDalamudPluginInterface pluginInterface) : base(6904, logger, itemSheet, configuration, gameGui, pluginInterface)
    {
        _colorSetting = colorSetting;
        _listService = listService;
    }
    public override bool IsEnabled =>
        Configuration.DisplayTooltip && Configuration.TooltipDisplayRetrieveAmount;
    public override unsafe void OnGenerateItemTooltip(NumberArrayData* numberArrayData, StringArrayData* stringArrayData)
    {
        if (!ShouldShow()) return;

        var item = HoverItem;
        if (item != null) {
            var textLines = new List<string>();

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

            if (seStr.Payloads.Count > 0)
            {
                if (Configuration.TooltipDisplayRetrieveAmount)
                {
                    var filterConfiguration = _listService.GetActiveList();
                    if (filterConfiguration != null)
                    {
                        if (filterConfiguration.FilterType == FilterType.CraftFilter)
                        {
                            var hoverItemIsHq = HoverItemIsHq;
                            var hoverItemId = HoverItemId;
                            var craftItem = filterConfiguration.CraftList.GetItemById(hoverItemId, hoverItemIsHq, HoverItem?.Base.CanBeHq ?? false);
                            if (craftItem != null)
                            {
                                var filterResult = filterConfiguration.SearchResults;
                                var missingOverall = craftItem.QuantityMissingOverall;
                                var willRetrieve = craftItem.QuantityWillRetrieve;
                                if (missingOverall != 0 || willRetrieve != 0)
                                {
                                    var missingText = "Missing: ";
                                    if (craftItem.IngredientPreference.Type is IngredientPreferenceType.Buy
                                        or IngredientPreferenceType.Item or IngredientPreferenceType.HouseVendor)
                                    {
                                        missingText = "Buy: ";
                                    }
                                    var needText = missingText + missingOverall;
                                    if (filterResult != null)
                                    {
                                        var sortedItems = filterResult.Where(c => c.InventoryItem != null &&
                                            c.InventoryItem.ItemId == hoverItemId && c.InventoryItem.IsHQ == hoverItemIsHq).ToList();
                                        if (sortedItems.Any())
                                        {
                                            var sortedItem = sortedItems.First();
                                            if (sortedItem.InventoryItem!.Quantity != 0)
                                            {
                                                needText += " / (" + Math.Min(willRetrieve,sortedItem.InventoryItem!.Quantity) + " should be retrieved)";
                                            }
                                        }
                                    }

                                    textLines.Add(needText + "\n");
                                }
                            }
                        }
                    }
                }

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
    }
    public override uint Order => 2;
}