using System;
using System.Collections.Generic;
using System.Linq;
using CriticalCommonLib;
using CriticalCommonLib.Services;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using InventoryTools.Logic;

namespace InventoryTools.Tooltips;

public class LocationDisplayTooltip : TooltipService.TooltipTweak
{
    public override unsafe void OnGenerateItemTooltip(NumberArrayData* numberArrayData, StringArrayData* stringArrayData)
    {
        
        if (!ConfigurationManager.Config.DisplayTooltip || Service.KeyState[VirtualKey.CONTROL])
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
                var seStr = GetTooltipString(stringArrayData, TooltipService.ItemTooltipField.ItemDescription);

                if (seStr != null && seStr.Payloads.Count > 0) {

                    var payloadNotAltered = seStr.Payloads.OfType<TextPayload>().Where(p => p.Text != null).Count(p => p.Text!.Contains("Allagan Tools")) == 0;
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
                                    var sortedItems = filterResult.Value.SortedItems.Where(c => c.InventoryItem.ItemId == id && c.InventoryItem.IsHQ == isHq).ToList();
                                    if (sortedItems.Any())
                                    {
                                        var sortedItem = sortedItems.First();
                                        if (sortedItem.Quantity != 0)
                                        {
                                            textLines.Add("Retrieve: " + sortedItem.Quantity + "\n");
                                        }
                                    }
                                }
                            }
                        }
                    }                    
                    if (payloadNotAltered)
                    {
                        var newText = "";
                        if (textLines.Count != 0)
                        {
                            newText += "\n\n";
                            newText += "[Allagan Tools]\n";
                            foreach (var line in textLines)
                            {
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
                                new UIForegroundPayload(0),
                                new UIGlowPayload(0),
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
}