using CriticalCommonLib.Crafting;
using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using ImGuiNET;
using InventoryTools.Logic.Columns.Abstract;

namespace InventoryTools.Logic.Columns
{
    public class CraftAmountRequiredColumn : DoubleIntegerColumn
    {
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
            if (item.IsOutputItem)
            {
                ImGui.TableNextColumn();
                var value = CurrentValue(item)?.Item2.ToString() ?? "";
                if (ImGui.InputText("##"+rowIndex+"RequiredInput", ref value, 4, ImGuiInputTextFlags.CharsDecimal))
                {
                    if (value != (CurrentValue(item)?.Item2.ToString() ?? ""))
                    {
                        int parsedNumber;
                        if (int.TryParse(value, out parsedNumber))
                        {
                            var number = (uint) parsedNumber;
                            if (number != item.QuantityRequired)
                            {
                                //TODO: Probably a better way to do this
                                configuration.CraftList.SetCraftRequiredQuantity(item.ItemId, number, item.Flags,
                                    item.Phase);
                                item.QuantityRequired = number;
                                configuration.StartRefresh();
                            }
                        }
                    }
                }
            }
            else
            {
                base.Draw(configuration, item, rowIndex);
            }
        }
        public override FilterType AvailableIn { get; } = Logic.FilterType.CraftFilter;
        public override string Name { get; set; } = "Required";
        public override float Width { get; set; } = 60;
        public override bool? CraftOnly => true;
        public override string HelpText { get; set; } = "This is the amount required to complete the craft.";
        public override bool HasFilter { get; set; } = false;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
    }
}