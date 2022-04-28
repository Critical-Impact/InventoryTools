using System;
using System.Collections.Generic;
using System.Linq;
using CriticalCommonLib.Models;
using ImGuiNET;
using InventoryTools.Extensions;
using Lumina.Excel.GeneratedSheets;
using NaturalSort.Extension;

namespace InventoryTools.Logic.Columns
{
    public abstract class IntegerColumn : Column<int?>
    {
        public virtual string EmptyText
        {
            get
            {
                return "";
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

        public override IEnumerable<Item> Filter(IEnumerable<Item> items)
        {
            return FilterText == "" ? items : items.Where(c =>
            {
                var currentValue = CurrentValue((Item) c);
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

        public override IEnumerable<InventoryItem> Sort(ImGuiSortDirection direction, IEnumerable<InventoryItem> items)
        {
            return direction == ImGuiSortDirection.Ascending ? items.OrderBy<InventoryItem, string>(c => CurrentValue(c).ToString() ?? "", StringComparison.OrdinalIgnoreCase.WithNaturalSort()) : items.OrderByDescending(c => CurrentValue(c).ToString() ?? "", StringComparison.OrdinalIgnoreCase.WithNaturalSort());
        }

        public override IEnumerable<Item> Sort(ImGuiSortDirection direction, IEnumerable<Item> items)
        {
            return direction == ImGuiSortDirection.Ascending ? items.OrderBy<Item, string>(c => CurrentValue(c).ToString() ?? "", StringComparison.OrdinalIgnoreCase.WithNaturalSort()) : items.OrderByDescending(c => CurrentValue(c).ToString() ?? "", StringComparison.OrdinalIgnoreCase.WithNaturalSort());
        }

        public override IEnumerable<SortingResult> Sort(ImGuiSortDirection direction, IEnumerable<SortingResult> items)
        {
            return direction == ImGuiSortDirection.Ascending ? items.OrderBy<SortingResult, string>(c => CurrentValue(c).ToString() ?? "", StringComparison.OrdinalIgnoreCase.WithNaturalSort()) : items.OrderByDescending(c => CurrentValue(c).ToString() ?? "", StringComparison.OrdinalIgnoreCase.WithNaturalSort());
        }

        public override IColumnEvent? DoDraw(int? currentValue, int rowIndex)
        {
            ImGui.TableNextColumn();
            if (currentValue != null)
            {
                ImGui.Text(currentValue.Value.ToString());
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