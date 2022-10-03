using System.Collections.Generic;
using System.Linq;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using ImGuiNET;
using InventoryTools.Extensions;

namespace InventoryTools.Logic.Columns.Abstract
{
    public abstract class DoubleIntegerColumn : Column<(int,int)?>
    {
        public override string CsvExport(InventoryItem item)
        {
            return (CurrentValue(item)?.Item1.ToString()  ?? "") + "/" + (CurrentValue(item)?.Item2.ToString() ?? "");
        }

        public override string CsvExport(ItemEx item)
        {
            return (CurrentValue((ItemEx)item)?.Item1.ToString()  ?? "") + "/" + (CurrentValue((ItemEx)item)?.Item2.ToString() ?? "");
        }

        public override string CsvExport(SortingResult item)
        {
            return (CurrentValue(item)?.Item1.ToString()  ?? "") + "/" + (CurrentValue(item)?.Item2.ToString() ?? "");
        }
        
        public override (int,int)? CurrentValue(CraftItem currentValue)
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
        
        public virtual string Divider => "/";

        public virtual string EmptyText => "";

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
            return FilterText == "" ? items : items.Where(c =>
            {
                var currentValue = CurrentValue( c);
                if (currentValue == null)
                {
                    return false;
                }

                return currentValue.Value.Item1.PassesFilter(FilterText) || currentValue.Value.Item2.PassesFilter(FilterText);
            });
        }

        public override IEnumerable<InventoryItem> Filter(IEnumerable<InventoryItem> items)
        {
            var isChecked = FilterText != "";
            return FilterText == "" ? items : items.Where(c =>
            {
                var currentValue = CurrentValue(c);
                if (currentValue == null)
                {
                    return false;
                }

                return currentValue.Value.Item1.PassesFilter(FilterText) || currentValue.Value.Item2.PassesFilter(FilterText);
            });
        }

        public override IEnumerable<SortingResult> Filter(IEnumerable<SortingResult> items)
        {
            var isChecked = FilterText != "";
            return FilterText == "" ? items : items.Where(c =>
            {
                var currentValue = CurrentValue(c);
                if (currentValue == null)
                {
                    return false;
                }

                return currentValue.Value.Item1.PassesFilter(FilterText) || currentValue.Value.Item2.PassesFilter(FilterText);
            });
        }

        public override IEnumerable<InventoryItem> Sort(ImGuiSortDirection direction, IEnumerable<InventoryItem> items)
        {
            return direction == ImGuiSortDirection.Ascending ? items.OrderBy(item =>
            {
                var currentValue = CurrentValue(item);
                if (currentValue == null)
                {
                    return 0;
                }

                return currentValue.Value.Item1;
            }) : items.OrderByDescending(item =>
            {
                var currentValue = CurrentValue(item);
                if (currentValue == null)
                {
                    return 0;
                }

                return currentValue.Value.Item1;
            });
        }

        public override IEnumerable<ItemEx> Sort(ImGuiSortDirection direction, IEnumerable<ItemEx> items)
        {
            return direction == ImGuiSortDirection.Ascending ? items.OrderBy(item =>
            {
                var currentValue = CurrentValue((ItemEx)item);
                if (currentValue == null)
                {
                    return 0;
                }

                return currentValue.Value.Item1;
            }) : items.OrderByDescending(item =>
            {
                var currentValue = CurrentValue((ItemEx)item);
                if (currentValue == null)
                {
                    return 0;
                }

                return currentValue.Value.Item1;
            });
        }

        public override IEnumerable<SortingResult> Sort(ImGuiSortDirection direction, IEnumerable<SortingResult> items)
        {
            return direction == ImGuiSortDirection.Ascending ? items.OrderBy(item =>
            {
                var currentValue = CurrentValue(item);
                if (currentValue == null)
                {
                    return 0;
                }

                return currentValue.Value.Item1;
            }) : items.OrderByDescending(item =>
            {
                var currentValue = CurrentValue(item);
                if (currentValue == null)
                {
                    return 0;
                }

                return currentValue.Value.Item1;
            });
        }

        public override IColumnEvent? DoDraw((int, int)? currentValue, int rowIndex,
            FilterConfiguration filterConfiguration)
        {
            ImGui.TableNextColumn();
            if (currentValue != null)
            {
                ImGui.Text($"{currentValue.Value.Item1:n0}" + Divider + $"{currentValue.Value.Item2:n0}");
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