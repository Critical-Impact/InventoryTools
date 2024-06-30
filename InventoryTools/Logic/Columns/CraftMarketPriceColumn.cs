using System.Collections.Generic;
using System.Numerics;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using CriticalCommonLib.Services.Mediator;
using CriticalCommonLib.Sheets;
using Dalamud.Game.Text;
using ImGuiNET;
using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;
using OtterGui;
using OtterGui.Raii;

namespace InventoryTools.Logic.Columns;

public class CraftMarketPriceColumn : GilColumn
{
    private readonly ExcelCache _excelCache;

    public CraftMarketPriceColumn(ILogger<CraftMarketPriceColumn> logger, ImGuiService imGuiService, ExcelCache excelCache) : base(logger, imGuiService)
    {
        _excelCache = excelCache;
    }

    public override ColumnCategory ColumnCategory { get; } = ColumnCategory.Crafting;
    public override int? CurrentValue(ColumnConfiguration columnConfiguration, InventoryItem item)
    {
        return null;
    }

    public override int? CurrentValue(ColumnConfiguration columnConfiguration, ItemEx item)
    {
        return null;
    }

    public override int? CurrentValue(ColumnConfiguration columnConfiguration, SortingResult item)
    {
        return null;
    }

    public override bool HasFilter { get; set; } = true;
    public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;

    public override List<MessageBase>? Draw(FilterConfiguration configuration, ColumnConfiguration columnConfiguration,
        CraftItem item, int rowIndex, int columnIndex)
    {
        ImGui.TableNextColumn();
        if (!item.Item.CanBeTraded) return new List<MessageBase>();
        if (item.MarketTotalPrice != null && item.MarketUnitPrice != null)
        {
            ImGui.Text($"{item.MarketUnitPrice.Value:n0}" + SeIconChar.Gil.ToIconString() + " (" + $"{item.MarketTotalPrice.Value:n0}" + SeIconChar.Gil.ToIconString() + ")");

            if (item.Item.ObtainedGil)
            {
                var currentIndex = configuration.CraftList.IngredientPreferenceTypeOrder.IndexOf((
                    item.IngredientPreference.Type,
                    item.IngredientPreference.LinkedItemId));
                var buyIndex =
                    configuration.CraftList.IngredientPreferenceTypeOrder.IndexOf((IngredientPreferenceType.Buy, null));
                var houseVendorIndex =
                    configuration.CraftList.IngredientPreferenceTypeOrder.IndexOf((IngredientPreferenceType.HouseVendor,
                        null));
                if (currentIndex < buyIndex || currentIndex < houseVendorIndex)
                {
                    if (item.MarketUnitPrice != 0 && item.MarketUnitPrice < item.Item.BuyFromVendorPrice)
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

        var craftPrices = item.CraftPrices;
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
                            var world = _excelCache.GetWorldSheet().GetRow(price.WorldId);
                            if (world != null)
                            {
                                ImGui.Text(price.Left + " available at " + price.UnitPrice +
                                           (price.IsHq ? " (HQ)" : "") + " (" + world.FormattedName + ")");
                            }

                            totalAvailable += price.Left;
                        }
                        
                        ImGui.Text("Available: " + totalAvailable);

                        if (item.MarketAvailable != item.QuantityNeeded)
                        {
                            ImGui.Text("Missing: " + (item.QuantityNeeded - item.MarketAvailable));
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
