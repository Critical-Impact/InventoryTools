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

public class HeaderTextTooltip : TooltipService.TooltipTweak
{
    public override bool IsEnabled =>
        ConfigurationManager.Config.DisplayTooltip && (ConfigurationManager.Config.TooltipHeaderLines != 0 ||
                                                       ConfigurationManager.Config.TooltipDisplayHeader);

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
                    var newText = "";
                    if (ConfigurationManager.Config.TooltipHeaderLines != 0)
                    {
                        for (int i = 0; i < ConfigurationManager.Config.TooltipHeaderLines; i++)
                        {
                            newText += "\n";
                        }
                    }
                    if (ConfigurationManager.Config.TooltipDisplayHeader)
                    {
                        newText += "\n[Allagan Tools]";
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