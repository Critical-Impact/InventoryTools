using System;

namespace InventoryTools.Logic
{
    [Flags] 
    public enum FilterType
    {
        None = 0,
        SearchFilter = 1, //For displaying the items in a source
        SortingFilter = 2, //For working out where items should go
        GameItemFilter = 4, //For displaying all the items
        CraftFilter = 8, //For crafting items
        HistoryFilter = 16, //For showing historical movement of items
        CuratedList = 32 //Manually curated lists
    }
}