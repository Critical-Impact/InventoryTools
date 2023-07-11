using CriticalCommonLib.Crafting;
using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using ImGuiNET;
using InventoryTools.Logic.Columns.Abstract;
using ImGuiUtil = InventoryTools.Ui.Widgets.ImGuiUtil;

namespace InventoryTools.Logic.Columns
{
    public class CraftAmountRequiredColumn : DoubleIntegerColumn
    {
        public override ColumnCategory ColumnCategory => ColumnCategory.Crafting;
        public override (int,int)? CurrentValue(InventoryItem item)
        {
            return null;
        }

        public override (int,int)? CurrentValue(ItemEx item)
        {
            return null;
        }

        public override (int,int)? CurrentValue(SortingResult item)
        {
            return null;
        }

        public override (int,int)? CurrentValue(CraftItem currentValue)
        {
            if (currentValue.IsOutputItem)
            {
                return ((int)currentValue.QuantityNeeded,(int)currentValue.QuantityRequired);
            }
            return ((int)currentValue.QuantityNeeded,(int)currentValue.QuantityRequired);
        }

        public override void Draw(FilterConfiguration configuration, CraftItem item, int rowIndex)
        {
            ImGui.TableNextColumn();
            if (item.IsOutputItem)
            {
                var value = CurrentValue(item)?.Item2.ToString() ?? "";
                ImGuiUtil.VerticalAlignButton(configuration.TableHeight);
                if (ImGui.InputText("##"+item.ItemId+"RequiredInput", ref value, 4, ImGuiInputTextFlags.CharsDecimal))
                {
                    if (value != (CurrentValue(item)?.Item2.ToString() ?? ""))
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
                                configuration.StartRefresh();
                            }
                        }
                    }
                }
            }
            else
            {
                ImGuiUtil.VerticalAlignText(item.QuantityNeeded + "/" + item.QuantityNeededPreUpdate, configuration.TableHeight, false);
            }
        }
        public override FilterType AvailableIn { get; } = Logic.FilterType.CraftFilter;
        public override string Name { get; set; } = "Amount Required";
        public override string RenderName => "Required";
        public override float Width { get; set; } = 60;
        public override bool? CraftOnly => true;
        public override string HelpText { get; set; } = "This is the amount required for this item in the craft.";
        public override bool HasFilter { get; set; } = false;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
    }
}