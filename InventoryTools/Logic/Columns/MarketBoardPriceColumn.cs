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
        public string Name { get; set; } = "MB Current Average Price";
        public float Width { get; set; } = 250.0f;
        public string FilterText { get; set; } = "";

        private static readonly string LOADING = "loading...";

        private string Value(InventoryItem item)
        {
            var marketBoardData = Universalis.GetMarketBoardData(item, Service.ClientState.LocalPlayer.CurrentWorld.GameData.Name.RawString);
            if (marketBoardData != null)
            {
                return $"{marketBoardData.currentAveragePrice}";
            }

            return LOADING;
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
            return direction == ImGuiSortDirection.Ascending ? items.OrderBy(c => Value(c)) : items.OrderByDescending(c => Value(c));
        }

        public IEnumerable<SortingResult> Sort(ImGuiSortDirection direction, IEnumerable<SortingResult> items)
        {
            return direction == ImGuiSortDirection.Ascending ? items.OrderBy(c => Value(c.InventoryItem)) : items.OrderByDescending(c => Value(c.InventoryItem));
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