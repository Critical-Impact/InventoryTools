using System.Collections.Generic;
using AllaganLib.GameSheets.Caches;
using AllaganLib.GameSheets.Model;
using AllaganLib.GameSheets.Sheets.Rows;
using InventoryTools.Logic;

namespace InventoryTools.EquipmentSuggest;

public class EquipmentSuggestItem
{
    public EquipSlot? EquipmentSlot { get; set; }

    public ClassJobRow? ClassJobRow { get; set; }
    public SearchResult? SelectedItem { get; set; }
    public ItemInfoType? AcquisitionSource { get; set; }

    public SearchResult? SecondarySelectedItem { get; set; }
    public ItemInfoType? SecondaryAcquisitionSource { get; set; }

    public Dictionary<int, List<SearchResult>> SuggestedItems { get; }

    public EquipmentSuggestItem(EquipSlot equipmentSlot)
    {
        SuggestedItems = new Dictionary<int, List<SearchResult>>
        {
            { 0, [] },
            { 1, [] },
            { 2, [] },
            { 3, [] },
            { 4, [] }
        };
        EquipmentSlot = equipmentSlot;
    }

    public EquipmentSuggestItem(ClassJobRow classJobRow)
    {
        SuggestedItems = new Dictionary<int, List<SearchResult>>
        {
            { 0, [] },
            { 1, [] },
            { 2, [] },
            { 3, [] },
            { 4, [] }
        };
        ClassJobRow = classJobRow;
    }
}