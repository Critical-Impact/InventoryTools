using System;
using System.Collections.Generic;
using System.Linq;
using CriticalCommonLib.Models;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;
using NaturalSort.Extension;

namespace InventoryTools.Logic.Columns
{
    public class MarketIdSortColumn : TextColumn
    {
        public override string? CurrentValue(InventoryItem item)
        {
            return "Sorting only";
        }

        public override string? CurrentValue(Item item)
        {
            return "Sorting only";
        }

        public override string? CurrentValue(SortingResult item)
        {
            return "Sorting only";
        }

        public override IEnumerable<Item> Sort(ImGuiSortDirection direction, IEnumerable<Item> items)
        {
            return direction == ImGuiSortDirection.Ascending ? items.OrderBy(c => (c.ItemSortCategory.Value?.Param) ?? 0).ThenBy(c => c.EquipSlotCategory.Row != 0 ? (int)c.LevelItem.Row : c.Unknown19) :  items.OrderByDescending(c => (c.ItemSortCategory.Value?.Param) ?? 0).ThenByDescending(c => c.EquipSlotCategory.Row != 0 ? (int)c.LevelItem.Row : c.Unknown19);
        }

        public override IEnumerable<SortingResult> Sort(ImGuiSortDirection direction, IEnumerable<SortingResult> items)
        {
            return direction == ImGuiSortDirection.Ascending ? items.OrderBy(c => c.InventoryItem.Item == null ? 0 : (c.InventoryItem.Item.ItemSortCategory.Value?.Param) ?? 0).ThenBy(c =>c.InventoryItem.Item == null ? 0 : c.InventoryItem.Item.EquipSlotCategory.Row != 0 ? (int)c.InventoryItem.Item.LevelItem.Row : c.InventoryItem.Item.Unknown19) :  items.OrderByDescending(c =>c.InventoryItem.Item == null ? 0 : (c.InventoryItem.Item.ItemSortCategory.Value?.Param) ?? 0).ThenByDescending(c =>c.InventoryItem.Item == null ? 0 : c.InventoryItem.Item.EquipSlotCategory.Row != 0 ? (int)c.InventoryItem.Item.LevelItem.Row : c.InventoryItem.Item.Unknown19);
        }

        public override IEnumerable<InventoryItem> Sort(ImGuiSortDirection direction, IEnumerable<InventoryItem> items)
        {
            return direction == ImGuiSortDirection.Ascending ? items.OrderBy(c => c.Item == null ? 0 : (c.Item.ItemSortCategory.Value?.Param) ?? 0).ThenBy(c =>c.Item == null ? 0 : c.Item.EquipSlotCategory.Row != 0 ? (int)c.Item.LevelItem.Row : c.Item.Unknown19) :  items.OrderByDescending(c =>c.Item == null ? 0 : (c.Item.ItemSortCategory.Value?.Param) ?? 0).ThenByDescending(c =>c.Item == null ? 0 : c.Item.EquipSlotCategory.Row != 0 ? (int)c.Item.LevelItem.Row : c.Item.Unknown19);
        }

        public override string Name { get; set; } = "Debug - Market ID";
        public override float Width { get; set; } = 100;
        public override string FilterText { get; set; } = "";
        public override bool HasFilter { get; set; } = true;
        public override bool IsDebug { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
    }
}