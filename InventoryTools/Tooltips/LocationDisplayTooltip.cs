using System;
using System.Collections.Generic;
using System.Linq;
using CriticalCommonLib.Enums;
using CriticalCommonLib.Services;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Component.GUI;
using InventoryTools.Logic;
using InventoryTools.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Tooltips;

public class LocationDisplayTooltip : BaseTooltip
{
    private readonly IListService _listService;

    public LocationDisplayTooltip(ILogger<LocationDisplayTooltip> logger,ExcelCache excelCache, InventoryToolsConfiguration configuration, IGameGui gameGui, IListService listService) : base(logger, excelCache, configuration, gameGui)
    {
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
                if (Configuration.TooltipDisplayRetrieveAmount)
                {
                    var filterConfiguration = _listService.GetActiveList();
                    if (filterConfiguration != null)
                    {
                        if (filterConfiguration.FilterType == FilterType.CraftFilter)
                        {
                            var hoverItemIsHq = HoverItemIsHq;
                            var hoverItemId = HoverItemId;
                            var craftItem = filterConfiguration.CraftList.GetItemById(hoverItemId, hoverItemIsHq, HoverItem?.CanBeHq ?? false);
                            if (craftItem != null)
                            {
                                var filterResult = filterConfiguration.SearchResults;
                                var missingOverall = craftItem.QuantityMissingOverall;
                                var willRetrieve = craftItem.QuantityWillRetrieve;
                                if (missingOverall != 0 || willRetrieve != 0)
                                {
                                    var needText = "Need: " + missingOverall;
                                    if (filterResult != null)
                                    {
                                        var sortedItems = filterResult.Where(c => c.InventoryItem != null &&
                                            c.InventoryItem.ItemId == hoverItemId && c.InventoryItem.IsHQ == hoverItemIsHq).ToList();
                                        if (sortedItems.Any())
                                        {
                                            var sortedItem = sortedItems.First();
                                            if (sortedItem.InventoryItem!.Quantity != 0)
                                            {
                                                needText += " / (" + Math.Min(willRetrieve,sortedItem.InventoryItem!.Quantity) + " can be retrieved)";
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
    }
    public override uint Order => 2;
}