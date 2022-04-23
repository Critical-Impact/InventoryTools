using System;
using System.Collections.Generic;
using System.Numerics;
using CriticalCommonLib.Models;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;

namespace InventoryTools.Logic.Columns
{
    public abstract class Column<T> : IColumn
    {
        public virtual FilterType AvailableIn => Logic.FilterType.SearchFilter | Logic.FilterType.SortingFilter |
                                                 Logic.FilterType.GameItemFilter;
        public abstract T CurrentValue(InventoryItem item);
        public abstract T CurrentValue(Item item);
        public abstract T CurrentValue(SortingResult item);
        public  abstract string Name { get; set; }
        public abstract float Width { get; set; }
        public abstract string FilterText { get; set; }
        public abstract bool HasFilter { get; set; }
        public abstract ColumnFilterType FilterType { get; set; }
        public abstract IEnumerable<InventoryItem> Filter(IEnumerable<InventoryItem> items);

        public abstract IEnumerable<SortingResult> Filter(IEnumerable<SortingResult> items);

        public abstract IEnumerable<Item> Filter(IEnumerable<Item> items);

        public abstract IEnumerable<InventoryItem> Sort(ImGuiSortDirection direction, IEnumerable<InventoryItem> items);

        public abstract IEnumerable<SortingResult> Sort(ImGuiSortDirection direction, IEnumerable<SortingResult> items);

        public abstract IEnumerable<Item> Sort(ImGuiSortDirection direction, IEnumerable<Item> items);

        public abstract void Draw(InventoryItem item, int rowIndex);

        public abstract void Draw(SortingResult item, int rowIndex);
        public abstract void Draw(Item item, int rowIndex);

        public abstract void DoDraw(T currentValue, int rowIndex);

        public abstract void Setup(int columnIndex);

        public virtual event IColumn.ButtonPressedDelegate? ButtonPressed
        {
            add { throw new NotSupportedException(); }
            remove { }
        }

        public virtual bool DrawFilter(string tableKey, int columnIndex)
        {
            var filter = FilterText;
            var hasChanged = false;

            ImGui.TableSetColumnIndex(columnIndex);
            ImGui.PushItemWidth(-20.000000f);
            ImGui.PushID(Name);
            ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(0, 0));
            ImGui.InputText("##" + tableKey + "FilterI" + Name, ref filter, 200);
            ImGui.PopStyleVar();
            ImGui.SameLine(0.0f, ImGui.GetStyle().ItemInnerSpacing.X);
            ImGui.TableHeader("");
            ImGui.PopID();
            if (filter != FilterText)
            {
                FilterText = filter;
                hasChanged = true;
            }

            return hasChanged;
        }
    }
}