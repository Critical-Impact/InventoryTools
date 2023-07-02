using System.Collections.Generic;
using System.Linq;
using CriticalCommonLib;
using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using ImGuiNET;
using InventoryTools.Logic.Columns.Abstract;
using OtterGui.Widgets;

namespace InventoryTools.Logic.Columns
{
    public class BestInSlotColumn : IntegerColumn
    {
        public override ColumnCategory ColumnCategory => ColumnCategory.Tools;
        
        public override bool IsDebug { get; set; } = true;
        private ClippedSelectableCombo<KeyValuePair<ulong, Character>>? _characters;
        private ulong? _selectedCharacter;

        public override IFilterEvent? DrawFooterFilter(FilterConfiguration configuration, FilterTable table)
        {
            ImGui.SameLine();
            var characterDictionary = PluginService.CharacterMonitor.Characters;
            var currentCharacterId = PluginService.CharacterMonitor.ActiveCharacterId;
            var allCharacters = characterDictionary.Where(c => c.Value.Name != "" && (c.Value.OwnerId == currentCharacterId || c.Key == currentCharacterId)).ToList();
            var currentCharacterName = _selectedCharacter == null
                ? ""
                : characterDictionary.ContainsKey(_selectedCharacter.Value)
                    ? characterDictionary[_selectedCharacter.Value].Name
                    : "";
            _characters = new ClippedSelectableCombo<KeyValuePair<ulong, Character>>("BestInSlotCharacterSelect", "BiS Character", 200,allCharacters, character => character.Value.NameWithClassAbv);
            if (_characters.Draw(currentCharacterName, out var selected))
            {
                _selectedCharacter = allCharacters[selected].Key;
                return new RefreshFilterEvent();
            }

            return null;
        }

        public override int? CurrentValue(InventoryItem item)
        {
            if (item.SortedCategory == InventoryCategory.CharacterEquipped || item.SortedCategory == InventoryCategory.RetainerEquipped || item.SortedCategory == InventoryCategory.Armoire || item.SortedCategory == InventoryCategory.GlamourChest || item.InGearSet)
            {
                return null;
            }
            return CurrentValue(item.Item);
        }

        public override int? CurrentValue(ItemEx item)
        {
            if (item.EquipSlotCategory.Row != 0 && CanCurrentJobEquip(item.ClassJobCategory.Row) && CanUse(item.LevelEquip))
            {
                var equippedItem = GetEquippedItem(item);
                if (equippedItem != null)
                {
                    if (item.EquipSlotCategoryEx?.SimilarSlots(equippedItem.Item) ?? false)
                    {
                        return (int)item.LevelItem.Row - (int)equippedItem.Item.LevelItem.Row;
                    }
                }

                return (int)item.LevelItem.Row;
            }

            return null;
            
        }

        public override int? CurrentValue(SortingResult item)
        {
            return CurrentValue(item.InventoryItem);
        }

        public override string Name { get; set; } = "Relative Item Level";
        public override float Width { get; set; } = 80;

        public override string HelpText { get; set; } =
            "Shows the relative item level of either your items or your retainers items compared to the item shown. This will show a drop down in the filter that lets you pick which character you are comparing against. A negative value indicates it's worse, a positive indicates it's better.";

        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;

        public bool CanUse(uint itemLevel)
        {
            if (_selectedCharacter != null)
            {
                if (PluginService.CharacterMonitor.Characters.ContainsKey(_selectedCharacter.Value))
                {
                    var character = PluginService.CharacterMonitor.Characters[_selectedCharacter.Value];
                    if (character.OwnerId != 0)
                    {
                        return character.Level >= itemLevel;
                    }
                }
            }
            if (PluginService.CharacterMonitor.ActiveCharacterId != 0)
            {
                var activeCharacter = PluginService.CharacterMonitor.ActiveCharacter;
                if (activeCharacter != null)
                {
                    return activeCharacter.Level >= itemLevel;
                }
            }

            return false;
        }

        public bool CanCurrentJobEquip(uint classJobCategory)
        {
            if (_selectedCharacter != null)
            {
                if (PluginService.CharacterMonitor.Characters.ContainsKey(_selectedCharacter.Value))
                {
                    var character = PluginService.CharacterMonitor.Characters[_selectedCharacter.Value];
                    if (character.OwnerId != 0)
                    {
                        if(Service.ExcelCache.IsItemEquippableBy(classJobCategory, character.ClassJob))
                        {
                            return true;
                        }

                        return false;
                    }
                }
            }
            if (PluginService.CharacterMonitor.ActiveCharacterId != 0)
            {
                var activeCharacter = PluginService.CharacterMonitor.ActiveCharacter;
                if (activeCharacter != null)
                {
                    if(Service.ExcelCache.IsItemEquippableBy(classJobCategory, activeCharacter.ClassJob))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public InventoryItem? GetEquippedItem(ItemEx comparingItem)
        {
            if (_selectedCharacter != null)
            {
                if (PluginService.CharacterMonitor.Characters.ContainsKey(_selectedCharacter.Value))
                {
                    var character = PluginService.CharacterMonitor.Characters[_selectedCharacter.Value];
                    if (character.OwnerId != 0)
                    {
                        var equipped = PluginService.InventoryMonitor.GetSpecificInventory(character.CharacterId,InventoryCategory.RetainerEquipped);
                        return equipped.FirstOrDefault(c => c.Item.EquipSlotCategoryEx?.SimilarSlots(comparingItem) ?? false);
                    }
                }
            }
            if (PluginService.CharacterMonitor.ActiveCharacterId != 0)
            {
                var equipped = PluginService.InventoryMonitor.GetSpecificInventory(PluginService.CharacterMonitor.ActiveCharacterId,
                    InventoryCategory.CharacterEquipped);
                return equipped.FirstOrDefault(c => c.Item.EquipSlotCategoryEx?.SimilarSlots(comparingItem) ?? false);
            }

            return null;
        }
    }
}