using CriticalCommonLib.Crafting;
using CriticalCommonLib.MarketBoard;
using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using Dalamud.Interface.Colors;
using ImGuiNET;
using InventoryTools.Logic.Columns.Abstract;

namespace InventoryTools.Logic.Columns
{
    public class MarketBoardPriceColumn : DoubleGilColumn
    {
        protected static readonly string LoadingString = "loading...";
        protected static readonly string UntradableString = "untradable";
        protected static readonly int Loading = -1;
        protected static readonly int Untradable = -2;

        public override void Draw(FilterConfiguration configuration, InventoryItem item, int rowIndex)
        {
            var result = DoDraw(CurrentValue(item), rowIndex, configuration);
            result?.HandleEvent(configuration,item);
        }
        public override void Draw(FilterConfiguration configuration, SortingResult item, int rowIndex)
        {
            var result = DoDraw(CurrentValue(item), rowIndex, configuration);
            result?.HandleEvent(configuration,item);
        }
        public override void Draw(FilterConfiguration configuration, ItemEx item, int rowIndex)
        {
            var result = DoDraw(CurrentValue((ItemEx)item), rowIndex, configuration);
            result?.HandleEvent(configuration,item);
        }
        public override void Draw(FilterConfiguration configuration, CraftItem item, int rowIndex)
        {
            var result = DoDraw(CurrentValue(item), rowIndex, configuration);
            result?.HandleEvent(configuration,item);
        }

        public override IColumnEvent? DoDraw((int, int)? currentValue, int rowIndex,
            FilterConfiguration filterConfiguration)
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
                base.DoDraw(currentValue, rowIndex, filterConfiguration);
                ImGui.SameLine();
                if (ImGui.SmallButton("R##" + rowIndex))
                {
                    return new RefreshPricingEvent();
                }
            }
            else
            {
                base.DoDraw(currentValue, rowIndex, filterConfiguration);
            }
            return null;
        }

        public override (int,int)? CurrentValue(InventoryItem item)
        {
            if (!item.CanBeTraded)
            {
                return (Untradable, Untradable);
            }

            var marketBoardData = PluginService.MarketCache.GetPricing(item.ItemId, false);
            if (marketBoardData != null)
            {
                var nq = marketBoardData.averagePriceNQ;
                var hq = marketBoardData.averagePriceHQ;
                return ((int)nq, (int)hq);
            }

            return (Loading, Loading);
        }

        public override (int, int)? CurrentValue(ItemEx item)
        {
            if (!item.CanBeTraded)
            {
                return (Untradable, Untradable);
            }

            var marketBoardData = PluginService.MarketCache.GetPricing(item.RowId, false);
            if (marketBoardData != null)
            {
                var nq = marketBoardData.averagePriceNQ;
                var hq = marketBoardData.averagePriceHQ;
                return ((int)nq, (int)hq);
            }

            return (Loading, Loading);
        }

        public override (int,int)? CurrentValue(SortingResult item)
        {
            return CurrentValue(item.InventoryItem);
        }

        public override string Name { get; set; } = "MB Average Price NQ/HQ";
        public override string HelpText { get; set; } =
            "Shows the average price of both the NQ and HQ form of the item. This data is sourced from universalis.";
        public override float Width { get; set; } = 200.0f;
        public override string FilterText { get; set; } = "";
        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
    }
}