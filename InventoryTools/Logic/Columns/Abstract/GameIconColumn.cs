using System.Collections.Generic;
using System.Numerics;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Models;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;

namespace InventoryTools.Logic.Columns.Abstract
{
    public abstract class GameIconColumn : Column<(ushort,bool)?>
    {
        public override string CsvExport(InventoryItem item)
        {
            return "";
        }

        public override string CsvExport(Item item)
        {
            return "";
        }

        public override string CsvExport(SortingResult item)
        {
            return "";
        }
        public override (ushort,bool)? CurrentValue(CraftItem currentValue)
        {
            if (currentValue.Item == null)
            {
                return null;
            }

            return CurrentValue(currentValue.Item);
        }
        
        public override IEnumerable<CraftItem> Filter(IEnumerable<CraftItem> items)
        {
            return items;
        }

        public override IEnumerable<CraftItem> Sort(ImGuiSortDirection direction, IEnumerable<CraftItem> items)
        {
            return items;
        }
        public virtual Vector2 IconSize
        {
            get
            {
                return new Vector2(32, 32);
            }
        }
        public virtual string EmptyText
        {
            get
            {
                return "N/A";
            }
        }
        public override void Draw(InventoryItem item, int rowIndex)
        {
            DoDraw(CurrentValue(item), rowIndex);
        }
        public override void Draw(SortingResult item, int rowIndex)
        {
            DoDraw(CurrentValue(item), rowIndex);
        }
        public override void Draw(Item item, int rowIndex)
        {
            DoDraw(CurrentValue(item), rowIndex);
        }
        public override void Draw(CraftItem item, int rowIndex, FilterConfiguration configuration)
        {
            DoDraw(CurrentValue(item), rowIndex);
        }

        public override IEnumerable<Item> Filter(IEnumerable<Item> items)
        {
            return items;
        }

        public override IEnumerable<InventoryItem> Filter(IEnumerable<InventoryItem> items)
        {
            return items;
        }

        public override IEnumerable<SortingResult> Filter(IEnumerable<SortingResult> items)
        {
            return items;
        }

        public override IEnumerable<InventoryItem> Sort(ImGuiSortDirection direction, IEnumerable<InventoryItem> items)
        {
            return items;
        }

        public override IEnumerable<Item> Sort(ImGuiSortDirection direction, IEnumerable<Item> items)
        {
            return items;
        }

        public override IEnumerable<SortingResult> Sort(ImGuiSortDirection direction, IEnumerable<SortingResult> items)
        {
            return items;
        }

        public override IColumnEvent? DoDraw((ushort,bool)? currentValue, int rowIndex)
        {
            ImGui.TableNextColumn();
            if (currentValue != null)
            {
                PluginService.PluginLogic.DrawIcon(currentValue.Value.Item1, IconSize, currentValue.Value.Item2);
            }
            return null;
        }

        public override void Setup(int columnIndex)
        {
            ImGui.TableSetupColumn(Name, ImGuiTableColumnFlags.WidthFixed, Width, (uint)columnIndex);
        }
    }
}