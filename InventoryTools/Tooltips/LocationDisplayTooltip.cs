using System.Collections.Generic;
using System.Linq;
using CriticalCommonLib;
using CriticalCommonLib.Enums;
using CriticalCommonLib.Services;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using FFXIVClientStructs.FFXIV.Component.GUI;
using InventoryTools.Logic;

namespace InventoryTools.Tooltips;

public class LocationDisplayTooltip : TooltipService.TooltipTweak
{
    
    public override bool IsEnabled =>
        ConfigurationManager.Config.DisplayTooltip && ConfigurationManager.Config.TooltipDisplayRetrieveAmount;
    public override unsafe void OnGenerateItemTooltip(NumberArrayData* numberArrayData, StringArrayData* stringArrayData)
    {
        if (!ConfigurationManager.Config.DisplayTooltip)
        {
            return;
        }
        var id = Service.Gui.HoveredItem;
        if (id < 2000000)
        {
            bool isHq = id > 1000000;
            id %= 500000;
            

            var item = Service.ExcelCache.GetItemExSheet().GetRow((uint)id);
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
                    if (ConfigurationManager.Config.TooltipDisplayRetrieveAmount)
                    {
                        var filterConfiguration = PluginService.FilterService.GetActiveFilter();
                        if (filterConfiguration != null)
                        {
                            if (filterConfiguration.FilterType == FilterType.CraftFilter)
                            {
                                var filterResult = filterConfiguration.FilterResult;
                                if (filterResult != null)
                                {
                                    var neededItems = filterConfiguration.CraftList.GetMissingMaterialsList();
                                    uint? neededItemQty = neededItems.ContainsKey((uint)id) ? neededItems[(uint)id] : null;
                                    if (neededItemQty != null && neededItemQty != 0)
                                    {
                                        var sortedItems = filterResult.SortedItems.Where(c =>
                                            c.InventoryItem.ItemId == id && c.InventoryItem.IsHQ == isHq).ToList();
                                        var needText = "Need: " + neededItemQty;
                                        if (sortedItems.Any())
                                        {
                                            var sortedItem = sortedItems.First();
                                            if (sortedItem.Quantity != 0)
                                            {
                                                needText += " / (" + sortedItem.Quantity + " can be retrieved)";
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

                        SetTooltipString(stringArrayData, itemTooltipField, seStr);
                    }
                }
            }
        }
    }
}