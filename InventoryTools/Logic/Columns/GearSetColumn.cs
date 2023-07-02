using System;
using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Columns.Abstract;

namespace InventoryTools.Logic.Columns
{
    public class GearSetColumn : TextColumn
    {
        public override ColumnCategory ColumnCategory => ColumnCategory.Basic;

        public override string? CurrentValue(InventoryItem item)
        {
            if (item.GearSetNames == null)
            {
                return "";
            }
            return String.Join(", ", item.GearSetNames);
        }

        public override string? CurrentValue(ItemEx item)
        {
            return "";
        }

        public override string? CurrentValue(SortingResult item)
        {
            return CurrentValue(item.InventoryItem);
        }

        public override string Name { get; set; } = "Gearset Number";
        public override string RenderName => "Gearsets";
        public override float Width { get; set; } = 100;
        public override string HelpText { get; set; } = "Provides the gearsets that an item is part of.";
        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
    }
}