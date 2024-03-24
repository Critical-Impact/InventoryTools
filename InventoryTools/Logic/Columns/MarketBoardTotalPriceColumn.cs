using System.Collections.Generic;
using CriticalCommonLib.Interfaces;
using CriticalCommonLib.MarketBoard;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using CriticalCommonLib.Services.Mediator;
using Dalamud.Interface.Colors;
using Dalamud.Plugin.Services;
using ImGuiNET;
using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Logic.Columns.Settings;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns
{
    public class MarketBoardTotalPriceColumn : MarketBoardPriceColumn
    {
        public MarketBoardTotalPriceColumn(ILogger<MarketBoardTotalPriceColumn> logger, ImGuiService imGuiService, MarketboardWorldSetting marketboardWorldSetting, ICharacterMonitor characterMonitor, IMarketCache marketCache) : base(logger, imGuiService, marketboardWorldSetting, characterMonitor, marketCache)
        {
        }
        public override ColumnCategory ColumnCategory => ColumnCategory.Market;
        public override FilterType AvailableIn => Logic.FilterType.SearchFilter | Logic.FilterType.SortingFilter;

        public override List<MessageBase>? DoDraw(IItem item, (int, int)? currentValue, int rowIndex,
            FilterConfiguration filterConfiguration, ColumnConfiguration columnConfiguration)
        {
            if (currentValue.HasValue && currentValue.Value.Item1 == Loading)
            {
                ImGui.TableNextColumn();
                ImGui.TextColored(ImGuiColors.DalamudYellow, LoadingString);
            }
            else if (currentValue.HasValue && currentValue.Value.Item1 == Untradable)
            {
                ImGui.TableNextColumn();
                ImGui.TextColored(ImGuiColors.DalamudRed, UntradableString);
            }
            else if(currentValue.HasValue)
            {
                base.DoDraw(item, currentValue, rowIndex, filterConfiguration, columnConfiguration);

            }
            else
            {
                base.DoDraw(item, currentValue, rowIndex, filterConfiguration, columnConfiguration);
            }

            return null;
        }

        public override (int, int)? CurrentValue(ColumnConfiguration columnConfiguration, InventoryItem item)
        {
            if (!item.CanBeTraded)
            {
                return (Untradable, Untradable);
            }
            var value = base.CurrentValue(columnConfiguration, item);
            return value.HasValue ? ((int)(value.Value.Item1 * item.Quantity), (int)(value.Value.Item2 * item.Quantity)) : null;
        }

        public override (int, int)? CurrentValue(ColumnConfiguration columnConfiguration, SortingResult item)
        {
            return CurrentValue(columnConfiguration, item.InventoryItem);
        }

        public override string Name { get; set; } = "Market Board Average Total Price(Qty * Price) NQ/HQ";
        public override string RenderName => "MB Avg. Total NQ/HQ";
        public override float Width { get; set; } = 250.0f;
        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
    }
}