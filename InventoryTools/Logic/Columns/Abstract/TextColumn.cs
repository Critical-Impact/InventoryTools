using System;
using System.Collections.Generic;
using System.Linq;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using ImGuiNET;
using InventoryTools.Extensions;
using NaturalSort.Extension;

namespace InventoryTools.Logic.Columns.Abstract
{
    public abstract class TextColumn : Column<string?>
    {
        public override string CsvExport(InventoryItem item)
        {
            return CurrentValue(item) ?? "";
        }

        public override string CsvExport(ItemEx item)
        {
            return CurrentValue((ItemEx)item) ?? "";
        }

        public override string CsvExport(SortingResult item)
        {
            return CurrentValue(item) ?? "";
        }
        public override string? CurrentValue(CraftItem currentValue)
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
        public override void Draw(InventoryItem item, int rowIndex)
        {
            DoDraw(CurrentValue(item), rowIndex);
        }
        public override void Draw(SortingResult item, int rowIndex)
        {
            DoDraw(CurrentValue(item), rowIndex);
        }
        public override void Draw(ItemEx item, int rowIndex)
        {
            DoDraw(CurrentValue((ItemEx)item), rowIndex);
        }
        public override void Draw(CraftItem item, int rowIndex, FilterConfiguration configuration)
        {
            DoDraw(CurrentValue(item), rowIndex);
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

                if (FilterType == ColumnFilterType.Choice)
                {
                    return currentValue == FilterText;
                }
                return currentValue.ToLower().PassesFilter(FilterText.ToLower());
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
                if (FilterType == ColumnFilterType.Choice)
                {
                    return currentValue == FilterText;
                }
                return currentValue.ToLower().PassesFilter(FilterText.ToLower());
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
                if (FilterType == ColumnFilterType.Choice)
                {
                    return currentValue == FilterText;
                }
                return currentValue.ToLower().PassesFilter(FilterText.ToLower());
            });
        }

        public override IEnumerable<InventoryItem> Sort(ImGuiSortDirection direction, IEnumerable<InventoryItem> items)
        {
            return direction == ImGuiSortDirection.Ascending ? items.OrderBy(CurrentValue, StringComparison.OrdinalIgnoreCase.WithNaturalSort()) : items.OrderByDescending(CurrentValue, StringComparison.OrdinalIgnoreCase.WithNaturalSort());
        }

        public override IEnumerable<ItemEx> Sort(ImGuiSortDirection direction, IEnumerable<ItemEx> items)
        {
            return direction == ImGuiSortDirection.Ascending ? items.OrderBy(item => CurrentValue((ItemEx)item), StringComparison.OrdinalIgnoreCase.WithNaturalSort()) : items.OrderByDescending(item => CurrentValue((ItemEx)item), StringComparison.OrdinalIgnoreCase.WithNaturalSort());
        }

        public override IEnumerable<SortingResult> Sort(ImGuiSortDirection direction, IEnumerable<SortingResult> items)
        {
            return direction == ImGuiSortDirection.Ascending ? items.OrderBy(CurrentValue, StringComparison.OrdinalIgnoreCase.WithNaturalSort()) : items.OrderByDescending(CurrentValue, StringComparison.OrdinalIgnoreCase.WithNaturalSort());
        }

        public override IColumnEvent? DoDraw(string? currentValue, int rowIndex)
        {
            ImGui.TableNextColumn();
            if (currentValue != null)
            {
                ImGui.Text(currentValue);
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