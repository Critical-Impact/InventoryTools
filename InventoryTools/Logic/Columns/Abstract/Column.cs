using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection.Emit;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Models;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;

namespace InventoryTools.Logic.Columns.Abstract
{
    public abstract class Column<T> : IColumn
    {
        public virtual uint MaxFilterLength { get; set; } = 200;

        public virtual FilterType AvailableIn => Logic.FilterType.SearchFilter | Logic.FilterType.SortingFilter |
                                                 Logic.FilterType.GameItemFilter | Logic.FilterType.CraftFilter;
        public abstract T CurrentValue(InventoryItem item);
        public abstract T CurrentValue(Item item);
        public abstract T CurrentValue(SortingResult item);
        public abstract T CurrentValue(CraftItem item);
        public abstract string CsvExport(InventoryItem item);
        public abstract string CsvExport(Item item);
        public abstract string CsvExport(SortingResult item);

        public virtual string CsvExport(CraftItem item)
        {
            return item.Item != null ? CsvExport(item.Item) : "";
        }
        
        public  abstract string Name { get; set; }
        public abstract float Width { get; set; }
        public abstract string FilterText { get; set; }
        public virtual List<string>? FilterChoices { get; set; } = null;
        public abstract bool HasFilter { get; set; }
        public abstract ColumnFilterType FilterType { get; set; }
        
        public virtual bool IsDebug { get; set; } = false;
        public abstract IEnumerable<InventoryItem> Filter(IEnumerable<InventoryItem> items);

        public abstract IEnumerable<SortingResult> Filter(IEnumerable<SortingResult> items);

        public abstract IEnumerable<Item> Filter(IEnumerable<Item> items);

        public abstract IEnumerable<CraftItem> Filter(IEnumerable<CraftItem> items);

        public abstract IEnumerable<InventoryItem> Sort(ImGuiSortDirection direction, IEnumerable<InventoryItem> items);

        public abstract IEnumerable<SortingResult> Sort(ImGuiSortDirection direction, IEnumerable<SortingResult> items);

        public abstract IEnumerable<Item> Sort(ImGuiSortDirection direction, IEnumerable<Item> items);
        public abstract IEnumerable<CraftItem> Sort(ImGuiSortDirection direction, IEnumerable<CraftItem> items);

        public abstract void Draw(InventoryItem item, int rowIndex);

        public abstract void Draw(SortingResult item, int rowIndex);
        public abstract void Draw(Item item, int rowIndex);

        public abstract void Draw(CraftItem item, int rowIndex, FilterConfiguration configuration);

        public abstract IColumnEvent? DoDraw(T currentValue, int rowIndex);

        public abstract void Setup(int columnIndex);

        public virtual event IColumn.ButtonPressedDelegate? ButtonPressed
        {
            add { throw new NotSupportedException(); }
            remove { }
        }

        public virtual bool DrawFilter(string tableKey, int columnIndex)
        {
            if (FilterType == ColumnFilterType.Text)
            {
                var filter = FilterText;
                var hasChanged = false;

                ImGui.TableSetColumnIndex(columnIndex);
                ImGui.PushItemWidth(-20.000000f);
                ImGui.PushID(Name);
                ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(0, 0));
                ImGui.InputText("##" + tableKey + "FilterI" + Name, ref filter, MaxFilterLength);
                ImGui.PopStyleVar();
                ImGui.SameLine(0.0f, ImGui.GetStyle().ItemInnerSpacing.X);
                ImGui.TableHeader("");
                ImGui.PopID();
                ImGui.PopItemWidth();
                if (filter != FilterText)
                {
                    FilterText = filter;
                    hasChanged = true;
                }

                return hasChanged;
            }
            else if (FilterType == ColumnFilterType.Choice)
            {
                var hasChanged = false;
                ImGui.TableSetColumnIndex(columnIndex);
                ImGui.PushItemWidth(-20.000000f);
                ImGui.PushID(Name);
                ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(0, 0));

                var currentItem = FilterText;
                
                if (ImGui.BeginCombo("##Choice", currentItem))
                {
                    if (FilterChoices != null)
                    {
                        if (ImGui.Selectable("", false))
                        {
                            FilterText = "";
                            hasChanged = true;
                        }
                        foreach (var column in FilterChoices)
                        {
                            if (ImGui.Selectable(column, currentItem == column))
                            {
                                FilterText = column;
                                hasChanged = true;
                            }
                        }
                    }

                    ImGui.EndCombo();
                }
                ImGui.PopStyleVar();
                ImGui.SameLine(0.0f, ImGui.GetStyle().ItemInnerSpacing.X);
                ImGui.TableHeader("");
                ImGui.PopID();
                ImGui.PopItemWidth();
                return hasChanged;
            }

            return false;
        }
    }
}