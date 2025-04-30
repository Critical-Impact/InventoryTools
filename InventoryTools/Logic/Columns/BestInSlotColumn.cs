using System.Collections.Generic;
using System.Linq;
using AllaganLib.GameSheets.Sheets;
using AllaganLib.GameSheets.Sheets.Rows;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using Dalamud.Interface;
using ImGuiNET;
using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Logic.Columns.ColumnSettings;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;
using OtterGui.Widgets;

namespace InventoryTools.Logic.Columns
{
    public sealed class BestInSlotColumn : IntegerColumn
    {
        private readonly CharacterColumnSetting _characterColumnSetting;
        private readonly ICharacterMonitor _characterMonitor;
        private readonly IInventoryMonitor _inventoryMonitor;
        private readonly ClassJobCategorySheet _classJobCategorySheet;

        public BestInSlotColumn(ILogger<BestInSlotColumn> logger, CharacterColumnSetting.Factory characterColumnSettingFactory, ImGuiService imGuiService, ICharacterMonitor characterMonitor, IInventoryMonitor inventoryMonitor, ClassJobCategorySheet classJobCategorySheet) : base(logger, imGuiService)
        {
            _characterColumnSetting = characterColumnSettingFactory.Invoke("Comparison Character", "Shows the relative item level of either your items or your retainers items compared to the item shown. This will show a drop down in the filter that lets you pick which character you are comparing against. A negative value indicates it's worse, a positive indicates it's better.", [CharacterType.Character, CharacterType.Retainer], true);
            _characterMonitor = characterMonitor;
            _inventoryMonitor = inventoryMonitor;
            _classJobCategorySheet = classJobCategorySheet;
            this.FilterSettings = [_characterColumnSetting];
            this.FilterIcon = FontAwesomeIcon.Cog.ToIconString();
        }
        public override ColumnCategory ColumnCategory => ColumnCategory.Tools;

        public override IEnumerable<SearchResult> Filter(ColumnConfiguration columnConfiguration, IEnumerable<SearchResult> searchResults)
        {
            var characterSetting = this._characterColumnSetting.CurrentValue(columnConfiguration.FilterConfiguration);

            var filter = base.Filter(columnConfiguration, searchResults);
            if (characterSetting != null)
            {
                filter = filter.Where(c => CurrentValue(columnConfiguration, c) > 0);
            }

            return filter;
        }

        public override int? CurrentValue(ColumnConfiguration columnConfiguration, SearchResult searchResult)
        {
            var inventoryItem = searchResult.InventoryItem;
            if (inventoryItem != null)
            {
                if (inventoryItem.SortedCategory == InventoryCategory.CharacterEquipped ||
                    inventoryItem.SortedCategory == InventoryCategory.RetainerEquipped ||
                    inventoryItem.SortedCategory == InventoryCategory.Armoire ||
                    inventoryItem.SortedCategory == InventoryCategory.GlamourChest || inventoryItem.InGearSet)
                {
                    return null;
                }
            }

            var character = _characterColumnSetting.CurrentValue(columnConfiguration.FilterConfiguration);
            var item = searchResult.Item;
            if (item.EquipSlotCategory != null && CanCurrentJobEquip(character, item.Base.ClassJobCategory.RowId) && CanUse(character, item.Base.LevelEquip))
            {
                var equippedItem = GetEquippedItem(character, item);
                if (equippedItem != null)
                {
                    if (item.EquipSlotCategory?.SimilarSlots(equippedItem.Item) ?? false)
                    {
                        return (int)item.Base.LevelItem.RowId - (int)equippedItem.Item.Base.LevelItem.RowId;
                    }
                }

                return (int)item.Base.LevelItem.RowId;
            }
            return null;
        }

        public override string Name { get; set; } = "Relative Item Level";
        public override float Width { get; set; } = 150;

        public override string HelpText { get; set; } =
            "Shows the relative item level of either your items or your retainers items compared to the item shown. This will show a drop down in the filter that lets you pick which character you are comparing against. A negative value indicates it's worse, a positive indicates it's better.";

        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;

        public bool CanUse(ulong? characterId, uint itemLevel)
        {
            if (characterId != null)
            {
                if (_characterMonitor.Characters.TryGetValue(characterId.Value, out var character))
                {
                    if (character.OwnerId != 0)
                    {
                        return character.Level >= itemLevel;
                    }
                }
            }
            if (_characterMonitor.ActiveCharacterId != 0)
            {
                var activeCharacter = _characterMonitor.ActiveCharacter;
                if (activeCharacter != null)
                {
                    return activeCharacter.Level >= itemLevel;
                }
            }

            return false;
        }

        public bool CanCurrentJobEquip(ulong? characterId, uint classJobCategory)
        {
            if (characterId != null)
            {
                if (_characterMonitor.Characters.TryGetValue(characterId.Value, out var character))
                {
                    if (character.OwnerId != 0)
                    {
                        if(_classJobCategorySheet.IsItemEquippableBy(classJobCategory, character.ClassJob))
                        {
                            return true;
                        }

                        return false;
                    }
                }
            }
            if (_characterMonitor.ActiveCharacterId != 0)
            {
                var activeCharacter = _characterMonitor.ActiveCharacter;
                if (activeCharacter != null)
                {
                    if(_classJobCategorySheet.IsItemEquippableBy(classJobCategory, activeCharacter.ClassJob))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public InventoryItem? GetEquippedItem(ulong? characterId, ItemRow comparingItem)
        {
            if (characterId != null)
            {
                if (_characterMonitor.Characters.TryGetValue(characterId.Value, out var character))
                {
                    if (character.OwnerId != 0)
                    {
                        var equipped = _inventoryMonitor.GetSpecificInventory(character.CharacterId,InventoryCategory.RetainerEquipped);
                        return equipped.FirstOrDefault(c => c.Item.EquipSlotCategory?.SimilarSlots(comparingItem) ?? false);
                    }
                }
            }
            if (_characterMonitor.ActiveCharacterId != 0)
            {
                var equipped = _inventoryMonitor.GetSpecificInventory(_characterMonitor.ActiveCharacterId,
                    InventoryCategory.CharacterEquipped);
                return equipped.FirstOrDefault(c => c.Item.EquipSlotCategory?.SimilarSlots(comparingItem) ?? false);
            }

            return null;
        }
    }
}