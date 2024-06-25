using System.Collections.Generic;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services.Mediator;
using CriticalCommonLib.Sheets;
using ImGuiNET;
using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;
using ImGuiUtil = InventoryTools.Ui.Widgets.ImGuiUtil;

namespace InventoryTools.Logic.Columns
{
    public class CraftAmountRequiredColumn : DoubleIntegerColumn
    {
        public CraftAmountRequiredColumn(ILogger<CraftAmountRequiredColumn> logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
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
            if (searchResult.CraftItem.IsOutputItem)
            {
                var value = CurrentValue(columnConfiguration, searchResult)?.Item2.ToString() ?? "";
                ImGuiUtil.VerticalAlignButton(configuration.TableHeight);
                if (ImGui.InputText("##"+searchResult.CraftItem.ItemId+"RequiredInput" + columnIndex, ref value, 4, ImGuiInputTextFlags.CharsDecimal))
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
                            if (number != searchResult.CraftItem.QuantityRequired && configuration.CraftList.BeenGenerated && configuration.CraftList.BeenUpdated)
                            {
                                configuration.CraftList.SetCraftRequiredQuantity(searchResult.CraftItem.ItemId, number,
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
                ImGuiUtil.VerticalAlignText(searchResult.CraftItem.QuantityNeeded + "/" + searchResult.CraftItem.QuantityNeededPreUpdate, configuration.TableHeight, false);
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