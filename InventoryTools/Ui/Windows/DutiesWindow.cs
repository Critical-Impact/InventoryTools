using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using CriticalCommonLib;
using CriticalCommonLib.Extensions;
using CriticalCommonLib.Sheets;
using Dalamud.Utility;
using ImGuiNET;
using InventoryTools.Logic;
using Lumina.Excel.GeneratedSheets;
using OtterGui.Raii;

namespace InventoryTools.Ui;

public class DutiesWindow : Window
{
    private List<ContentType> _contentTypes;
    private Dictionary<uint, List<ContentFinderConditionEx>> _duties;
    private Dictionary<uint, List<ContentFinderConditionEx>> _filteredDuties;
    public DutiesWindow(string name = "Allagan Tools - Duties", ImGuiWindowFlags flags = ImGuiWindowFlags.None, bool forceMainWindow = false) : base(name, flags, forceMainWindow)
    {
        SetupWindow();
    }

    public DutiesWindow() : base("Allagan Tools - Duties")
    {
        SetupWindow();
    }

    public void SetupWindow()
    {
        _columns = new List<TableColumn<ContentFinderConditionEx>>()
        {
            new("Icon", 32, ImGuiTableColumnFlags.WidthFixed)
            {
                OnLeftClick = OnLeftClick
            },
            new("Name", 200, ImGuiTableColumnFlags.WidthFixed)
            {
                Sort = (specs, exes) =>
                {
                    if (specs == null)
                    {
                        return exes;
                    }

                    return specs == ImGuiSortDirection.Ascending ? exes.OrderBy(c => c.FormattedName) : exes.OrderByDescending(c => c.FormattedName);
                }
            }
        };
        _contentTypes = Service.ExcelCache.GetContentTypeSheet().Where(c => c.Name.ToDalamudString().ToString() != "" && c.IconDutyFinder != 0).ToList();
        _duties = new Dictionary<uint, List<ContentFinderConditionEx>>();
        _filteredDuties = new Dictionary<uint, List<ContentFinderConditionEx>>();
    }

    private bool OnLeftClick(ContentFinderConditionEx arg)
    {
        PluginService.WindowService.OpenDutyWindow(arg.RowId);
        return true;
    }

    private List<ContentFinderConditionEx> GetDuties(uint contentTypeId)
    {
        if (!_duties.ContainsKey(contentTypeId))
        {
            if (contentTypeId == 0)
            {
                var duties = Service.ExcelCache.GetContentFinderConditionExSheet().Where(c => c.Name.ToDalamudString().ToString() != "").ToList();
                _duties.Add(contentTypeId, duties);
            }
            else
            {
                var duties = Service.ExcelCache.GetContentFinderConditionExSheet().Where(c => c.Name.ToDalamudString().ToString() != "" && c.ContentType.Row == contentTypeId).ToList();
                _duties.Add(contentTypeId, duties);
            }
        }

        if (!_filteredDuties.ContainsKey(contentTypeId))
        {
            var unfilteredList = _duties[contentTypeId];
            if (_sortColumn != null && _columns[(int)_sortColumn].Sort != null)
            {
                unfilteredList = _columns[(int)_sortColumn].Sort?.Invoke(_sortDirection, unfilteredList).ToList();
            }

            _filteredDuties.Add(contentTypeId, unfilteredList);
        }

        return _filteredDuties[contentTypeId];
    }

    public static string AsKey
    {
        get { return "duties"; }
    }
    
    public override string Key => AsKey;
    public override bool DestroyOnClose => false;
    public override bool SaveState => true;
    public override Vector2 MaxSize { get; } = new(2000, 2000);
    public override Vector2 MinSize { get; } = new(200, 200);
    public override Vector2 DefaultSize { get; } = new(600, 600);

    public ImGuiTableFlags _tableFlags = ImGuiTableFlags.RowBg | ImGuiTableFlags.BordersV |
                                         ImGuiTableFlags.BordersOuterV | ImGuiTableFlags.BordersInnerV |
                                         ImGuiTableFlags.BordersH | ImGuiTableFlags.BordersOuterH |
                                         ImGuiTableFlags.BordersInnerH |
                                         ImGuiTableFlags.Resizable | ImGuiTableFlags.Sortable |
                                         ImGuiTableFlags.Hideable | ImGuiTableFlags.ScrollX |
                                         ImGuiTableFlags.ScrollY;

    public override void Draw()
    {
        if (ImGui.BeginTabBar("###DutyTabs", ImGuiTabBarFlags.FittingPolicyScroll))
        {
            ImGui.PushID(0.ToString());
            if (ImGui.BeginTabItem("All"))
            {
                DrawTable("contentTable",GetDuties(0), DrawRow, _tableFlags, _columns, 0);
                ImGui.EndTabItem();
            }
            ImGui.PopID();
            
            for (var index = 0; index < _contentTypes.Count; index++)
            {
                var contentType = _contentTypes[index];
                ImGui.PushID(contentType.RowId.ToString());
                if (ImGui.BeginTabItem(contentType.Name))
                {
                    DrawTable("contentTable",GetDuties(contentType.RowId), DrawRow, _tableFlags, _columns, contentType.RowId);
                    ImGui.EndTabItem();
                }
                ImGui.PopID();
            }
        }
    }

    private List<TableColumn<ContentFinderConditionEx>> _columns;
    private int? _sortColumn;
    private ImGuiSortDirection? _sortDirection;
    
    public void DrawTable(string label, IEnumerable<ContentFinderConditionEx> data, Action<ContentFinderConditionEx> drawRow, ImGuiTableFlags flags, List<TableColumn<ContentFinderConditionEx>> tableColumns, uint contentTypeId)
    {
        if (tableColumns.Count == 0)
            return;

        using var table = ImRaii.Table(label, tableColumns.Count, flags);
        if (!table)
            return;
        var refresh = false;
        ImGui.TableSetupScrollFreeze(0,1);
        foreach (var tableColumn in tableColumns)
        {
            ImGui.TableSetupColumn(tableColumn.Name, tableColumn.ColumnFlags, tableColumn.Width);
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
        }
        ImGui.TableHeadersRow();

        foreach (var datum in data)
        {
            ImGui.TableNextRow(ImGuiTableRowFlags.None, 32);
            drawRow(datum);
        }

        if (refresh)
        {
            _filteredDuties.Remove(contentTypeId);
        }
    }


    private void DrawRow(ContentFinderConditionEx obj)
    {
        ImGui.PushID(obj.RowId.ToString());
        ImGui.TableNextColumn();
        if (ImGui.ImageButton(PluginService.IconStorage[(int)obj.ContentType.Value!.IconDutyFinder].ImGuiHandle,
                new Vector2(32, 32)))
        {
            _columns[0].OnLeftClick?.Invoke(obj);
        }
        ImGui.TableNextColumn();
        ImGui.Text(obj.FormattedName);
        ImGui.PopID();

    }

    public override void Invalidate()
    {
    }

    public override FilterConfiguration? SelectedConfiguration => null;
}