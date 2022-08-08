using CriticalCommonLib.Crafting;
using CriticalCommonLib.Models;
using Dalamud.Interface.Colors;
using ImGuiNET;

namespace InventoryTools.Logic.Columns
{
    public class MarketBoardMinTotalPriceColumn : MarketBoardMinPriceColumn
    {
        public override string HelpText { get; set; } =
            "Shows the minimum price of both the NQ and HQ form of the item and multiplies it by the quantity available. This data is sourced from universalis.";
        public override FilterType AvailableIn => Logic.FilterType.SearchFilter | Logic.FilterType.SortingFilter;

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
            var value = base.CurrentValue(item);
            return value.HasValue ? ((int)(value.Value.Item1 * item.Quantity), (int)(value.Value.Item2 * item.Quantity)) : null;
        }

        public override (int,int)? CurrentValue(SortingResult item)
        {
            return CurrentValue(item.InventoryItem);
        }

        public override (int, int)? CurrentValue(CraftItem currentValue)
        {
            if (!currentValue.Item.CanBeTraded)
            {
                return (Untradable, Untradable);
            }
            var value = CurrentValue(currentValue.Item);
            return value.HasValue ? ((int)(value.Value.Item1 * currentValue.QuantityRequired), (int)(value.Value.Item2 * currentValue.QuantityRequired)) : null;
        }

        public override string Name { get; set; } = "MB Minimum Total Price";
    }
}