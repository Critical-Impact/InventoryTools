using System.Numerics;
using CriticalCommonLib.Models;
using Dalamud.Interface.Colors;
using Lumina.Excel.GeneratedSheets;

namespace InventoryTools.Logic.Columns
{
    public class NameColumn : ColoredTextColumn
    {
        public override (string, Vector4)? CurrentValue(InventoryItem item)
        {
            return (item.FormattedName, item.ItemColour);
        }

        public override (string, Vector4)? CurrentValue(Item item)
        {
            return (item.Name, ImGuiColors.DalamudWhite);
        }

        public override (string, Vector4)? CurrentValue(SortingResult item)
        {
            return CurrentValue(item.InventoryItem);
        }

        public override string Name { get; set; } = "Name";
        public override float Width { get; set; } = 250.0f;
        public override string FilterText { get; set; } = "";
        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
        public override event IColumn.ButtonPressedDelegate? ButtonPressed;
    }
}