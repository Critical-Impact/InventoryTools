using System.Linq;
using CriticalCommonLib;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Logic.Filters;
using InventoryTools.Logic.Filters.Abstract;
using Lumina.Excel.GeneratedSheets;

namespace InventoryTools.Logic.Columns
{
    public class BestInSlotColumn : IntegerColumn
    {
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

        private ulong? _characterId = 33776997237932704;

        public override string Name { get; set; } = "Relative Item Level";
        public override float Width { get; set; } = 80;

        public override string FilterText { get; set; } ="";

        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;

        public bool CanUse(uint itemLevel)
        {
            if (_characterId != null)
            {
                if (PluginService.CharacterMonitor.Characters.ContainsKey(_characterId.Value))
                {
                    var character = PluginService.CharacterMonitor.Characters[_characterId.Value];
                    if (character.OwnerId != 0)
                    {
                        return character.Level >= itemLevel;
                    }
                }
            }
            if (PluginService.CharacterMonitor.ActiveCharacter != 0)
            {
                if (Service.ClientState.LocalPlayer != null)
                {
                    return Service.ClientState.LocalPlayer.Level >= itemLevel;
                }
            }

            return false;
        }

        public bool CanCurrentJobEquip(uint classJobCategory)
        {
            if (_characterId != null)
            {
                if (PluginService.CharacterMonitor.Characters.ContainsKey(_characterId.Value))
                {
                    var character = PluginService.CharacterMonitor.Characters[_characterId.Value];
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
            if (PluginService.CharacterMonitor.ActiveCharacter != 0)
            {
                if (Service.ClientState.LocalPlayer != null)
                {
                    if(Service.ExcelCache.IsItemEquippableBy(classJobCategory, Service.ClientState.LocalPlayer.ClassJob.Id))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public InventoryItem? GetEquippedItem(ItemEx comparingItem)
        {
            if (_characterId != null)
            {
                if (PluginService.CharacterMonitor.Characters.ContainsKey(_characterId.Value))
                {
                    var character = PluginService.CharacterMonitor.Characters[_characterId.Value];
                    if (character.OwnerId != 0)
                    {
                        var equipped = PluginService.InventoryMonitor.GetSpecificInventory(character.CharacterId,InventoryCategory.RetainerEquipped);
                        return equipped.FirstOrDefault(c => c.Item.EquipSlotCategoryEx?.SimilarSlots(comparingItem) ?? false);
                    }
                }
            }
            if (PluginService.CharacterMonitor.ActiveCharacter != 0)
            {
                var equipped = PluginService.InventoryMonitor.GetSpecificInventory(PluginService.CharacterMonitor.ActiveCharacter,
                    InventoryCategory.CharacterEquipped);
                return equipped.FirstOrDefault(c => c.Item.EquipSlotCategoryEx?.SimilarSlots(comparingItem) ?? false);
            }

            return null;
        }
    }
}