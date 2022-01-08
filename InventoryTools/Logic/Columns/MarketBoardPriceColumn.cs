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
        
        public IEnumerable<InventoryItem> Filter(IEnumerable<InventoryItem> items)
        {
            return FilterText == "" ? items : items.Where(c => c.FormattedName.ToLower().PassesFilter(FilterText.ToLower()));
        }

        public IEnumerable<SortingResult> Filter(IEnumerable<SortingResult> items)
        {
            return FilterText == "" ? items : items.Where(c => c.InventoryItem.FormattedName.ToLower().PassesFilter(FilterText.ToLower()));
        }

        public IEnumerable<InventoryItem> Sort(ImGuiSortDirection direction, IEnumerable<InventoryItem> items)
        {
            return direction == ImGuiSortDirection.Ascending ? items.OrderBy(c => c.FormattedName.ToLower()) : items.OrderByDescending(c => c.FormattedName.ToLower());
        }

        public IEnumerable<SortingResult> Sort(ImGuiSortDirection direction, IEnumerable<SortingResult> items)
        {
            return direction == ImGuiSortDirection.Ascending ? items.OrderBy(c => c.InventoryItem.FormattedName.ToLower()) : items.OrderByDescending(c => c.InventoryItem.FormattedName.ToLower());
        }

        public void Draw(InventoryItem item)
        {
            ImGui.TableNextColumn();
            ImGui.TextColored(item.ItemColour, item.FormattedName);
        }

        public void Draw(SortingResult item)
        {
            ImGui.TableNextColumn();

            var marketBoardData = Universalis.GetMarketBoardData(item.InventoryItem, Service.ClientState.LocalPlayer.CurrentWorld.GameData.Name.RawString);
            if (marketBoardData != null)
            {
                ImGui.TextColored(item.InventoryItem.ItemColour, $"{marketBoardData.currentAveragePrice}");
            } else
            {
                ImGui.TextColored(ImGuiColors.DalamudYellow, $"loading...");
            }
        }

        public void Setup(int columnIndex)
        {
            ImGui.TableSetupColumn(Name, ImGuiTableColumnFlags.WidthFixed, Width,(uint)columnIndex);
        }
    }
}