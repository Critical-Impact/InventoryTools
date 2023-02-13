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

public class AmountOwnedTooltip : TooltipService.TooltipTweak
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
                    if (ConfigurationManager.Config.TooltipDisplayAmountOwned)
                    {
                        var ownedItems = PluginService.InventoryMonitor.AllItems.Where(item => 
                                item.ItemId == itemId && 
                                PluginService.CharacterMonitor.Characters.ContainsKey(item.RetainerId) &&
                                ((ConfigurationManager.Config.TooltipCurrentCharacter && PluginService.CharacterMonitor.BelongsToActiveCharacter(item.RetainerId)) ||  !ConfigurationManager.Config.TooltipCurrentCharacter)
                                )
                            .ToList();
                            
                        uint storageCount = 0;
                        List<string> locations = new List<string>();
                        
                        if (ConfigurationManager.Config.TooltipLocationDisplayMode ==
                            TooltipLocationDisplayMode.CharacterBagSlotQuality)
                        {
                            foreach (var oItem in ownedItems)
                            {
                                storageCount += oItem.Quantity;
                                
                                if (locations.Count >= ConfigurationManager.Config.TooltipLocationLimit)
                                    continue;

                                var characterMonitorCharacter = PluginService.CharacterMonitor.Characters[oItem.RetainerId];
                                var name = characterMonitorCharacter?.FormattedName ?? "Unknown";
                                name = name.Trim().Length == 0 ? "Unknown" : name.Trim();
                                if (characterMonitorCharacter != null && characterMonitorCharacter.OwnerId != 0 &&
                                    ConfigurationManager.Config.TooltipAddCharacterNameOwned &&
                                    PluginService.CharacterMonitor.Characters.ContainsKey(characterMonitorCharacter
                                        .OwnerId))
                                {
                                    var owner = PluginService.CharacterMonitor.Characters[
                                        characterMonitorCharacter.OwnerId];
                                    name += " (" + owner.FormattedName + ")";
                                }

                                var typeIcon = "";
                                if (oItem.IsHQ)
                                {
                                    typeIcon = "\uE03c";
                                }
                                else if (oItem.IsCollectible)
                                {
                                    typeIcon = "\uE03d";
                                }

                                locations.Add($"{name} - {oItem.FormattedBagLocation} " + typeIcon);
                            }
                            if (ownedItems.Count > ConfigurationManager.Config.TooltipLocationLimit)
                            {
                                locations.Add(ownedItems.Count - ConfigurationManager.Config.TooltipLocationLimit + " other locations.");
                            }                        
                        }
                        else if (ConfigurationManager.Config.TooltipLocationDisplayMode == TooltipLocationDisplayMode.CharacterCategoryQuantityQuality)
                        {
                            var groupedItems = ownedItems.GroupBy(c => (c.RetainerId, c.SortedCategory, c.Flags)).ToList();
                            foreach (var oGroup in groupedItems)
                            {
                                var quantity = oGroup.Sum(c => c.Quantity);
                                storageCount += (uint)quantity;
                                
                                if (locations.Count >= ConfigurationManager.Config.TooltipLocationLimit)
                                    continue;

                                var characterMonitorCharacter = PluginService.CharacterMonitor.Characters[oGroup.Key.RetainerId];
                                var name = characterMonitorCharacter?.FormattedName ?? "Unknown";
                                name = name.Trim().Length == 0 ? "Unknown" : name.Trim();
                                if (characterMonitorCharacter != null && characterMonitorCharacter.OwnerId != 0 &&
                                    ConfigurationManager.Config.TooltipAddCharacterNameOwned &&
                                    PluginService.CharacterMonitor.Characters.ContainsKey(characterMonitorCharacter
                                        .OwnerId))
                                {
                                    var owner = PluginService.CharacterMonitor.Characters[
                                        characterMonitorCharacter.OwnerId];
                                    name += " (" + owner.FormattedName + ")";
                                }

                                var typeIcon = "";
                                if ((oGroup.Key.Flags & FFXIVClientStructs.FFXIV.Client.Game.InventoryItem.ItemFlags.HQ) != 0)
                                {
                                    typeIcon = "\uE03c";
                                }
                                else if ((oGroup.Key.Flags & FFXIVClientStructs.FFXIV.Client.Game.InventoryItem.ItemFlags.Collectable) != 0)
                                {
                                    typeIcon = "\uE03d";
                                }

                                locations.Add($"{name} - {oGroup.Key.SortedCategory.FormattedName()} - " + quantity + " " + typeIcon);
                            }
                            if (groupedItems.Count > ConfigurationManager.Config.TooltipLocationLimit)
                            {
                                locations.Add(groupedItems.Count - ConfigurationManager.Config.TooltipLocationLimit + " other locations.");
                            }  
                        }
                        else if (ConfigurationManager.Config.TooltipLocationDisplayMode == TooltipLocationDisplayMode.CharacterQuantityQuality)
                        {
                            var groupedItems = ownedItems.GroupBy(c => (c.RetainerId, c.Flags)).ToList();
                            foreach (var oGroup in groupedItems)
                            {
                                var quantity = oGroup.Sum(c => c.Quantity);
                                storageCount += (uint)quantity;
                                
                                if (locations.Count >= ConfigurationManager.Config.TooltipLocationLimit)
                                    continue;

                                var characterMonitorCharacter = PluginService.CharacterMonitor.Characters[oGroup.Key.RetainerId];
                                var name = characterMonitorCharacter?.FormattedName ?? "Unknown";
                                name = name.Trim().Length == 0 ? "Unknown" : name.Trim();
                                if (characterMonitorCharacter != null && characterMonitorCharacter.OwnerId != 0 &&
                                    ConfigurationManager.Config.TooltipAddCharacterNameOwned &&
                                    PluginService.CharacterMonitor.Characters.ContainsKey(characterMonitorCharacter
                                        .OwnerId))
                                {
                                    var owner = PluginService.CharacterMonitor.Characters[
                                        characterMonitorCharacter.OwnerId];
                                    name += " (" + owner.FormattedName + ")";
                                }

                                var typeIcon = "";
                                if ((oGroup.Key.Flags & FFXIVClientStructs.FFXIV.Client.Game.InventoryItem.ItemFlags.HQ) != 0)
                                {
                                    typeIcon = "\uE03c";
                                }
                                else if ((oGroup.Key.Flags & FFXIVClientStructs.FFXIV.Client.Game.InventoryItem.ItemFlags.Collectable) != 0)
                                {
                                    typeIcon = "\uE03d";
                                }

                                locations.Add($"{name} - " + quantity + " " + typeIcon);
                            }
                            if (groupedItems.Count > ConfigurationManager.Config.TooltipLocationLimit)
                            {
                                locations.Add(groupedItems.Count - ConfigurationManager.Config.TooltipLocationLimit + " other locations.");
                            }  
                        }

                        if (storageCount > 0)
                        {
                            textLines.Add($"Owned: {storageCount}\n");
                            textLines.Add($"Locations:\n");
                            for (var index = 0; index < locations.Count; index++)
                            {
                                var location = locations[index];
                                textLines.Add($"{indentation}{location}\n");
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