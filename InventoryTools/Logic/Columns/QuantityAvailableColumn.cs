using System.Linq;
using CriticalCommonLib.Services;
using FFXIVClientStructs.FFXIV.Client.Game;
using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Logic.Columns.ColumnSettings;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns
{
    public class QuantityAvailableColumn : IntegerColumn
    {
        private readonly IInventoryMonitor _inventoryMonitor;
        private readonly QualitySelectorSetting _qualitySelectorSetting;

        public QuantityAvailableColumn(ILogger<QuantityAvailableColumn> logger, ImGuiService imGuiService, IInventoryMonitor inventoryMonitor, QualitySelectorSetting qualitySelectorSetting) : base(logger, imGuiService)
        {
            _inventoryMonitor = inventoryMonitor;
            _qualitySelectorSetting = qualitySelectorSetting;
            this.Settings.Add(qualitySelectorSetting);
            this.FilterSettings.Add(qualitySelectorSetting);
        }
        public override ColumnCategory ColumnCategory => ColumnCategory.Inventory;

        public override int? CurrentValue(ColumnConfiguration columnConfiguration, SearchResult searchResult)
        {
            var qualitySetting = _qualitySelectorSetting.CurrentValue(columnConfiguration)?.Select(c => c.Item1).ToList() ?? null;

            var qty = 0;
            if ((qualitySetting == null || qualitySetting.Contains(InventoryItem.ItemFlags.None)) && _inventoryMonitor.ItemCounts.ContainsKey((searchResult.Item.RowId,
                    FFXIVClientStructs.FFXIV.Client.Game.InventoryItem.ItemFlags.None)))
            {
                qty += _inventoryMonitor.ItemCounts[(searchResult.Item.RowId,
                    FFXIVClientStructs.FFXIV.Client.Game.InventoryItem.ItemFlags.None)];
            }
            if ((qualitySetting == null || qualitySetting.Contains(InventoryItem.ItemFlags.HighQuality)) && _inventoryMonitor.ItemCounts.ContainsKey((searchResult.Item.RowId,
                    FFXIVClientStructs.FFXIV.Client.Game.InventoryItem.ItemFlags.HighQuality)))
            {
                qty += _inventoryMonitor.ItemCounts[(searchResult.Item.RowId,
                    FFXIVClientStructs.FFXIV.Client.Game.InventoryItem.ItemFlags.HighQuality)];
            }
            if ((qualitySetting == null || qualitySetting.Contains(InventoryItem.ItemFlags.Collectable)) && _inventoryMonitor.ItemCounts.ContainsKey((searchResult.Item.RowId,
                    FFXIVClientStructs.FFXIV.Client.Game.InventoryItem.ItemFlags.Collectable)))
            {
                qty += _inventoryMonitor.ItemCounts[(searchResult.Item.RowId,
                    FFXIVClientStructs.FFXIV.Client.Game.InventoryItem.ItemFlags.Collectable)];
            }
            return qty;
        }
        public override string Name { get; set; } = "Total Quantity Available";
        public override string RenderName => "Available";
        public override float Width { get; set; } = 100;

        public override string HelpText { get; set; } =
            "The number of items available across all inventories of this item.";
        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;

        public override FilterType AvailableIn => Logic.FilterType.CraftFilter | Logic.FilterType.CuratedList;
    }
}