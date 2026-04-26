using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AllaganLib.GameSheets.Sheets;
using AllaganLib.Shared.Extensions;
using Lumina.Excel;
using Lumina.Excel.Sheets;

namespace InventoryTools.Compendium.Types.Extra;

public class ChocoboItemIterator : IEnumerable<ChocoboItem>
{
    private readonly ItemSheet _itemSheet;
    
    private readonly ExcelSheet<BuddyItem> _buddyItemSheet;
    private readonly ExcelSheet<BuddyEquip> _buddyEquipSheet;
    private Dictionary<uint, uint>? _buddyEquipItemMap;
    
    public ChocoboItemIterator(
        ItemSheet itemSheet,
        ExcelSheet<BuddyItem> buddyItemSheet,
        ExcelSheet<BuddyEquip> buddyEquipSheet)
    {
        _itemSheet = itemSheet;
        _buddyItemSheet = buddyItemSheet;
        _buddyEquipSheet = buddyEquipSheet;
    }
    
    private Dictionary<uint, uint> BuildBuddyEquipMap()
    {
        var buddyEquipItemMap = new Dictionary<uint, uint>();
        var equipPluralLookup = _buddyEquipSheet.Where(c => !c.Plural.IsEmpty)
            .ToDictionary(
                i => i.Plural.ToImGuiString(),
                i => i.RowId);
        
        foreach (var item in _itemSheet)
        {
            var plural = item.Base.Plural.ToImGuiString();
            
            if (equipPluralLookup.TryGetValue(plural, out var equipRowId))
            {
                buddyEquipItemMap[equipRowId] = item.RowId;
            }
        }
        
        return buddyEquipItemMap;
    }
    
    public IEnumerator<ChocoboItem> GetEnumerator()
    {
        foreach (var buddy in _buddyItemSheet)
        {
            var item = buddy.Item.RowId;
            
            if (item == 0 || buddy.Item.ValueNullable?.Icon == 0)
                continue;
            
            yield return new ChocoboItem
            {
                RowId = buddy.RowId,
                
                Item = _itemSheet.GetRow(item),
                
                BuddyItem = new RowRef<BuddyItem>(
                    _buddyItemSheet.Module,
                    buddy.RowId,
                    _buddyItemSheet.Language),
                
                BuddyEquip = null,
                
                SourceType = ChocoboItemSourceType.BuddyItem
            };
        }
        
        _buddyEquipItemMap ??= BuildBuddyEquipMap();
        
        foreach (var equip in _buddyEquipSheet)
        {
            if (!_buddyEquipItemMap.TryGetValue(equip.RowId, out var itemRowId))
                continue;
            
            if (itemRowId == 0)
            {
                continue;
            }
            
            yield return new ChocoboItem
            {
                RowId = 1_000_000 + equip.RowId,
                
                Item = _itemSheet.GetRow(itemRowId),
                
                BuddyItem = null,
                
                BuddyEquip = new RowRef<BuddyEquip>(
                    _buddyEquipSheet.Module,
                    equip.RowId,
                    _buddyEquipSheet.Language),
                
                SourceType = ChocoboItemSourceType.BuddyEquip
            };
        }
    }
    
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}