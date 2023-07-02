using System.Numerics;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using Dalamud.Interface.Colors;
using InventoryTools.Logic.Columns.Abstract;

namespace InventoryTools.Logic.Columns
{
    public class NameColumn : ColoredTextColumn
    {
        public override ColumnCategory ColumnCategory => ColumnCategory.Basic;
        public override (string, Vector4)? CurrentValue(InventoryItem item)
        {
            return (item.FormattedName, item.ItemColour);
        }

        public override (string, Vector4)? CurrentValue(ItemEx item)
        {
            return (item.NameString, ImGuiColors.DalamudWhite);
        }

        public override (string, Vector4)? CurrentValue(SortingResult item)
        {
            return CurrentValue(item.InventoryItem);
        }
        

        public override (string, Vector4)? CurrentValue(CraftItem currentValue)
        {
            return (currentValue.FormattedName, ImGuiColors.DalamudWhite);
        }

        public override string Name { get; set; } = "Name";
        public override float Width { get; set; } = 250.0f;
        public override string HelpText { get; set; } = "The name of the item.";
        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
    }
}