using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using Dalamud.Logging;
using ImGuiNET;
using InventoryTools.Extensions;
using InventoryTools.Logic.Columns.Abstract;

namespace InventoryTools.Logic.Columns
{
    public class ShortcutColumn : TextColumn
    {
        public override string? CurrentValue(InventoryItem item)
        {
            return null;
        }

        public override string? CurrentValue(ItemEx item)
        {
            return null;
        }

        public override string? CurrentValue(SortingResult item)
        {
            return null;
        }

        public override string Name { get; set; } = "Shortcuts";
        public override float Width { get; set; } = 32.0f;
        public override string FilterText { get; set; } = "";
        public override bool HasFilter { get; set; } = false;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
        
        public override void Draw(InventoryItem item, int rowIndex)
        {
            ImGui.TableNextColumn();
            if (ImGui.SmallButton("G##G" + rowIndex))
            {
                $"https://www.garlandtools.org/db/#item/{item.ItemId}".OpenBrowser();
            }
            if (item.Item.CanTryOn)
            {
                ImGui.SameLine();
                if (ImGui.SmallButton("Try On##TO" + rowIndex))
                {
                    if (PluginService.TryOn.CanUseTryOn)
                    {
                        PluginService.TryOn.TryOnItem(item.Item, 0, item.IsHQ);
                    }
                    else
                    {
                        PluginLog.Error("Something went wrong while attempting to try on " + item.Item.Name);
                    }
                }
            }
        }

        public override void Draw(SortingResult item, int rowIndex)
        {
           Draw(item.InventoryItem, rowIndex);
        }

        public override void Draw(ItemEx item, int rowIndex)
        {
            ImGui.TableNextColumn();
            if (ImGui.SmallButton("G##G"+rowIndex))
            {
                $"https://www.garlandtools.org/db/#item/{item.RowId}".OpenBrowser();
            }
            if (item.CanTryOn)
            {
                ImGui.SameLine();
                if (ImGui.SmallButton("Try On##TO" + rowIndex))
                {
                    if (PluginService.TryOn.CanUseTryOn)
                    {
                        PluginService.TryOn.TryOnItem(item, 0, false);
                    }
                    else
                    {
                        PluginLog.Error("Something went wrong while attempting to try on " + item.Name);
                    }
                }
            }
        }
    }
}