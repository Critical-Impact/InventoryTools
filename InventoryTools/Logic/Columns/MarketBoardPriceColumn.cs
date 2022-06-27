using CriticalCommonLib.Crafting;
using CriticalCommonLib.MarketBoard;
using CriticalCommonLib.Models;
using Dalamud.Interface.Colors;
using Dalamud.Logging;
using ImGuiNET;
using InventoryTools.Extensions;
using InventoryTools.Logic.Columns.Abstract;
using Lumina.Excel.GeneratedSheets;

namespace InventoryTools.Logic.Columns
{
    public class MarketBoardPriceColumn : DoubleGilColumn
    {
        protected static readonly string LoadingString = "loading...";
        protected static readonly string UntradableString = "untradable";
        protected static readonly int Loading = -1;
        protected static readonly int Untradable = -2;

        public override void Draw(InventoryItem item, int rowIndex)
        {
            var result = DoDraw(CurrentValue(item), rowIndex);
            result?.HandleEvent(item);
        }
        public override void Draw(SortingResult item, int rowIndex)
        {
            var result = DoDraw(CurrentValue(item), rowIndex);
            result?.HandleEvent(item);
        }
        public override void Draw(Item item, int rowIndex)
        {
            var result = DoDraw(CurrentValue(item), rowIndex);
            result?.HandleEvent(item);
        }
        public override void Draw(CraftItem item, int rowIndex, FilterConfiguration configuration)
        {
            var result = DoDraw(CurrentValue(item), rowIndex);
            result?.HandleEvent(item);
        }

        public override IColumnEvent? DoDraw((int, int)? currentValue, int rowIndex)
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
                base.DoDraw(currentValue, rowIndex);
                ImGui.SameLine();
                if (ImGui.SmallButton("R##" + rowIndex))
                {
                    return new RefreshPricingEvent();
                }
            }
            else
            {
                base.DoDraw(currentValue, rowIndex);
            }
            return null;
        }

        public override (int,int)? CurrentValue(InventoryItem item)
        {
            if (!item.CanBeTraded)
            {
                return (Untradable, Untradable);
            }

            var marketBoardData = Cache.GetPricing(item.ItemId, false);
            if (marketBoardData != null)
            {
                var nq = marketBoardData.averagePriceNQ;
                var hq = marketBoardData.averagePriceHQ;
                return ((int)nq, (int)hq);
            }

            return (Loading, Loading);
        }

        public override (int,int)? CurrentValue(Item item)
        {
            if (!item.CanBeTraded())
            {
                return (Untradable, Untradable);
            }

            var marketBoardData = Cache.GetPricing(item.RowId, false);
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
        public override float Width { get; set; } = 200.0f;
        public override string FilterText { get; set; } = "";
        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
    }
}