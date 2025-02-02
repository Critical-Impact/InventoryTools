using System.Collections.Generic;
using System.Linq;
using AllaganLib.GameSheets.Sheets;
using CriticalCommonLib.Enums;
using CriticalCommonLib.Extensions;
using CriticalCommonLib.Services;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Component.GUI;
using InventoryTools.Logic.Editors;
using InventoryTools.Logic.Settings;
using Microsoft.Extensions.Logging;
using OtterGui;

namespace InventoryTools.Tooltips;

public class AmountOwnedTooltip : BaseTooltip
{
    private readonly TooltipAmountOwnedColorSetting _colorSetting;
    private readonly ICharacterMonitor _characterMonitor;
    private readonly IInventoryMonitor _inventoryMonitor;
    private readonly InventoryScopeCalculator _inventoryScopeCalculator;

    public AmountOwnedTooltip(ILogger<AmountOwnedTooltip> logger, TooltipAmountOwnedColorSetting colorSetting, ItemSheet itemSheet, InventoryToolsConfiguration configuration, IGameGui gameGui, ICharacterMonitor characterMonitor, IInventoryMonitor inventoryMonitor, InventoryScopeCalculator inventoryScopeCalculator, IDalamudPluginInterface pluginInterface) : base(6900, logger, itemSheet, configuration, gameGui, pluginInterface)
    {
        _colorSetting = colorSetting;
        _characterMonitor = characterMonitor;
        _inventoryMonitor = inventoryMonitor;
        _inventoryScopeCalculator = inventoryScopeCalculator;
    }
    private const string indentation = "      ";

    public override bool IsEnabled => Configuration.DisplayTooltip && Configuration.TooltipDisplayAmountOwned;
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

            var sortMode = Configuration.TooltipAmountOwnedSort;

            var enumerable = _inventoryMonitor.AllItems.Where(item =>
                item.ItemId == HoverItemId &&
                _characterMonitor.Characters.ContainsKey(item.RetainerId) &&
                ((Configuration.TooltipCurrentCharacter &&
                  _characterMonitor.BelongsToActiveCharacter(item.RetainerId)) ||
                 !Configuration.TooltipCurrentCharacter)
            );
            if (Configuration.TooltipSearchScope != null && Configuration.TooltipSearchScope.Count != 0)
            {
                enumerable = enumerable.Where(c => _inventoryScopeCalculator.Filter(Configuration.TooltipSearchScope, c));
            }

            if (sortMode == TooltipAmountOwnedSort.Alphabetically)
            {
                var characterNames = _characterMonitor.Characters.OrderBy(c => c.Value.FormattedName).ToList();
                enumerable = enumerable.OrderBy(c => characterNames.IndexOf(d => d.Key == c.RetainerId));
            }
            else if(sortMode == TooltipAmountOwnedSort.Categorically)
            {
                var characterNames = _characterMonitor.Characters.OrderBy(c => c.Value.FormattedName).ToList();
                enumerable = enumerable.OrderBy(c => c.SortedCategory.FormattedName()).ThenBy(c => characterNames.IndexOf(d => d.Key == c.RetainerId));
            }
            else if(sortMode == TooltipAmountOwnedSort.Quantity)
            {
                enumerable = enumerable.OrderByDescending(c => c.Quantity);
            }

            var ownedItems = enumerable
                .ToList();



            uint storageCount = 0;
            List<string> locations = new List<string>();

