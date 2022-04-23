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
            return "fuck";
        }

        public override string? CurrentValue(Item item)
        {
            return "fuck";
        }

        public override string? CurrentValue(SortingResult item)
        {
            return "fuck";
        }

        public override IEnumerable<Item> Sort(ImGuiSortDirection direction, IEnumerable<Item> items)
        {
            /*var unknown19Items = items.Where(c => c.Unknown19 != 0).ToList();
            var emptyUnknown19Items = items.Where(c => c.Unknown19 == 0).ToList();
            var finalItems = new List<Item>();
            finalItems.AddRange(direction == ImGuiSortDirection.Ascending ? unknown19Items.OrderBy(c => c.Unknown19).ThenBy(c => c.RowId) : unknown19Items.OrderByDescending(c => c.Unknown19).ThenByDescending(c => c.RowId));
            finalItems.AddRange(direction == ImGuiSortDirection.Ascending ? emptyUnknown19Items.OrderBy<Item, string>(c => c.ItemSortCategory.Row.ToString(), StringComparison.OrdinalIgnoreCase.WithNaturalSort()) : emptyUnknown19Items.OrderByDescending<Item, string>(c => c.ItemSortCategory.Row.ToString(), StringComparison.OrdinalIgnoreCase.WithNaturalSort()));*/
            return direction == ImGuiSortDirection.Ascending ? items.OrderBy(c => (c.ItemSortCategory.Value?.Param) ?? 0).ThenBy(c => c.EquipSlotCategory.Row != 0 ? (int)c.LevelItem.Row : c.Unknown19) :  items.OrderByDescending(c => (c.ItemSortCategory.Value?.Param) ?? 0).ThenByDescending(c => c.EquipSlotCategory.Row != 0 ? (int)c.LevelItem.Row : c.Unknown19);
            //return finalItems;
        }

        public override IEnumerable<SortingResult> Sort(ImGuiSortDirection direction, IEnumerable<SortingResult> items)
        {
            return direction == ImGuiSortDirection.Ascending ? items.OrderBy(c => c.InventoryItem.Item == null ? 0 : (c.InventoryItem.Item.ItemSortCategory.Value?.Param) ?? 0).ThenBy(c =>c.InventoryItem.Item == null ? 0 : c.InventoryItem.Item.EquipSlotCategory.Row != 0 ? (int)c.InventoryItem.Item.LevelItem.Row : c.InventoryItem.Item.Unknown19) :  items.OrderByDescending(c =>c.InventoryItem.Item == null ? 0 : (c.InventoryItem.Item.ItemSortCategory.Value?.Param) ?? 0).ThenByDescending(c =>c.InventoryItem.Item == null ? 0 : c.InventoryItem.Item.EquipSlotCategory.Row != 0 ? (int)c.InventoryItem.Item.LevelItem.Row : c.InventoryItem.Item.Unknown19);
        }

        public override IEnumerable<InventoryItem> Sort(ImGuiSortDirection direction, IEnumerable<InventoryItem> items)
        {
            return direction == ImGuiSortDirection.Ascending ? items.OrderBy(c => c.Item == null ? 0 : (c.Item.ItemSortCategory.Value?.Param) ?? 0).ThenBy(c =>c.Item == null ? 0 : c.Item.EquipSlotCategory.Row != 0 ? (int)c.Item.LevelItem.Row : c.Item.Unknown19) :  items.OrderByDescending(c =>c.Item == null ? 0 : (c.Item.ItemSortCategory.Value?.Param) ?? 0).ThenByDescending(c =>c.Item == null ? 0 : c.Item.EquipSlotCategory.Row != 0 ? (int)c.Item.LevelItem.Row : c.Item.Unknown19);
        }

        public override string Name { get; set; } = "Market ID";
        public override float Width { get; set; } = 100;
        public override string FilterText { get; set; } = "";
        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
    }
}