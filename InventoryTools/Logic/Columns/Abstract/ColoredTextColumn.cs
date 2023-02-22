using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using ImGuiNET;
using InventoryTools.Extensions;

namespace InventoryTools.Logic.Columns.Abstract
{
    public abstract class ColoredTextColumn : Column<(string, Vector4)?>
    {
        public override string CsvExport(InventoryItem item)
        {
            return CurrentValue(item)?.Item1 ?? "";
        }

        public override string CsvExport(ItemEx item)
        {
            return CurrentValue((ItemEx)item)?.Item1 ?? "";
        }

        public override string CsvExport(SortingResult item)
        {
            return CurrentValue(item)?.Item1 ?? "";
        }
        
        public override (string, Vector4)? CurrentValue(CraftItem currentValue)
        {
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


        public virtual string EmptyText
        {
            get
            {
                return "N/A";
            }
        }
        public override void Draw(FilterConfiguration configuration, InventoryItem item, int rowIndex)
        {
            DoDraw(CurrentValue(item), rowIndex, configuration);
        }
        public override void Draw(FilterConfiguration configuration, SortingResult item, int rowIndex)
        {
            DoDraw(CurrentValue(item), rowIndex, configuration);
        }
        public override void Draw(FilterConfiguration configuration, ItemEx item, int rowIndex)
        {
            DoDraw(CurrentValue((ItemEx)item), rowIndex, configuration);
        }
        public override void Draw(FilterConfiguration configuration, CraftItem item, int rowIndex)
        {
            DoDraw(CurrentValue(item), rowIndex, configuration);
        }

        public override IEnumerable<ItemEx> Filter(IEnumerable<ItemEx> items)
        {
            return FilterText == "" ? items : items.AsParallel().Where(c =>
            {
                var currentValue = CurrentValue(c);
                if (currentValue == null)
                {
                    return false;
                }

                return currentValue.Value.Item1.ToLower().PassesFilter(FilterComparisonText);
            });
        }

        public override IEnumerable<InventoryItem> Filter(IEnumerable<InventoryItem> items)
        {
            var isChecked = FilterText != "";
            return FilterText == "" ? items : items.AsParallel().Where(c =>
            {
                var currentValue = CurrentValue(c);
                if (currentValue == null)
                {
                    return false;
                }

                return currentValue.Value.Item1.ToLower().PassesFilter(FilterComparisonText);
            });
        }

        public override IEnumerable<SortingResult> Filter(IEnumerable<SortingResult> items)
        {
            var isChecked = FilterText != "";
            return FilterText == "" ? items : items.AsParallel().Where(c =>
            {
                var currentValue = CurrentValue(c);
                if (currentValue == null)
                {
                    return false;
                }

                return currentValue.Value.Item1.ToLower().PassesFilter(FilterComparisonText);
            });
        }

        public override IEnumerable<InventoryItem> Sort(ImGuiSortDirection direction, IEnumerable<InventoryItem> items)
        {
            return direction == ImGuiSortDirection.Ascending ? items.OrderBy(item =>
            {
                var currentValue = CurrentValue(item);
                return !currentValue.HasValue ? "" : currentValue.Value.Item1;
            }) : items.OrderByDescending(item =>
            {
                var currentValue = CurrentValue(item);
                return !currentValue.HasValue ? "" : currentValue.Value.Item1;
            });
        }

        public override IEnumerable<ItemEx> Sort(ImGuiSortDirection direction, IEnumerable<ItemEx> items)
        {
            return direction == ImGuiSortDirection.Ascending ? items.OrderBy(item =>
            {
                var currentValue = CurrentValue((ItemEx)item);
                return !currentValue.HasValue ? "" : currentValue.Value.Item1;
            }) : items.OrderByDescending(item =>
            {
                var currentValue = CurrentValue((ItemEx)item);
                return !currentValue.HasValue ? "" : currentValue.Value.Item1;
            });
        }

        public override IEnumerable<SortingResult> Sort(ImGuiSortDirection direction, IEnumerable<SortingResult> items)
        {
            return direction == ImGuiSortDirection.Ascending ? items.OrderBy(item =>
            {
                var currentValue = CurrentValue(item);
                return !currentValue.HasValue ? "" : currentValue.Value.Item1;
            }) : items.OrderByDescending(item =>
            {
                var currentValue = CurrentValue(item);
                return !currentValue.HasValue ? "" : currentValue.Value.Item1;
            });
        }

        public override IColumnEvent? DoDraw((string, Vector4)? currentValue, int rowIndex,
            FilterConfiguration filterConfiguration)
        {
            ImGui.TableNextColumn();
            if (currentValue.HasValue)
            {
                ImGui.TextColored( currentValue.Value.Item2, currentValue.Value.Item1);
            }
            else
            {
                ImGui.Text(EmptyText);
            }
            return null;
        }

        public override void Setup(int columnIndex)
        {
            ImGui.TableSetupColumn(Name, ImGuiTableColumnFlags.WidthFixed, Width, (uint)columnIndex);
        }
    }
}