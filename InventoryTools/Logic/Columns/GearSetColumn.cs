using System;
using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns
{
    public class GearSetColumn : TextColumn
    {
        public GearSetColumn(ILogger<GearSetColumn> logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }
        public override ColumnCategory ColumnCategory => ColumnCategory.Basic;

        public override string? CurrentValue(ColumnConfiguration columnConfiguration, InventoryItem item)
        {
            if (item.GearSetNames == null)
            {
                return "";
            }
            return String.Join(", ", item.GearSetNames);
        }

        public override string? CurrentValue(ColumnConfiguration columnConfiguration, ItemEx item)
        {
            return "";
        }

        public override string? CurrentValue(ColumnConfiguration columnConfiguration, SortingResult item)
        {
            return CurrentValue(columnConfiguration, item.InventoryItem);
        }

        public override string Name { get; set; } = "Gearset Number";
        public override string RenderName => "Gearsets";
        public override float Width { get; set; } = 100;
        public override string HelpText { get; set; } = "Provides the gearsets that an item is part of.";
        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
    }
}