            if (Configuration.TooltipLocationDisplayMode ==
                TooltipLocationDisplayMode.CharacterBagSlotQuality)
            {
                foreach (var oItem in ownedItems)
                {
                    storageCount += oItem.Quantity;

                    if (locations.Count >= Configuration.TooltipLocationLimit)
                        continue;

                    var name = _characterMonitor.GetCharacterNameById(oItem.RetainerId);
                    if (Configuration.TooltipAddCharacterNameOwned)
                    {
                        var owner = _characterMonitor.GetCharacterNameById(
                            oItem.RetainerId, true);
                        if (owner.Trim().Length != 0)
                            name += " (" + owner + ")";
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
                if (ownedItems.Count > Configuration.TooltipLocationLimit)
                {
                    locations.Add(ownedItems.Count - Configuration.TooltipLocationLimit + " other locations.");
                }
            }
            if (Configuration.TooltipLocationDisplayMode ==
                TooltipLocationDisplayMode.CharacterBagSlotQuantity)
            {
                foreach (var oItem in ownedItems)
                {
                    storageCount += oItem.Quantity;

                    if (locations.Count >= Configuration.TooltipLocationLimit)
                        continue;

                    var name = _characterMonitor.GetCharacterNameById(oItem.RetainerId);
                    if (Configuration.TooltipAddCharacterNameOwned)
                    {
                        var owner = _characterMonitor.GetCharacterNameById(
                            oItem.RetainerId, true);
                        if (owner.Trim().Length != 0)
                            name += " (" + owner + ")";
                    }

                    locations.Add($"{name} - {oItem.FormattedBagLocation} - {+ oItem.Quantity} ");
                }
                if (ownedItems.Count > Configuration.TooltipLocationLimit)
                {
                    locations.Add(ownedItems.Count - Configuration.TooltipLocationLimit + " other locations.");
                }
            }
            else if (Configuration.TooltipLocationDisplayMode == TooltipLocationDisplayMode.CharacterCategoryQuantityQuality)
            {
                var groupedItems = ownedItems.GroupBy(c => (c.RetainerId, c.SortedCategory, c.Flags)).ToList();
                foreach (var oGroup in groupedItems)
                {
                    var quantity = oGroup.Sum(c => c.Quantity);
                    storageCount += (uint)quantity;

                    if (locations.Count >= Configuration.TooltipLocationLimit)
                        continue;

                    var name = _characterMonitor.GetCharacterNameById(oGroup.Key.RetainerId);
                    if (Configuration.TooltipAddCharacterNameOwned)
                    {
                        var owner = _characterMonitor.GetCharacterNameById(
                            oGroup.Key.RetainerId, true);
                        if (owner.Trim().Length != 0)
                            name += " (" + owner + ")";
                    }

                    var typeIcon = "";
                    if ((oGroup.Key.Flags & FFXIVClientStructs.FFXIV.Client.Game.InventoryItem.ItemFlags.HighQuality) != 0)
                    {
                        typeIcon = "\uE03c";
                    }
                    else if ((oGroup.Key.Flags & FFXIVClientStructs.FFXIV.Client.Game.InventoryItem.ItemFlags.Collectable) != 0)
                    {
                        typeIcon = "\uE03d";
                    }

                    locations.Add($"{name} - {oGroup.Key.SortedCategory.FormattedName()} - " + quantity + " " + typeIcon);
                }
                if (groupedItems.Count > Configuration.TooltipLocationLimit)
                {
                    locations.Add(groupedItems.Count - Configuration.TooltipLocationLimit + " other locations.");
                }
            }
            else if (Configuration.TooltipLocationDisplayMode == TooltipLocationDisplayMode.CharacterQuantityQuality)
            {
                var groupedItems = ownedItems.GroupBy(c => (c.RetainerId, c.Flags)).ToList();
                foreach (var oGroup in groupedItems)
                {
                    var quantity = oGroup.Sum(c => c.Quantity);
                    storageCount += (uint)quantity;

                    if (locations.Count >= Configuration.TooltipLocationLimit)
                        continue;

                    var name = _characterMonitor.GetCharacterNameById(oGroup.Key.RetainerId);
                    if (Configuration.TooltipAddCharacterNameOwned)
                    {
                        var owner = _characterMonitor.GetCharacterNameById(
                            oGroup.Key.RetainerId, true);
                        if (owner.Trim().Length != 0)
                            name += " (" + owner + ")";
                    }

                    var typeIcon = "";
                    if ((oGroup.Key.Flags & FFXIVClientStructs.FFXIV.Client.Game.InventoryItem.ItemFlags.HighQuality) != 0)
                    {
                        typeIcon = "\uE03c";
                    }
                    else if ((oGroup.Key.Flags & FFXIVClientStructs.FFXIV.Client.Game.InventoryItem.ItemFlags.Collectable) != 0)
                    {
                        typeIcon = "\uE03d";
                    }

                    locations.Add($"{name} - " + quantity + " " + typeIcon);
                }
                if (groupedItems.Count > Configuration.TooltipLocationLimit)
                {
                    locations.Add(groupedItems.Count - Configuration.TooltipLocationLimit + " other locations.");
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

    public override uint Order => 1;
}