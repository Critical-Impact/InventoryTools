using CriticalCommonLib.Crafting;
using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using ImGuiNET;
using InventoryTools.Logic.Columns.Abstract;

namespace InventoryTools.Logic.Columns
{
    public class CraftGatherColumn : CheckboxColumn
    {
        public override ColumnCategory ColumnCategory => ColumnCategory.Tools;

        public override bool? CurrentValue(InventoryItem item)
        {
            return CurrentValue(item.Item);
        }

        public override bool? CurrentValue(ItemEx item)
        {
            return item.CanBeGathered || item.ObtainedFishing;
        }

        public override bool? CurrentValue(SortingResult item)
        {
            return CurrentValue(item.InventoryItem);
        }

        public override bool? CurrentValue(CraftItem currentValue)
        {
            return CurrentValue(currentValue.Item);
        }

        public override void Draw(FilterConfiguration configuration, CraftItem item, int rowIndex)
        {
            ImGui.TableNextColumn();
            if (CurrentValue(item) == true)
            {
                if (item.Item.ObtainedGathering)
                {
                    if (ImGui.SmallButton("Gather##Gather" + rowIndex))
                    {
                        PluginService.CommandService.ProcessCommand("/gather " + item.Name);
                    }
                }
                else if(item.Item.ObtainedFishing)
                {
                    if (ImGui.SmallButton("Gather##Gather" + rowIndex))
                    {
                        PluginService.CommandService.ProcessCommand("/gatherfish " + item.Name);
                    }
                }
            }
        }

        public override void Draw(FilterConfiguration configuration, InventoryItem item, int rowIndex)
        {
            ImGui.TableNextColumn();
            if (CurrentValue(item) == true)
            {
                if (item.Item.ObtainedGathering)
                {
                    if (ImGui.SmallButton("Gather##Gather" + rowIndex))
                    {
                        PluginService.CommandService.ProcessCommand("/gather " + item.Item.NameString);
                    }
                }
                else if (item.Item.ObtainedFishing)
                {
                    if (ImGui.SmallButton("Gather##Gather" + rowIndex))
                    {
                        PluginService.CommandService.ProcessCommand("/gatherfish " + item.Item.NameString);
                    }
                }
            }
        }

        public override void Draw(FilterConfiguration configuration, ItemEx item, int rowIndex)
        {
            ImGui.TableNextColumn();
            if (CurrentValue(item) == true)
            {
                if (item.ObtainedGathering)
                {
                    if (ImGui.SmallButton("Gather##Gather" + rowIndex))
                    {
                        PluginService.CommandService.ProcessCommand("/gather " + item.NameString);
                    }
                }
                else if (item.ObtainedFishing)
                {
                    if (ImGui.SmallButton("Gather##Gather" + rowIndex))
                    {
                        PluginService.CommandService.ProcessCommand("/gatherfish " + item.NameString);
                    }
                }
            }
        }

        public override string Name { get; set; } = "Gather";
        public override float Width { get; set; } = 100;
        public override string HelpText { get; set; } = "Shows a button that links to gatherbuddy's /gather function.";
        public override bool HasFilter { get; set; } = false;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
    }
}