using System.Collections.Generic;
using System.Numerics;
using AllaganLib.GameSheets.Caches;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services.Mediator;

using Dalamud.Game.Text;
using ImGuiNET;
using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Services;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using Microsoft.Extensions.Logging;
using OtterGui;
using OtterGui.Raii;

namespace InventoryTools.Logic.Columns;

public class CraftMarketPriceColumn : GilColumn
{
    private readonly ExcelSheet<World> _worldSheet;

    public CraftMarketPriceColumn(ILogger<CraftMarketPriceColumn> logger, ImGuiService imGuiService, ExcelSheet<World> worldSheet) : base(logger, imGuiService)
    {
        _worldSheet = worldSheet;
    }

    public override ColumnCategory ColumnCategory { get; } = ColumnCategory.Crafting;
    public override int? CurrentValue(ColumnConfiguration columnConfiguration, SearchResult searchResult)
    {
        return null;
    }

    public override bool HasFilter { get; set; } = true;
    public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;

    public override List<MessageBase>? Draw(FilterConfiguration configuration, ColumnConfiguration columnConfiguration,
        SearchResult searchResult, int rowIndex, int columnIndex)
    {
        if (searchResult.CraftItem == null) return null;

        ImGui.TableNextColumn();
        if (!ImGui.TableGetColumnFlags().HasFlag(ImGuiTableColumnFlags.IsEnabled)) return null;
        if (!searchResult.CraftItem.Item.CanBeTraded) return new List<MessageBase>();
        if (searchResult.CraftItem.MarketTotalPrice != null && searchResult.CraftItem.MarketUnitPrice != null)
        {
            ImGui.Text($"{searchResult.CraftItem.MarketUnitPrice.Value:n0}" + SeIconChar.Gil.ToIconString() + " (" + $"{searchResult.CraftItem.MarketTotalPrice.Value:n0}" + SeIconChar.Gil.ToIconString() + ")");

            if (searchResult.Item.HasSourcesByType(ItemInfoType.GilShop, ItemInfoType.CalamitySalvagerShop))
            {
                var currentIndex = configuration.CraftList.IngredientPreferenceTypeOrder.IndexOf((
                    searchResult.CraftItem.IngredientPreference.Type,
                    searchResult.CraftItem.IngredientPreference.LinkedItemId));
                var buyIndex =
                    configuration.CraftList.IngredientPreferenceTypeOrder.IndexOf((IngredientPreferenceType.Buy, null));
                var houseVendorIndex =
                    configuration.CraftList.IngredientPreferenceTypeOrder.IndexOf((IngredientPreferenceType.HouseVendor,
                        null));
                if (currentIndex < buyIndex || currentIndex < houseVendorIndex)
                {
                    if (searchResult.CraftItem.MarketUnitPrice != 0 && searchResult.CraftItem.MarketUnitPrice < searchResult.Item.BuyFromVendorPrice)
                    {
                        ImGui.SameLine();
                        ImGui.Image(ImGuiService.GetIconTexture(Icons.QuestionMarkIcon).ImGuiHandle, new Vector2(16, 16));
                        ImGuiUtil.HoverTooltip(
                            "The market price of this item is cheaper than buying it from a vendor and you prefer vendors over the current ingredient preference.");
                    }
                }
            }
        }
        else
        {
            ImGui.Text("N/A");
        }

        var craftPrices = searchResult.CraftItem.CraftPrices;
        if (craftPrices != null && craftPrices.Count != 0)
        {
            ImGui.SameLine();
            ImGui.Image(ImGuiService.GetIconTexture(Icons.MarketboardIcon).ImGuiHandle, new Vector2(16,16));
            if (ImGui.IsItemHovered(ImGuiHoveredFlags.None))
            {
                using (var tooltip = ImRaii.Tooltip())
                {
                    if (tooltip.Success)
                    {
                        var totalAvailable = 0;
                        foreach (var price in craftPrices)
                        {
                            var world = _worldSheet.GetRowOrDefault(price.WorldId);
                            if (world != null)
                            {
                                ImGui.Text(price.Left + " available at " + price.UnitPrice +
                                           (price.IsHq ? " (HQ)" : "") + " (" + world.Value.Name.ExtractText() + ")");
                            }

                            totalAvailable += price.Left;
                        }

                        ImGui.Text("Available: " + totalAvailable);

                        if (searchResult.CraftItem.MarketAvailable != searchResult.CraftItem.QuantityNeeded)
                        {
                            ImGui.Text("Missing: " + (searchResult.CraftItem.QuantityNeeded - searchResult.CraftItem.MarketAvailable));
                        }
                    }
                }
            }
        }

        return new List<MessageBase>();
    }

    public override string Name { get; set; } = "Market Pricing";
    public override float Width { get; set; } = 150;
    public override string HelpText { get; set; } = "The current market pricing for the given item. ";

    public override FilterType DefaultIn => Logic.FilterType.CraftFilter;
    public override FilterType AvailableIn => Logic.FilterType.CraftFilter;
}
