using CriticalCommonLib;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using CriticalCommonLib.Sheets;
using ImGuiNET;
using InventoryTools.Logic.Columns.Abstract;

namespace InventoryTools.Logic.Columns
{
    public class CraftGatherColumn : CheckboxColumn
    {
        public override bool? CurrentValue(InventoryItem item)
        {
            return Service.ExcelCache.CanBeGathered(item.ItemId);
        }

        public override bool? CurrentValue(ItemEx item)
        {
            return Service.ExcelCache.CanBeGathered(item.RowId);
        }

        public override bool? CurrentValue(SortingResult item)
        {
            return Service.ExcelCache.CanBeGathered(item.InventoryItem.ItemId);
        }

        public override bool? CurrentValue(CraftItem currentValue)
        {
            return Service.ExcelCache.CanBeGathered(currentValue.ItemId);
        }

        public override void Draw(FilterConfiguration configuration, CraftItem item, int rowIndex)
        {
            ImGui.TableNextColumn();
            if (CurrentValue(item) == true)
            {
                if (ImGui.SmallButton("Gather##Gather" + rowIndex))
                {
                    Service.Commands.ProcessCommand("/gather " + item.Name);
                }
            }
        }

        public override string Name { get; set; } = "Gather";
        public override float Width { get; set; } = 100;
        public override string HelpText { get; set; } = "Shows a button that links to gatherbuddy's /gather function.";
        public override string FilterText { get; set; } = "";
        public override bool HasFilter { get; set; } = false;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
    }
}