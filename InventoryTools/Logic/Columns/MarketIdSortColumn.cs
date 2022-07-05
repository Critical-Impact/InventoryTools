using System.Collections.Generic;
using System.Linq;
using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using ImGuiNET;
using InventoryTools.Logic.Columns.Abstract;

namespace InventoryTools.Logic.Columns
{
    public class MarketIdSortColumn : TextColumn
    {
        public override string? CurrentValue(InventoryItem item)
        {
            return "Sorting only";
        }

        public override string? CurrentValue(ItemEx item)
        {
            return "Sorting only";
        }

        public override string? CurrentValue(SortingResult item)
        {
            return "Sorting only";
        }

        public override IEnumerable<ItemEx> Sort(ImGuiSortDirection direction, IEnumerable<ItemEx> items)
        {
            return direction == ImGuiSortDirection.Ascending ? items.OrderBy(c => (c.ItemSortCategory.Value?.Param) ?? 0).ThenBy(c => c.EquipSlotCategory.Row != 0 ? (int)c.LevelItem.Row : c.Unknown19) :  items.OrderByDescending(c => (c.ItemSortCategory.Value?.Param) ?? 0).ThenByDescending(c => c.EquipSlotCategory.Row != 0 ? (int)c.LevelItem.Row : c.Unknown19);
        }

        public override IEnumerable<SortingResult> Sort(ImGuiSortDirection direction, IEnumerable<SortingResult> items)
        {
            return direction == ImGuiSortDirection.Ascending
                ? items
                    .OrderBy(c =>c.InventoryItem.Item.ItemSortCategory.Value?.Param)
                    .ThenBy(c =>
                        c.InventoryItem.Item.EquipSlotCategory.Row != 0 ? (int)c.InventoryItem.Item.LevelItem.Row :
                        c.InventoryItem.Item.Unknown19)
                : items
                    .OrderByDescending(c => c.InventoryItem.Item.ItemSortCategory.Value?.Param)
                    .ThenByDescending(c =>c.InventoryItem.Item.EquipSlotCategory.Row != 0 ? (int)c.InventoryItem.Item.LevelItem.Row :
                        c.InventoryItem.Item.Unknown19);
        }

        public override IEnumerable<InventoryItem> Sort(ImGuiSortDirection direction, IEnumerable<InventoryItem> items)
        {
            return direction == ImGuiSortDirection.Ascending
                ? items.OrderBy(c => c.Item.ItemSortCategory.Value?.Param).ThenBy(c =>
                    c.Item.EquipSlotCategory.Row != 0 ? (int)c.Item.LevelItem.Row : c.Item.Unknown19)
                : items.OrderByDescending(c =>c.Item.ItemSortCategory.Value?.Param)
                    .ThenByDescending(c =>
                        c.Item.EquipSlotCategory.Row != 0 ? (int)c.Item.LevelItem.Row : c.Item.Unknown19);
        }

        public override string Name { get; set; } = "Debug - Market ID";
        public override float Width { get; set; } = 100;
        public override string FilterText { get; set; } = "";
        public override bool HasFilter { get; set; } = true;
        public override bool IsDebug { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
    }
}