using System.Collections.Generic;
using CriticalCommonLib.Crafting;
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
using InventoryTools.Ui.Widgets;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns
{
    public class MarketBoardMinTotalPriceColumn : MarketBoardMinPriceColumn
    {
        public MarketBoardMinTotalPriceColumn(ILogger<MarketBoardMinTotalPriceColumn> logger, ImGuiService imGuiService, MarketboardWorldSetting marketboardWorldSetting, ICharacterMonitor characterMonitor, IMarketCache marketCache) : base(logger, imGuiService, marketboardWorldSetting, characterMonitor, marketCache)
        {
        }
        public override ColumnCategory ColumnCategory => ColumnCategory.Market;
        public override string HelpText { get; set; } =
            "Shows the minimum price of both the NQ and HQ form of the item and multiplies it by the quantity available. This data is sourced from universalis.";
        public override FilterType AvailableIn => Logic.FilterType.SearchFilter | Logic.FilterType.SortingFilter;

        public override List<MessageBase>? DoDraw(IItem item, (int, int)? currentValue, int rowIndex,
            FilterConfiguration filterConfiguration, ColumnConfiguration columnConfiguration)
        {
            
            if (currentValue.HasValue && currentValue.Value.Item1 == Loading)
            {
                ImGui.TableNextColumn();
                ImGuiUtil.VerticalAlignTextColored(LoadingString, ImGuiColors.DalamudYellow, filterConfiguration.TableHeight, false);
            }
            else if (currentValue.HasValue && currentValue.Value.Item1 == Untradable)
            {
                ImGui.TableNextColumn();
                ImGuiUtil.VerticalAlignTextColored(UntradableString, ImGuiColors.DalamudRed, filterConfiguration.TableHeight, false);
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

        public override (int, int)? CurrentValue(ColumnConfiguration columnConfiguration, CraftItem currentValue)
        {
            if (!currentValue.Item.CanBeTraded)
            {
                return (Untradable, Untradable);
            }
            var value = CurrentValue(columnConfiguration, currentValue.Item);
            return value.HasValue ? ((int)(value.Value.Item1 * currentValue.QuantityRequired), (int)(value.Value.Item2 * currentValue.QuantityRequired)) : null;
        }
        
        public override string Name { get; set; } = "Market Board Minimum Total Price(Qty * Price) NQ/HQ";
        public override string RenderName => "MB Min. Total NQ/HQ";
    }
}