using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using CriticalCommonLib.Models;
using Dalamud.Interface.Colors;
using Dalamud.Logging;
using ImGuiNET;
using InventoryTools.Extensions;
using InventoryTools.MarketBoard;

namespace InventoryTools.Logic
{
    public class MarketBoardPriceColumn : IColumn
    {
        public string Name { get; set; } = "MB Average Price";
        public float Width { get; set; } = 250.0f;
        public string FilterText { get; set; } = "";

        private static readonly string LOADING = "loading...";
        private static readonly string UNTRADABLE = "untradable";

        private string Value(InventoryItem item, bool forceCheck = false)
        {
            if (!item.CanBeTraded)
            {
                return UNTRADABLE;
            }

            var marketBoardData = Cache.GetData(item.ItemId, fromCheck: false, forceCheck: forceCheck);
            if (marketBoardData != null)
            {
                if (item.IsHQ)
                {
                    if (marketBoardData.calculcatedPriceHQ != "N/A")
                    {
                        return $"{marketBoardData.calculcatedPriceHQ}";
                    }
                }
                return $"{marketBoardData.calculcatedPrice}";
            }

            return LOADING;
        }

        private double ValueDouble(InventoryItem item, bool forceCheck = false)
        {
            var value = Value(item,forceCheck);
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
            if (FilterText.Contains(">") || FilterText.Contains("=") || FilterText.Contains("<"))
            {
                return FilterText == "" ? items : items.Where(c => ValueDouble(c).PassesFilter(FilterText.ToLower()));
            }
            return FilterText == "" ? items : items.Where(c => Value(c).PassesFilter(FilterText.ToLower()));
        }

        public IEnumerable<SortingResult> Filter(IEnumerable<SortingResult> items)
        {
            if (FilterText.Contains(">") || FilterText.Contains("=") || FilterText.Contains("<"))
            {
                return FilterText == "" ? items : items.Where(c => ValueDouble(c.InventoryItem).PassesFilter(FilterText.ToLower()));
            }
            return FilterText == "" ? items : items.Where(c => Value(c.InventoryItem).PassesFilter(FilterText.ToLower()));
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

        public void Draw(SortingResult item, int rowIndex)
        {
            ImGui.TableNextColumn();

            var marketBoardData = Value(item.InventoryItem);
            if (marketBoardData != LOADING)
            {
                
                ImGui.TextColored(marketBoardData != UNTRADABLE ? item.InventoryItem.ItemColour : ImGuiColors.DalamudRed, $"{marketBoardData}");
            }
            else
            {
                ImGui.TextColored(ImGuiColors.DalamudYellow, LOADING);
            }

            if (marketBoardData != LOADING && marketBoardData != UNTRADABLE)
            {
                ImGui.SameLine();
                if (ImGui.SmallButton("R##" + rowIndex))
                {
                    PluginLog.Verbose("Forcing a universalis check");
                    Value(item.InventoryItem, true);
                }
            }
        }

        public void Setup(int columnIndex)
        {
            ImGui.TableSetupColumn(Name, ImGuiTableColumnFlags.WidthFixed, Width, (uint)columnIndex);
        }
    }
}