using CriticalCommonLib.Crafting;
using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using Dalamud.Logging;
using ImGuiNET;
using InventoryTools.Logic.Columns.Abstract;

namespace InventoryTools.Logic.Columns
{
    public class CraftAmountRequiredColumn : IntegerColumn
    {
        public override int? CurrentValue(InventoryItem item)
        {
            return 0;
        }

        public override int? CurrentValue(ItemEx item)
        {
            return 0;
        }

        public override int? CurrentValue(SortingResult item)
        {
            return 0;
        }

        public override int? CurrentValue(CraftItem currentValue)
        {
            if (currentValue.IsOutputItem)
            {
                return (int)currentValue.QuantityRequired;
            }
            return (int)currentValue.QuantityNeeded;
        }

        public override void Draw(FilterConfiguration configuration, CraftItem item, int rowIndex)
        {
            if (item.IsOutputItem)
            {
                ImGui.TableNextColumn();
                var value = CurrentValue(item)?.ToString() ?? "";
                if (ImGui.InputText("##"+rowIndex+"RequiredInput", ref value, 4, ImGuiInputTextFlags.CharsDecimal))
                {
                    if (value != (CurrentValue(item)?.ToString() ?? ""))
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
        public override string HelpText { get; set; } = "This is the amount required to complete the craft.";
        public override string FilterText { get; set; } = "";
        public override bool HasFilter { get; set; } = false;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
    }
}