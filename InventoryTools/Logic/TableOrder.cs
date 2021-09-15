using ImGuiNET;

namespace InventoryTools.Logic
{
    public struct TableOrder
    {
        public ImGuiSortDirection SortDirection;
        public short SortIndex;

        public TableOrder(ImGuiSortDirection sortDirection, short sortIndex)
        {
            SortDirection = sortDirection;
            SortIndex = sortIndex;
        }
    }
}