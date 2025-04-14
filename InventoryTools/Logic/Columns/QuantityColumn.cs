using System.Collections.Generic;
using CharacterTools.Logic.Editors;
using CriticalCommonLib.Services;
using CriticalCommonLib.Services.Mediator;
using FFXIVClientStructs.FFXIV.Client.Game;
using ImGuiNET;
using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Logic.Columns.ColumnSettings;
using InventoryTools.Services;
using InventoryTools.Ui.Widgets;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns
{
    public class QuantityColumn : IntegerColumn
    {
        private readonly IInventoryMonitor _inventoryMonitor;
        private readonly CharacterScopePickerColumnSetting _scopePickerColumnSetting;
        private readonly CharacterScopeCalculator _scopeCalculator;

        public QuantityColumn(ILogger<QuantityColumn> logger, ImGuiService imGuiService, IInventoryMonitor inventoryMonitor, CharacterScopePickerColumnSetting scopePickerColumnSetting, CharacterScopeCalculator scopeCalculator) : base(logger, imGuiService)
        {
            _inventoryMonitor = inventoryMonitor;
            _scopePickerColumnSetting = scopePickerColumnSetting;
            _scopeCalculator = scopeCalculator;
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

            var scopes = _scopePickerColumnSetting.CurrentValue(columnConfiguration);
            if (scopes != null)
            {
                return (int)_scopeCalculator.Count(scopes, searchResult.ItemId, InventoryItem.ItemFlags.None) + (int)_scopeCalculator.Count(scopes, searchResult.ItemId, InventoryItem.ItemFlags.HighQuality);
            }
            else
            {
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
        }

        public override List<MessageBase>? DrawEditor(ColumnConfiguration columnConfiguration,
            FilterConfiguration configuration)
        {
            if (configuration.FilterType == Logic.FilterType.GameItemFilter ||
                configuration.FilterType == Logic.FilterType.CraftFilter)
            {
                ImGui.NewLine();
                ImGui.Separator();
                ImGui.SetNextItemWidth(220);
                ImGui.LabelText("##" + configuration.Key + "Search", "Characters to search in:");
                ImGui.SetNextItemWidth(250);
                ImGui.SameLine();
                _scopePickerColumnSetting.Draw(columnConfiguration,
                    "This lets you set which characters you want to generate a total based off.");
                base.DrawEditor(columnConfiguration, configuration);
            }
            return null;
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