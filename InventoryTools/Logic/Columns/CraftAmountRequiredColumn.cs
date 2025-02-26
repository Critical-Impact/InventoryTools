using System;
using System.Collections.Generic;
using AllaganLib.GameSheets.Sheets;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Services.Mediator;
using DalaMock.Shared.Interfaces;
using Dalamud.Interface;
using ImGuiNET;
using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;
using OtterGui.Raii;
using ImGuiUtil = InventoryTools.Ui.Widgets.ImGuiUtil;

namespace InventoryTools.Logic.Columns
{
    public class CraftAmountRequiredColumn : DoubleIntegerColumn
    {
        private readonly IFont _font;
        private readonly ItemSheet _itemSheet;

        public CraftAmountRequiredColumn(ILogger<CraftAmountRequiredColumn> logger, ImGuiService imGuiService, IFont font, ItemSheet itemSheet) : base(logger, imGuiService)
        {
            _font = font;
            _itemSheet = itemSheet;
        }
        public override ColumnCategory ColumnCategory => ColumnCategory.Crafting;

        public override (int, int)? CurrentValue(ColumnConfiguration columnConfiguration, SearchResult searchResult)
        {
            if (searchResult.CraftItem == null) return null;

            if (searchResult.CraftItem.IsOutputItem)
            {
                return ((int)searchResult.CraftItem.QuantityNeeded,(int)searchResult.CraftItem.QuantityRequired);
            }
            return ((int)searchResult.CraftItem.QuantityNeeded,(int)searchResult.CraftItem.QuantityRequired);
        }

