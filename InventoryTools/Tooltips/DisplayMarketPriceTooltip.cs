using System;
using System.Collections.Generic;
using System.Linq;
using AllaganLib.GameSheets.Sheets;
using CriticalCommonLib.Enums;
using CriticalCommonLib.MarketBoard;
using CriticalCommonLib.Services;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Component.GUI;
using InventoryTools.Logic.Settings;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Tooltips;

public class DisplayMarketPriceTooltip : BaseTooltip
{
    private readonly TooltipMarketPricingColorSetting _colorSetting;
    private readonly ICharacterMonitor _characterMonitor;
    private readonly IMarketCache _marketCache;

    public DisplayMarketPriceTooltip(ILogger<DisplayMarketPriceTooltip> logger, TooltipMarketPricingColorSetting colorSetting, ItemSheet itemSheet, InventoryToolsConfiguration configuration, IGameGui gameGui, ICharacterMonitor characterMonitor, IMarketCache marketCache, IDalamudPluginInterface pluginInterface) : base(6901, logger, itemSheet, configuration, gameGui, pluginInterface)
    {
        _colorSetting = colorSetting;
        _characterMonitor = characterMonitor;
        _marketCache = marketCache;
    }
    private const string indentation = "      ";

    public override bool IsEnabled =>
        Configuration.DisplayTooltip &&
        (Configuration.TooltipDisplayMarketAveragePrice ||
         Configuration.TooltipDisplayMarketLowestPrice);

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
                if (Configuration.TooltipDisplayMarketAveragePrice ||
                    Configuration.TooltipDisplayMarketLowestPrice)
                {
                    var hoverItemId = HoverItemId;
                    if (!(ItemSheet.GetRowOrDefault(hoverItemId)?.Base.IsUntradable ?? true))
                    {
                        var activeCharacter = _characterMonitor.ActiveCharacter;
                        if (activeCharacter != null)
                        {
                            var marketData = _marketCache.GetPricing((uint)hoverItemId, activeCharacter.WorldId, false);
                            if (marketData != null)
                            {
                                textLines.Add("Market Board Data:\n");
                                if (Configuration.TooltipDisplayMarketAveragePrice)
                                {
                                    textLines.Add(
                                        $"{indentation}Average Price: {Math.Round(marketData.AveragePriceNq, 0)}\n");
                                    textLines.Add(
                                        $"{indentation}Average Price (HQ): {Math.Round(marketData.AveragePriceHq, 0)}\n");
                                }

                                if (Configuration.TooltipDisplayMarketLowestPrice)
                                {
                                    textLines.Add(
                                        $"{indentation}Minimum Price: {Math.Round(marketData.MinPriceNq, 0)}\n");
                                    textLines.Add(
                                        $"{indentation}Minimum Price (HQ): {Math.Round(marketData.MinPriceHq, 0)}\n");
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
    public override uint Order => 3;
}