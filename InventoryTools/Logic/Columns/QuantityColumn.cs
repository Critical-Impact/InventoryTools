using System.Collections.Generic;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using CriticalCommonLib.Services.Mediator;
using CriticalCommonLib.Sheets;
using ImGuiNET;
using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Services;
using InventoryTools.Ui.Widgets;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns
{
    public class QuantityColumn : IntegerColumn
    {
        private readonly IInventoryMonitor _inventoryMonitor;

        public QuantityColumn(ILogger<QuantityColumn> logger, ImGuiService imGuiService, IInventoryMonitor inventoryMonitor) : base(logger, imGuiService)
        {
            _inventoryMonitor = inventoryMonitor;
        }
        public override ColumnCategory ColumnCategory => ColumnCategory.Inventory;
        public override int? CurrentValue(ColumnConfiguration columnConfiguration, SearchResult searchResult)
        {
            if (searchResult.CuratedItem != null)
            {
                return (int)searchResult.CuratedItem.Quantity;
            }
            if (searchResult.InventoryItem != null)
            {
                return (int)searchResult.InventoryItem.Quantity;
            }
            var qty = 0;
            if (_inventoryMonitor.ItemCounts.ContainsKey((searchResult.Item.RowId,
                    FFXIVClientStructs.FFXIV.Client.Game.InventoryItem.ItemFlags.None)))
            {
                qty += _inventoryMonitor.ItemCounts[(searchResult.Item.RowId,
                    FFXIVClientStructs.FFXIV.Client.Game.InventoryItem.ItemFlags.None)];
            }
            if (_inventoryMonitor.ItemCounts.ContainsKey((searchResult.Item.RowId,
                    FFXIVClientStructs.FFXIV.Client.Game.InventoryItem.ItemFlags.HighQuality)))
            {
                qty += _inventoryMonitor.ItemCounts[(searchResult.Item.RowId,
                    FFXIVClientStructs.FFXIV.Client.Game.InventoryItem.ItemFlags.HighQuality)];
            }
            return qty;
        }

        public override List<MessageBase>? Draw(FilterConfiguration configuration, ColumnConfiguration columnConfiguration, SearchResult searchResult,
            int rowIndex, int columnIndex)
        {
            if (searchResult.CuratedItem != null)
            {
                ImGui.TableNextColumn();
                if (!ImGui.TableGetColumnFlags().HasFlag(ImGuiTableColumnFlags.IsEnabled)) return null;
                var value = CurrentValue(columnConfiguration, searchResult)?.ToString() ?? "";
                ImGuiUtil.VerticalAlignButton(configuration.TableHeight);
                if (ImGui.InputText("##"+rowIndex+"QtyInput" + columnIndex, ref value, 4, ImGuiInputTextFlags.CharsDecimal))
                {
                    if (value != (CurrentValue(columnConfiguration, searchResult)?.ToString() ?? ""))
                    {
                        int parsedNumber;
                        if (int.TryParse(value, out parsedNumber))
                        {
                            if (parsedNumber < 0)
                            {
                                parsedNumber = 0;
                            }
                            var number = (uint)parsedNumber;
                            if (number != searchResult.CuratedItem.Quantity)
                            {
                                searchResult.CuratedItem.Quantity = number;
                                configuration.ConfigurationDirty = true;
                            }
                        }
                    }
                }

                return null;
            }
            return base.Draw(configuration, columnConfiguration, searchResult, rowIndex, columnIndex);
        }

        public override string Name { get; set; } = "Total Quantity Available";
        public override string RenderName => "Quantity";

        public override float Width { get; set; } = 70.0f;

        public override string HelpText { get; set; } =
            "The quantity of the item. If viewing from a game items or craft filter, this will show the total number of items available in all inventories.";
        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;

        public override FilterType AvailableIn => Logic.FilterType.SearchFilter | Logic.FilterType.SortingFilter | Logic.FilterType.GameItemFilter | Logic.FilterType.HistoryFilter | Logic.FilterType.CuratedList;
        public override FilterType DefaultIn => Logic.FilterType.SearchFilter | Logic.FilterType.SortingFilter | Logic.FilterType.CraftFilter | Logic.FilterType.HistoryFilter | Logic.FilterType.CuratedList;
    }
}