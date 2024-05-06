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
        public override (int, int)? CurrentValue(ColumnConfiguration columnConfiguration, InventoryItem item)
        {
            return null;
        }

        public override (int, int)? CurrentValue(ColumnConfiguration columnConfiguration, ItemEx item)
        {
            return null;
        }

        public override (int, int)? CurrentValue(ColumnConfiguration columnConfiguration, SortingResult item)
        {
            return null;
        }

        public override (int, int)? CurrentValue(ColumnConfiguration columnConfiguration, CraftItem currentValue)
        {
            if (currentValue.IsOutputItem)
            {
                return ((int)currentValue.QuantityNeeded,(int)currentValue.QuantityRequired);
            }
            return ((int)currentValue.QuantityNeeded,(int)currentValue.QuantityRequired);
        }

        public override List<MessageBase>? Draw(FilterConfiguration configuration,
            ColumnConfiguration columnConfiguration,
            CraftItem item, int rowIndex)
        {
            ImGui.TableNextColumn();
            if (item.IsOutputItem)
            {
                var value = CurrentValue(columnConfiguration, item)?.Item2.ToString() ?? "";
                ImGuiUtil.VerticalAlignButton(configuration.TableHeight);
                if (ImGui.InputText("##"+item.ItemId+"RequiredInput", ref value, 4, ImGuiInputTextFlags.CharsDecimal))
                {
                    if (value != (CurrentValue(columnConfiguration, item)?.Item2.ToString() ?? ""))
                    {
                        int parsedNumber;
                        if (int.TryParse(value, out parsedNumber))
                        {
                            if (parsedNumber < 0)
                            {
                                parsedNumber = 0;
                            }
                            var number = item.GetRoundedQuantity((uint)parsedNumber);
                            if (number != item.QuantityRequired && configuration.CraftList.BeenGenerated && configuration.CraftList.BeenUpdated)
                            {
                                configuration.CraftList.SetCraftRequiredQuantity(item.ItemId, number,
                                    item.Flags,
                                    item.Phase);
                                item.QuantityRequired = number;
                                configuration.NeedsRefresh = true;
                            }
                        }
                    }
                }
            }
            else
            {
                ImGuiUtil.VerticalAlignText(item.QuantityNeeded + "/" + item.QuantityNeededPreUpdate, configuration.TableHeight, false);
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