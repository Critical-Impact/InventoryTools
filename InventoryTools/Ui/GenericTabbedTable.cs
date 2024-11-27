using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using CriticalCommonLib.Services.Mediator;

using ImGuiNET;
using Dalamud.Interface.Utility.Raii;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Ui;

public abstract class GenericTabbedTable<T> : GenericWindow, IGenericTabbedTable<T>
{
    private readonly ImGuiService _imGuiService;

    protected GenericTabbedTable(ILogger logger, MediatorService mediator, ImGuiService imGuiService, InventoryToolsConfiguration configuration, string name) : base(logger, mediator, imGuiService, configuration, name)
    {
        _imGuiService = imGuiService;
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
        using (var tabBar = ImRaii.TabBar("###Tab" + TableName, ImGuiTabBarFlags.FittingPolicyScroll | ImGuiTabBarFlags.TabListPopupButton))
        {
            if (tabBar.Success)
            {
                using (var tabItem = ImRaii.TabItem("All"))
                {
                    if (tabItem.Success)
                    {
                        CurrentTab = 0;
                        ImGui.PushID(0.ToString());
                        DrawTable(TableName + "Table", GetItems(0), TableFlags, Columns, 0);
                        ImGui.PopID();
                    }
                }

                foreach (var tab in Tabs)
                {
                    if (tab.Key != 0)
                    {
                        using (var tabItem = ImRaii.TabItem(tab.Value))
                        {
                            if (tabItem.Success)
                            {
                                CurrentTab = tab.Key;
                                ImGui.PushID(tab.Key.ToString());
                                DrawTable(tab.Key + "Table", GetItems(CurrentTab), TableFlags, Columns, CurrentTab);
                                ImGui.PopID();
                            }
                        }
                    }
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

        using var table = ImRaii.Table(label, tableColumns.Count(c => (contentTypeId == 0 && c.AllTabOnly) || !c.AllTabOnly), flags);
        if (!table || !table.Success)
            return;
        var refresh = false;
        ImGui.TableSetupScrollFreeze(0, 2);
        var index = 0;
        for (var tableColumnIndex = 0; tableColumnIndex < tableColumns.Count; tableColumnIndex++)
        {
            var tableColumn = tableColumns[tableColumnIndex];
            if (tableColumn.AllTabOnly && contentTypeId != 0)
            {
                continue;
            }
            ImGui.TableSetupColumn(tableColumn.Name, tableColumn.ColumnFlags, tableColumn.Width, (uint)tableColumnIndex + 1);
            index++;
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
        index = 0;
        for (var columnIndex = 0; columnIndex < tableColumns.Count; columnIndex++)
        {
            var name = ImGui.TableGetColumnName(index);
            var column = tableColumns[columnIndex];
            if (column.AllTabOnly && contentTypeId != 0)
            {
                continue;
            }
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
                    ? (isChecked.Value ? _imGuiService.CheckboxChecked : _imGuiService.CheckboxUnChecked)
                    : _imGuiService.CheckboxUnChecked;
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + (ImGui.GetContentRegionAvail().X / 2) -
                                    checkboxUnChecked.Size.X / 2);
                ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 2);
                if (isChecked == null)
                {
                    ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0, 0, 0, 0.1f));
                }

                if (_imGuiService.DrawUldIconButton(checkboxUnChecked))
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

            index++;
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
            for (var clipperIndex = clipper.DisplayStart; clipperIndex < clipper.DisplayEnd; clipperIndex++)
            {
                var ex = items[clipperIndex];
                ImGui.TableNextRow(ImGuiTableRowFlags.None, RowSize);
                ImGui.PushID(GetRowId(ex));

                var columnIndex = 0;
                for (var i = 0; i < tableColumns.Count; i++)
                {
                    var column = tableColumns[i];
                    if (column.AllTabOnly && contentTypeId != 0)
                    {
                        continue;
                    }
                    ImGui.PushID(columnIndex);
                    ImGui.TableNextColumn();
                    column.Draw?.Invoke(ex, contentTypeId);

                    ImGui.PopID();
                    columnIndex++;
                }

                ImGui.PopID();
            }
        }
        
        clipper.End();
        clipper.Destroy();
        
        if (refresh)
        {
            Items.Remove(contentTypeId);
            FilteredItems.Remove(contentTypeId);
        }
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