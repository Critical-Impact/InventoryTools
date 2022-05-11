using System;
using CriticalCommonLib.Models;
using InventoryTools.Logic.Columns.Abstract;
using Lumina.Excel.GeneratedSheets;

namespace InventoryTools.Logic.Columns
{
    public class IconColumn : GameIconColumn
    {
        public override ushort? CurrentValue(InventoryItem item)
        {
            return item.Icon;
        }

        public override ushort? CurrentValue(Item item)
        {
            return item.Icon;
        }

        public override ushort? CurrentValue(SortingResult item)
        {
            return item.InventoryItem.Icon;
        }

        public override string Name { get; set; } = "Icon";
        public override float Width { get; set; } = 40.0f;
        public override string FilterText { get; set; } = "";
        public override bool HasFilter { get; set; } = false;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
    }
}