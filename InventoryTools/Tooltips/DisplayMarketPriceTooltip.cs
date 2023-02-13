using System;
using System.Collections.Generic;
using System.Linq;
using CriticalCommonLib;
using CriticalCommonLib.Enums;
using CriticalCommonLib.Extensions;
using CriticalCommonLib.Services;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using InventoryTools.Logic;
using InventoryTools.Logic.Settings;

namespace InventoryTools.Tooltips;

public class DisplayMarketPriceTooltip : TooltipService.TooltipTweak
{
    private const string indentation = "      ";
    public override unsafe void OnGenerateItemTooltip(NumberArrayData* numberArrayData, StringArrayData* stringArrayData)
    {
        if (!ConfigurationManager.Config.DisplayTooltip)
        {
            return;
        }
        var itemId = Service.Gui.HoveredItem;
        if (itemId < 2000000)
        {
            bool isHq = itemId > 1000000;
            itemId %= 500000;
            

            var item = Service.ExcelCache.GetItemExSheet().GetRow((uint)itemId);
            if (item != null) {
                var textLines = new List<string>();
                TooltipService.ItemTooltipField itemTooltipField;
                var tooltipVisibility = GetTooltipVisibility((int**)numberArrayData);
                if (tooltipVisibility.HasFlag(ItemTooltipFieldVisibility.Description))
                {
                    itemTooltipField = TooltipService.ItemTooltipField.ItemDescription;
                }
                else if (tooltipVisibility.HasFlag(ItemTooltipFieldVisibility.Effects))
                {
                    itemTooltipField = TooltipService.ItemTooltipField.Effects;
                }
                else if (tooltipVisibility.HasFlag(ItemTooltipFieldVisibility.Levels))
                {
                    itemTooltipField = TooltipService.ItemTooltipField.Levels;
                }
                else
                {
                    return;
                }
                
                var seStr = GetTooltipString(stringArrayData, itemTooltipField);

                if (seStr != null && seStr.Payloads.Count > 0)
                {
                    if (ConfigurationManager.Config.TooltipDisplayMarketAveragePrice ||
                        ConfigurationManager.Config.TooltipDisplayMarketLowestPrice)
                    {
                        if (!(Service.ExcelCache.GetItemExSheet().GetRow((uint)itemId)?.IsUntradable ?? true))
                        {
                            var marketData = PluginService.MarketCache.GetPricing((uint)itemId, false);
                            if (marketData != null)
                            {
                                textLines.Add("Market Board Data:\n");
                                if (ConfigurationManager.Config.TooltipDisplayMarketAveragePrice)
                                {
                                    textLines.Add($"{indentation}Average Price: {Math.Round(marketData.averagePriceNQ, 0)}\n");
                                    textLines.Add(
                                        $"{indentation}Average Price (HQ): {Math.Round(marketData.averagePriceHQ, 0)}\n");
                                }

                                if (ConfigurationManager.Config.TooltipDisplayMarketLowestPrice)
                                {
                                    textLines.Add($"{indentation}Minimum Price: {Math.Round(marketData.minPriceNQ, 0)}\n");
                                    textLines.Add($"{indentation}Minimum Price (HQ): {Math.Round(marketData.minPriceHQ, 0)}\n");
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
                            new UIForegroundPayload((ushort)(ConfigurationManager.Config.TooltipColor ?? 1)),
                            new UIGlowPayload(0),
                            new TextPayload(newText),
                            new UIGlowPayload(0),
                            new UIForegroundPayload(0),
                        };
                        foreach (var line in lines)
                        {
                            seStr.Payloads.Add(line);
                        }

                        SetTooltipString(stringArrayData, TooltipService.ItemTooltipField.ItemDescription, seStr);
                    }
                }
            }
        }
    }
}