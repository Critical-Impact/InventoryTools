using System.Collections.Generic;
using CriticalCommonLib.Interfaces;
using CriticalCommonLib.MarketBoard;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using CriticalCommonLib.Services.Mediator;
using Dalamud.Interface.Colors;
using ImGuiNET;
using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Logic.Columns.ColumnSettings;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns
{
    public class MarketBoardTotalPriceColumn : MarketBoardPriceColumn
    {
        public MarketBoardTotalPriceColumn(ILogger<MarketBoardTotalPriceColumn> logger, ImGuiService imGuiService, MarketboardWorldSetting marketboardWorldSetting, ICharacterMonitor characterMonitor, IMarketCache marketCache, ExcelCache excelCache) : base(logger, imGuiService, marketboardWorldSetting, characterMonitor, marketCache, excelCache)
        {
        }
        public override ColumnCategory ColumnCategory => ColumnCategory.Market;
        public override FilterType AvailableIn => Logic.FilterType.SearchFilter | Logic.FilterType.SortingFilter;

        public override List<MessageBase>? DoDraw(SearchResult searchResult, (int, int)? currentValue, int rowIndex,
            FilterConfiguration filterConfiguration, ColumnConfiguration columnConfiguration)
        {
            if (currentValue.HasValue && currentValue.Value.Item1 == Loading)
            {
                ImGui.TableNextColumn();
                if (ImGui.TableGetColumnFlags().HasFlag(ImGuiTableColumnFlags.IsEnabled))
                {
                    ImGui.TextColored(ImGuiColors.DalamudYellow, LoadingString);
                }
            }
            else if (currentValue.HasValue && currentValue.Value.Item1 == Untradable)
            {
                ImGui.TableNextColumn();
                if (ImGui.TableGetColumnFlags().HasFlag(ImGuiTableColumnFlags.IsEnabled))
                {
                    ImGui.TextColored(ImGuiColors.DalamudRed, UntradableString);
                }
            }
            else if(currentValue.HasValue)
            {
                base.DoDraw(searchResult, currentValue, rowIndex, filterConfiguration, columnConfiguration);

            }
            else
            {
                base.DoDraw(searchResult, currentValue, rowIndex, filterConfiguration, columnConfiguration);
            }

            return null;
        }

        public override (int, int)? CurrentValue(ColumnConfiguration columnConfiguration, SearchResult searchResult)
        {
            if (searchResult.InventoryItem is {CanBeTraded: false})
            {
                return (Untradable, Untradable);
            }
            var value = base.CurrentValue(columnConfiguration, searchResult);
            var quantity = searchResult.InventoryItem?.Quantity ?? 1;
            return value.HasValue ? ((int)(value.Value.Item1 * quantity), (int)(value.Value.Item2 * quantity)) : null;
        }
        public override string Name { get; set; } = "Market Board Average Total Price(Qty * Price) NQ/HQ";
        public override string RenderName => "MB Avg. Total NQ/HQ";
        public override string HelpText { get; set; } =
            "Shows the average price of both the NQ and HQ form of the item and multiplies it by the quantity available. If no world is selected, your home world is used. This data is sourced from universalis.";
        public override float Width { get; set; } = 250.0f;
        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
    }
}