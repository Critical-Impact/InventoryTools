using System;
using System.Collections.Generic;
using System.Linq;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using ImGuiNET;
using InventoryTools.Extensions;
using InventoryTools.Ui.Widgets;

namespace InventoryTools.Logic.Columns.Abstract
{
    public abstract class IntegerColumn : Column<int?>
    {
        public override string CsvExport(InventoryItem item)
        {
            return CurrentValue(item).ToString() ?? "";
        }

        public override string CsvExport(ItemEx item)
        {
            return CurrentValue((ItemEx)item).ToString() ?? "";
        }

        public override string CsvExport(SortingResult item)
        {
            return CurrentValue(item).ToString() ?? "";
        }

        public override string CsvExport(CraftItem item)
        {
            return CurrentValue(item).ToString() ?? "";
        }
        public override int? CurrentValue(CraftItem currentValue)
        {
            return CurrentValue(currentValue.Item);
        }
        
        public override int? CurrentValue(InventoryChange currentValue)
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
                return "";
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
                var currentValue = CurrentValue( c);
                if (currentValue == null)
                {
                    return false;
                }

                return currentValue.Value.PassesFilter(FilterText);
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

                return currentValue.Value.PassesFilter(FilterText);
            });
        }

        public override IEnumerable<SortingResult> Filter(IEnumerable<SortingResult> items)
        {
            return FilterText == "" ? items : items.Where(c =>
            {
                var currentValue = CurrentValue(c);
                if (currentValue == null)
                {
                    return false;
                }

                return currentValue.Value.PassesFilter(FilterText);
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

                return currentValue.Value.PassesFilter(FilterText);
            });
        }


        public override IEnumerable<InventoryItem> Sort(ImGuiSortDirection direction, IEnumerable<InventoryItem> items)
        {
            return direction == ImGuiSortDirection.Ascending ? items.OrderBy(c => CurrentValue(c) ?? Int32.MaxValue) : items.OrderByDescending(c => CurrentValue(c) ?? Int32.MinValue);
        }

        public override IEnumerable<ItemEx> Sort(ImGuiSortDirection direction, IEnumerable<ItemEx> items)
        {
            return direction == ImGuiSortDirection.Ascending ? items.OrderBy(c => CurrentValue(c) ?? Int32.MaxValue) : items.OrderByDescending(c => CurrentValue(c) ?? Int32.MinValue);
        }

        public override IEnumerable<SortingResult> Sort(ImGuiSortDirection direction, IEnumerable<SortingResult> items)
        {
            return direction == ImGuiSortDirection.Ascending ? items.OrderBy(c => CurrentValue(c) ?? Int32.MaxValue) : items.OrderByDescending(c => CurrentValue(c) ?? Int32.MinValue);
        }

        public override IEnumerable<InventoryChange> Sort(ImGuiSortDirection direction, IEnumerable<InventoryChange> items)
        {
            return direction == ImGuiSortDirection.Ascending ? items.OrderBy(c => CurrentValue(c.InventoryItem) ?? Int32.MaxValue) : items.OrderByDescending(c => CurrentValue(c.InventoryItem) ?? Int32.MinValue);
        }

        public override IColumnEvent? DoDraw(int? currentValue, int rowIndex, FilterConfiguration filterConfiguration)
        {
            ImGui.TableNextColumn();
            if (currentValue != null)
            {
                var fmt = $"{currentValue.Value:n0}";
                ImGuiUtil.VerticalAlignText(fmt, filterConfiguration.TableHeight, false);
            }
            else
            {
                ImGuiUtil.VerticalAlignText(EmptyText, filterConfiguration.TableHeight, false);
            }
            return null;
        }

        public override void Setup(int columnIndex)
        {
            ImGui.TableSetupColumn(RenderName ?? Name, ImGuiTableColumnFlags.WidthFixed, Width, (uint)columnIndex);
        }
    }
}