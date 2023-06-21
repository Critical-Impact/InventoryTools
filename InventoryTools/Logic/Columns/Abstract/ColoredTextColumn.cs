using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using ImGuiNET;
using InventoryTools.Extensions;
using InventoryTools.Ui.Widgets;
using OtterGui.Raii;

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

        public override dynamic? JsonExport(InventoryItem item)
        {
            return CurrentValue(item)?.Item1 ?? "";
        }

        public override dynamic? JsonExport(ItemEx item)
        {
            return CurrentValue(item)?.Item1 ?? "";
        }

        public override dynamic? JsonExport(SortingResult item)
        {
            return CurrentValue(item)?.Item1 ?? "";
        }

        public override dynamic? JsonExport(CraftItem item)
        {
            return CurrentValue(item)?.Item1 ?? "";
        }

        public override (string, Vector4)? CurrentValue(CraftItem currentValue)
        {
            return CurrentValue(currentValue.Item);
        }
        
        public override (string, Vector4)? CurrentValue(InventoryChange currentValue)
        {
            return CurrentValue(currentValue.InventoryItem);
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
        public override void Draw(FilterConfiguration configuration, InventoryChange item, int rowIndex)
        {
            DoDraw(CurrentValue(item), rowIndex, configuration);
        }
        public override IEnumerable<ItemEx> Filter(IEnumerable<ItemEx> items)
        {
            return FilterText == "" ? items : items.Where(c =>
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
            return FilterText == "" ? items : items.Where(c =>
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
            return FilterText == "" ? items : items.Where(c =>
            {
                var currentValue = CurrentValue(c);
                if (currentValue == null)
                {
                    return false;
                }

                return currentValue.Value.Item1.ToLower().PassesFilter(FilterComparisonText);
            });
        }
        
        public override IEnumerable<InventoryChange> Filter(IEnumerable<InventoryChange> items)
        {
            var isChecked = FilterText != "";
            return FilterText == "" ? items : items.Where(c =>
            {
                var currentValue = CurrentValue(c.InventoryItem);
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
        
        public override IEnumerable<InventoryChange> Sort(ImGuiSortDirection direction, IEnumerable<InventoryChange> items)
        {
            return direction == ImGuiSortDirection.Ascending ? items.OrderBy(item =>
            {
                var currentValue = CurrentValue(item.InventoryItem);
                return !currentValue.HasValue ? "" : currentValue.Value.Item1;
            }) : items.OrderByDescending(item =>
            {
                var currentValue = CurrentValue(item.InventoryItem);
                return !currentValue.HasValue ? "" : currentValue.Value.Item1;
            });
        }

        public override IColumnEvent? DoDraw((string, Vector4)? currentValue, int rowIndex,
            FilterConfiguration filterConfiguration)
        {
            ImGui.TableNextColumn();
            if (currentValue.HasValue)
            {
                if (filterConfiguration.FilterType == Logic.FilterType.CraftFilter)
                {
                    ImGuiUtil.VerticalAlignTextColored(currentValue.Value.Item1, currentValue.Value.Item2, filterConfiguration.TableHeight, true);
                }
                else
                {
                    ImGuiUtil.VerticalAlignTextColored(currentValue.Value.Item1, currentValue.Value.Item2, filterConfiguration.TableHeight, false);
                }
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