        public override List<MessageBase>? Draw(FilterConfiguration configuration,
            ColumnConfiguration columnConfiguration,
            SearchResult searchResult, int rowIndex, int columnIndex)
        {
            if (searchResult.CraftItem == null) return null;

            ImGui.TableNextColumn();
            if (!ImGui.TableGetColumnFlags().HasFlag(ImGuiTableColumnFlags.IsEnabled)) return null;
            var originalCursorPosY = ImGui.GetCursorPosY();
            var itemHovered = false;

            if (searchResult.CraftItem.IsOutputItem)
            {
                if (configuration.CraftList.CraftListMode == CraftListMode.Normal)
                {
                    var value = CurrentValue(columnConfiguration, searchResult)?.Item2.ToString() ?? "";
                    ImGuiUtil.VerticalAlignButton(configuration.TableHeight);
                    if (ImGui.InputText("##" + searchResult.CraftItem.ItemId + "RequiredInput" + columnIndex, ref value,
                            4, ImGuiInputTextFlags.CharsDecimal))
                    {
                        if (value != (CurrentValue(columnConfiguration, searchResult)?.Item2.ToString() ?? ""))
                        {
                            int parsedNumber;
                            if (int.TryParse(value, out parsedNumber))
                            {
                                if (parsedNumber < 0)
                                {
                                    parsedNumber = 0;
                                }

                                var number = searchResult.CraftItem.GetRoundedQuantity((uint)parsedNumber);
                                if (number != searchResult.CraftItem.QuantityRequired &&
                                    configuration.CraftList.BeenGenerated && configuration.CraftList.BeenUpdated)
                                {
                                    configuration.CraftList.SetCraftRequiredQuantity(searchResult.CraftItem.ItemId,
                                        number,
                                        searchResult.CraftItem.Flags,
                                        searchResult.CraftItem.Phase);
                                    searchResult.CraftItem.QuantityRequired = number;
                                    configuration.NeedsRefresh = true;
                                }
                            }
                        }
                    }
                }
                else
                {
                    var widthAvailable = ImGui.GetContentRegionAvail().X / 2;

                    var value = searchResult.CraftItem.QuantityReady.ToString();
                    ImGuiUtil.VerticalAlignButton(configuration.TableHeight);
                    ImGui.SetNextItemWidth(widthAvailable - ImGui.GetStyle().ItemSpacing.X);
                    using (var disabled = ImRaii.Disabled())
                    {
                        if (disabled)
                        {
                            if (ImGui.InputText("##" + searchResult.CraftItem.ItemId + "RequiredInput" + columnIndex,
                                    ref value,
                                    4, ImGuiInputTextFlags.CharsDecimal))
                            {

                            }
                        }
                    }

                    if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
                    {
                        itemHovered = true;
                    }

                    var toStock = searchResult.CraftItem.QuantityToStock.ToString();
                    ImGui.SameLine();
                    ImGui.SetNextItemWidth(widthAvailable);
                    if (ImGui.InputText("##" + searchResult.CraftItem.ItemId + "StockInput" + columnIndex, ref toStock,
                            4, ImGuiInputTextFlags.CharsDecimal))
                    {
                        if (toStock != (searchResult.CraftItem.QuantityToStock.ToString() ?? ""))
                        {
                            int parsedNumber;
                            if (int.TryParse(toStock, out parsedNumber))
                            {
                                if (parsedNumber < 0)
                                {
                                    parsedNumber = 0;
                                }

                                var number = searchResult.CraftItem.GetRoundedQuantity((uint)parsedNumber);
                                if (number != searchResult.CraftItem.QuantityToStock &&
                                    configuration.CraftList.BeenGenerated && configuration.CraftList.BeenUpdated)
                                {
                                    configuration.CraftList.SetCraftToStockQuantity(searchResult.CraftItem.ItemId,
                                        number,
                                        searchResult.CraftItem.Flags,
                                        searchResult.CraftItem.Phase);
                                    searchResult.CraftItem.QuantityToStock = number;
                                    configuration.NeedsRefresh = true;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                ImGuiUtil.VerticalAlignText(searchResult.CraftItem.QuantityNeeded + "/" + searchResult.CraftItem.QuantityNeededPreUpdate, configuration.TableHeight, false);
            }
            ImGui.SameLine();
            ImGui.SetCursorPosY(originalCursorPosY);
            ImGui.PushFont(_font.IconFont);
            ImGuiUtil.VerticalAlignTextDisabled(FontAwesomeIcon.InfoCircle.ToIconString(), configuration.TableHeight, false);
            ImGui.PopFont();
            if (itemHovered || ImGui.IsItemHovered(ImGuiHoveredFlags.None))
            {
                using var tt = ImRaii.Tooltip();
                ImGui.Text("Ingredient Breakdown:");
                ImGui.TextUnformatted("Amount Originally Required: " + searchResult.CraftItem.QuantityRequired);
                ImGui.TextUnformatted("Amount Required: " + searchResult.CraftItem.QuantityNeededPreUpdate);
                ImGui.TextUnformatted("Amount in Inventory: " + searchResult.CraftItem.QuantityReady);
                ImGui.TextUnformatted("Amount to Retrieve: " + searchResult.CraftItem.QuantityAvailable);
                ImGui.Separator();
                ImGui.TextUnformatted("Amount Missing: " + searchResult.CraftItem.QuantityMissingOverall);
                if (searchResult.Item.CanBeCrafted)
                {
                    ImGui.TextUnformatted("Amount Craftable: " + searchResult.CraftItem.QuantityCanCraft);
                    if (searchResult.CraftItem.Yield != 1)
                    {
                        ImGui.Separator();
                        ImGui.TextUnformatted("Craft Operations Required: " +
                                              searchResult.CraftItem.QuantityNeeded / searchResult.CraftItem.Yield);
                        ImGui.TextUnformatted("Recipe Yield: " + searchResult.CraftItem.Yield);
                    }
                }


                if (searchResult.CraftItem.Recipe != null)
                {
                    ImGui.Separator();
                    ImGui.TextUnformatted("Ingredients: ");
                    using (ImRaii.PushIndent())
                    {
                        foreach (var ingredient in searchResult.CraftItem.Ingredients)
                        {
                            var item = _itemSheet.GetRow(ingredient.Key.Item1);
                            var quantityRequired = ingredient.Value;
                            ImGui.TextUnformatted(item.NameString + ": " + quantityRequired);
                        }
                    }
                }

            }
            return null;
        }
        public override FilterType AvailableIn { get; } = Logic.FilterType.CraftFilter;
        public override string Name { get; set; } = "Amount Required";
        public override string RenderName => "Required";
        public override float Width { get; set; } = 60;
        public override bool? CraftOnly => true;
        public override string HelpText { get; set; } = "The amount required with inventory and external sources factored in/The amount required without inventory and external sources factored in.";
        public override bool HasFilter { get; set; } = false;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;

        public override FilterType DefaultIn => Logic.FilterType.CraftFilter;
    }
}