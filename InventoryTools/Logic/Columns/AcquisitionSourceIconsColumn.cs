using System.Collections.Generic;
using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Columns.Abstract;

namespace InventoryTools.Logic.Columns
{
    public class AcquisitionSourceIconsColumn : GameIconsColumn
    {
        public override List<ushort>? CurrentValue(InventoryItem item)
        {
            return CurrentValue(item.Item);
        }

        public override List<ushort>? CurrentValue(ItemEx item)
        {
            return item.SourceIcons;
        }

        public override List<ushort>? CurrentValue(SortingResult item)
        {
            return CurrentValue(item.InventoryItem);
        }

        public override string Name { get; set; } = "Acqusition Icons(Rename me bro)";
        public override float Width { get; set; } = 100;
        public override string FilterText { get; set; } = "";
        public override bool HasFilter { get; set; } = false;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
    }
}