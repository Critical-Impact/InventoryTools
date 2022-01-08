using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using CriticalCommonLib.Models;
using Dalamud.Interface.Colors;
using ImGuiNET;
using InventoryTools.Extensions;
using InventoryTools.MarketBoard;

namespace InventoryTools.Logic
{
    public class MarketBoardPriceColumn : IColumn
    {
        public string Name { get; set; } = "MB Calc Price";
        public float Width { get; set; } = 250.0f;
        public string FilterText { get; set; } = "";

        private static readonly string LOADING = "loading...";

        private string Value(InventoryItem item)
        {
            var marketBoardData = Universalis.GetMarketBoardData(item);
            if (marketBoardData != null)
            {
                return $"{marketBoardData.calculcatedPrice}";
            }

            return LOADING;
        }

        private double ValueDouble(InventoryItem item)
        {
            var value = Value(item);
            double num;
            if (double.TryParse(value, out num))
            {
                return num;
            }
            else
            {
                return -1;
            }
        }

        public IEnumerable<InventoryItem> Filter(IEnumerable<InventoryItem> items)
        {
            return FilterText == "" ? items : items.Where(c => Value(c).ToLower().PassesFilter(FilterText.ToLower()));
        }

        public IEnumerable<SortingResult> Filter(IEnumerable<SortingResult> items)
        {
            return FilterText == "" ? items : items.Where(c => Value(c.InventoryItem).ToLower().PassesFilter(FilterText.ToLower()));
        }

        public IEnumerable<InventoryItem> Sort(ImGuiSortDirection direction, IEnumerable<InventoryItem> items)
        {
            return direction == ImGuiSortDirection.Ascending ? items.OrderBy(c => ValueDouble(c)) : items.OrderByDescending(c => ValueDouble(c));
        }

        public IEnumerable<SortingResult> Sort(ImGuiSortDirection direction, IEnumerable<SortingResult> items)
        {
            return direction == ImGuiSortDirection.Ascending ? items.OrderBy(c => ValueDouble(c.InventoryItem)) : items.OrderByDescending(c => ValueDouble(c.InventoryItem));
        }

        public void Draw(InventoryItem item)
        {
            ImGui.TableNextColumn();
            ImGui.TextColored(item.ItemColour, item.FormattedName);
        }

        public void Draw(SortingResult item)
        {
            ImGui.TableNextColumn();

            var marketBoardData = Value(item.InventoryItem);
            if (marketBoardData != LOADING)
            {
                ImGui.TextColored(item.InventoryItem.ItemColour, $"{marketBoardData}");
            }
            else
            {
                ImGui.TextColored(ImGuiColors.DalamudYellow, LOADING);
            }
        }

        public void Setup(int columnIndex)
        {
            ImGui.TableSetupColumn(Name, ImGuiTableColumnFlags.WidthFixed, Width, (uint)columnIndex);
        }
    }
}