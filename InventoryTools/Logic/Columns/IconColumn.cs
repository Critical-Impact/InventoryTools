using System.Numerics;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using ImGuiNET;
using InventoryTools.Logic.Columns.Abstract;

namespace InventoryTools.Logic.Columns
{
    public class IconColumn : GameIconColumn
    {
        public override ColumnCategory ColumnCategory => ColumnCategory.Basic;
        public override (ushort, bool)? CurrentValue(InventoryItem item)
        {
            return (item.Icon, item.IsHQ);
        }

        public override (ushort, bool)? CurrentValue(ItemEx item)
        {
            return (item.Icon, false);
        }

        public override (ushort, bool)? CurrentValue(SortingResult item)
        {
            return CurrentValue(item.InventoryItem);
        }
        
        public override dynamic? JsonExport(InventoryItem item)
        {
            return CurrentValue(item)?.Item1;
        }

        public override dynamic? JsonExport(ItemEx item)
        {
            return CurrentValue(item)?.Item1;
        }

        public override dynamic? JsonExport(SortingResult item)
        {
            return CurrentValue(item)?.Item1;
        }

        public override dynamic? JsonExport(CraftItem item)
        {
            return CurrentValue(item)?.Item1;
        }

        public override IColumnEvent? DoDraw((ushort, bool)? currentValue, int rowIndex,
            FilterConfiguration filterConfiguration)
        {
            ImGui.TableNextColumn();
            if (currentValue != null)
            {
                var textureWrap = PluginService.PluginLogic.GetIcon(currentValue.Value.Item1, currentValue.Value.Item2);
                if (textureWrap != null)
                {
                    ImGui.PushID("icon" + rowIndex);
                    if (ImGui.ImageButton(textureWrap.ImGuiHandle, new Vector2(filterConfiguration.TableHeight, filterConfiguration.TableHeight) * ImGui.GetIO().FontGlobalScale,new Vector2(0,0), new Vector2(1,1), 2))
                    {
                        ImGui.PopID();
                        return new ItemIconPressedColumnEvent();
                    }
                    ImGui.PopID();
                }
                else
                {
                    ImGui.Button("", new Vector2(filterConfiguration.TableHeight, filterConfiguration.TableHeight) * ImGui.GetIO().FontGlobalScale);
                }
            }
            return null;
            
        }


        public override string Name { get; set; } = "Icon";
        public override string RenderName => "";
        public override float Width { get; set; } = 60.0f;
        public override string HelpText { get; set; } = "Shows the icon of the item, pressing it will open the more information window for the item.";
        public override bool HasFilter { get; set; } = false;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
    }
}