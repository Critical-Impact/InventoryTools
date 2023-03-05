using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using CriticalCommonLib;
using CriticalCommonLib.Sheets;
using Dalamud.Utility;
using ImGuiNET;
using InventoryTools.Extensions;
using InventoryTools.Images;
using InventoryTools.Logic;
using Lumina.Excel;
using OtterGui.Raii;

namespace InventoryTools.Ui;

public abstract class GenericTabbedTable<T> : Window, IGenericTabbedTable<T>
{
    protected GenericTabbedTable(string name, ImGuiWindowFlags flags = ImGuiWindowFlags.None, bool forceMainWindow = false) : base(name, flags, forceMainWindow)
    {
    }
    

    private uint _currentTab = 0;
    
    public uint CurrentTab
    {
        get
        {
            return _currentTab;
        }
        set
        {
            if (_currentTab != value)
            {
                Items.Remove(value);
                _currentTab = value;
            }
        }
    }
    public void DrawTabs()
    {
        if (ImGui.BeginTabBar("###Tab" + TableName, ImGuiTabBarFlags.FittingPolicyScroll | ImGuiTabBarFlags.TabListPopupButton))
        {
            if (ImGui.BeginTabItem("All"))
            {
                CurrentTab = 0;
                ImGui.PushID(0.ToString());
                DrawTable(TableName + "Table",GetItems(0), TableFlags, Columns, 0);
                ImGui.PopID();
                ImGui.EndTabItem();
            }
            
            foreach(var tab in Tabs)
            {
                if (tab.Key != 0 && ImGui.BeginTabItem(tab.Value))
                {
                    CurrentTab = tab.Key;
                    ImGui.PushID(tab.Key.ToString());
                    DrawTable(tab.Key + "Table",GetItems(CurrentTab), TableFlags, Columns, CurrentTab);
                    ImGui.PopID();
                    ImGui.EndTabItem();
                }
            }
        }
    }

    private int? _sortColumn;
    private ImGuiSortDirection? _sortDirection;
    private uint _rowSize = 32;

    public void DrawTable(string label, IEnumerable<T> data, ImGuiTableFlags flags, List<TableColumn<T>> tableColumns,
        uint contentTypeId)
    {
        if (tableColumns.Count == 0)
            return;

        using var table = ImRaii.Table(label, tableColumns.Count, flags);
        if (!table)
            return;
        var refresh = false;
        ImGui.TableSetupScrollFreeze(0, 2);
        for (var index = 0; index < tableColumns.Count; index++)
        {
            var tableColumn = tableColumns[index];
            ImGui.TableSetupColumn(tableColumn.Name, tableColumn.ColumnFlags, tableColumn.Width, (uint)index + 1);
        }

        var currentSortSpecs = ImGui.TableGetSortSpecs();
        if (currentSortSpecs.SpecsDirty)
        {
            var actualSpecs = currentSortSpecs.Specs;
            if (_sortColumn != actualSpecs.ColumnIndex)
            {
                _sortColumn = actualSpecs.ColumnIndex;
                refresh = true;
            }

            if (_sortDirection != actualSpecs.SortDirection)
            {
                _sortDirection = actualSpecs.SortDirection;
                refresh = true;
            }
        }
        else
        {
            if (_sortColumn != null)
            {
                _sortColumn = null;
                refresh = true;
            }

            if (_sortDirection != null)
            {
                _sortDirection = null;
                refresh = true;
            }
        }

        ImGui.TableHeadersRow();

        ImGui.TableNextRow(ImGuiTableRowFlags.Headers);

        for (var index = 0; index < tableColumns.Count; index++)
        {
            var name = ImGui.TableGetColumnName(index);
            var column = tableColumns[index];
            if (column.Filter != null)
            {
                var filter = column.FilterText;

                ImGui.TableSetColumnIndex(index);
                ImGui.PushItemWidth(-20.000000f);
                ImGui.PushID(column.Name);
                ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(0, 0));
                ImGui.InputText("##SearchFilter" + column.Name, ref filter, 200);
                ImGui.PopStyleVar();
                ImGui.SameLine(0.0f, ImGui.GetStyle().ItemInnerSpacing.X);
                ImGui.TableHeader("");
                ImGui.PopID();
                ImGui.PopItemWidth();
                if (filter != column.FilterText)
                {
                    column.FilterText = filter;
                    refresh = true;
                }
            }

            if (column.FilterBool != null)
            {
                ImGui.TableSetColumnIndex(index);
                ImGui.PushID(column.Name);
                ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(0, 0));
                var isChecked = column.FilterBoolean;
                var checkboxUnChecked = isChecked.HasValue
                    ? (isChecked.Value ? GameIcon.CheckboxChecked : GameIcon.CheckboxUnChecked)
                    : GameIcon.CheckboxUnChecked;
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + (ImGui.GetContentRegionAvail().X / 2) -
                                    checkboxUnChecked.Size.X / 2);
                ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 2);
                if (isChecked == null)
                {
                    ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0, 0, 0, 0.1f));
                }

                if (PluginService.PluginLogic.DrawUldIconButton(checkboxUnChecked))
                {
                    if (!isChecked.HasValue)
                    {
                        column.FilterBoolean = false;
                    }
                    else if (isChecked.Value == false)
                    {
                        column.FilterBoolean = true;
                    }
                    else
                    {
                        column.FilterBoolean = null;
                    }

                    refresh = true;
                }

                if (isChecked == null)
                {
                    ImGui.PopStyleColor();
                }

                ImGui.PopStyleVar();
                ImGui.SameLine(0.0f, ImGui.GetStyle().ItemInnerSpacing.X);
                ImGui.TableHeader("");
                ImGui.PopID();

            }
        }

        var items = data.ToList();
        ImGuiListClipperPtr clipper;
        unsafe
        {
            clipper = new ImGuiListClipperPtr(ImGuiNative.ImGuiListClipper_ImGuiListClipper());
            clipper.ItemsHeight = RowSize;
        }

        clipper.Begin(items.Count);
        while (clipper.Step())
        {
            for (var index = clipper.DisplayStart; index < clipper.DisplayEnd; index++)
            {
                var ex = items[index];
                ImGui.TableNextRow(ImGuiTableRowFlags.None, RowSize);
                ImGui.PushID(GetRowId(ex));

                for (var i = 0; i < tableColumns.Count; i++)
                {
                    ImGui.PushID(i);
                    var column = tableColumns[i];
                    ImGui.TableNextColumn();
                    column.Draw(ex, contentTypeId);
                    ImGui.PopID();
                }

                ImGui.PopID();
            }

            if (refresh)
            {
                Items.Remove(contentTypeId);
                FilteredItems.Remove(contentTypeId);
            }
        }
        
        clipper.End();
        clipper.Destroy();
    }

    public abstract int GetRowId(T item);

    public abstract Dictionary<uint, List<T>> Items { get; }
    public abstract Dictionary<uint, List<T>> FilteredItems { get; }
    public abstract List<TableColumn<T>> Columns { get; }
    public abstract ImGuiTableFlags TableFlags { get; }
    public abstract List<T> GetItems(uint tabId);
    public abstract Dictionary<uint, string> Tabs { get; }

    public abstract string TableName { get; }

    public int? SortColumn => _sortColumn;

    public ImGuiSortDirection? SortDirection => _sortDirection;
    
    public abstract bool UseClipper { get; }

    public float RowSize => _rowSize;
}