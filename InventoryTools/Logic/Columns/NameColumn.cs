using System.Numerics;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using Dalamud.Interface.Colors;
using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns
{
    public class NameColumn : ColoredTextColumn
    {
        public NameColumn(ILogger<NameColumn> logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }
        public override ColumnCategory ColumnCategory => ColumnCategory.Basic;
        public override (string, Vector4)? CurrentValue(ColumnConfiguration columnConfiguration, InventoryItem item)
        {
            return (item.FormattedName, item.ItemColour);
        }

        public override (string, Vector4)? CurrentValue(ColumnConfiguration columnConfiguration, ItemEx item)
        {
            return (item.NameString, ImGuiColors.DalamudWhite);
        }

        public override (string, Vector4)? CurrentValue(ColumnConfiguration columnConfiguration, SortingResult item)
        {
            return CurrentValue(columnConfiguration, item.InventoryItem);
        }
        

        public override (string, Vector4)? CurrentValue(ColumnConfiguration columnConfiguration, CraftItem currentValue)
        {
            return (currentValue.FormattedName, ImGuiColors.DalamudWhite);
        }

        public override string Name { get; set; } = "Name";
        public override float Width { get; set; } = 250.0f;
        public override string HelpText { get; set; } = "The name of the item.";
        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
        
        public override FilterType DefaultIn => Logic.FilterType.SearchFilter | Logic.FilterType.SortingFilter | Logic.FilterType.GameItemFilter | Logic.FilterType.CraftFilter | Logic.FilterType.HistoryFilter;
    }
}