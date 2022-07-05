using System;
using System.Linq;
using CriticalCommonLib;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using CriticalCommonLib.Sheets;
using ImGuiNET;
using InventoryTools.Logic.Columns.Abstract;

namespace InventoryTools.Logic.Columns
{
    public class VendorsColumn : TextColumn
    {
        public override string? CurrentValue(InventoryItem item)
        {
            return CurrentValue(item.Item);
        }

        public override string? CurrentValue(ItemEx item)
        {
            return String.Join(", ", item.Vendors);
        }

        public override string? CurrentValue(SortingResult item)
        {
            return CurrentValue(item.InventoryItem);
        }

        public override void Draw(ItemEx item, int rowIndex)
        {
            ImGui.TableNextColumn();
            var vendors = Service.ExcelCache.GetVendors(item.RowId);
            foreach (var vendor in vendors)
            {
                var npcLevels = Service.ExcelCache.GetNpcLevels(vendor.Item1.RowId);
                var placeNames = npcLevels.Select(c => c.FormattedName);
                ImGui.Text(vendor.Item1.Singular + " - " + vendor.Item2.Name + " - " + String.Join(", ", placeNames));
            }
        }

        public override void Draw(InventoryItem item, int rowIndex)
        {
            Draw(item.Item, rowIndex);
        }


        public override void Draw(SortingResult item, int rowIndex)
        {
            Draw(item.InventoryItem, rowIndex);
        }

        public override string Name { get; set; } = "Vendors";
        public override float Width { get; set; } = 100;
        public override string FilterText { get; set; } = "";
        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
    }
}