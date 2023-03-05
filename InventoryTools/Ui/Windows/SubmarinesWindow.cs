using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using CriticalCommonLib;
using CriticalCommonLib.Sheets;
using Dalamud.Utility;
using ImGuiNET;
using InventoryTools.Extensions;
using InventoryTools.Logic;
using InventoryTools.Ui.Widgets;
using ImGuiUtil = OtterGui.ImGuiUtil;

namespace InventoryTools.Ui;

public class SubmarinesWindow : GenericTabbedTable<SubmarineExplorationEx>
{
    public SubmarinesWindow(string name = "Allagan Tools - Submarines", ImGuiWindowFlags flags = ImGuiWindowFlags.None, bool forceMainWindow = false) : base(name, flags, forceMainWindow)
    {
        SetupWindow();
    }

    public SubmarinesWindow() : base("Allagan Tools - Submarines")
    {
        SetupWindow();
    }

    private void SetupWindow()
    {
        _columns = new List<TableColumn<SubmarineExplorationEx>>()
        {
            new("Icon", 32, ImGuiTableColumnFlags.WidthFixed)
            {
                OnLeftClick = OnLeftClick,
                Draw = (ex, contentTypeId) =>
                {
                    if (ImGui.ImageButton(PluginService.IconStorage[65035].ImGuiHandle,
                            new Vector2(RowSize, RowSize)))
                    {
                        _columns[0].OnLeftClick?.Invoke(ex);
                    }
                }
            },
            new("Name", 200, ImGuiTableColumnFlags.WidthFixed)
            {
                Sort = (specs, exes) =>
                {
                    if (specs == null)
                    {
                        return exes;
                    }

                    return specs == ImGuiSortDirection.Ascending ? exes.OrderBy(c => c.FormattedNameShort) : exes.OrderByDescending(c => c.FormattedNameShort);
                },
                Filter = (s, exes) =>
                {
                    if (s == null)
                    {
                        return exes;
                    }
                    return s == "" ? exes : exes.Where(c => c.FormattedNameShort.ToLower().PassesFilter(s.ToLower()));
                },
                Draw = (ex, tabId) =>
                {
                    ImGui.Text(ex.FormattedNameShort.ToString());
                }
            },
            new("Unlock Zone", 200, ImGuiTableColumnFlags.WidthFixed)
            {
                Sort = (specs, exes) =>
                {
                    if (specs == null)
                    {
                        return exes;
                    }

                    return specs == ImGuiSortDirection.Ascending ? exes.OrderBy(c => c.UnlockPointEx.Value?.FormattedNameShort ?? "") : exes.OrderByDescending(c => c.UnlockPointEx.Value?.FormattedNameShort ?? "");
                },
                Filter = (s, exes) =>
                {
                    if (s == null)
                    {
                        return exes;
                    }
                    return s == "" ? exes : exes.Where(c => (c.UnlockPointEx.Value?.FormattedNameShort ?? "").ToLower().PassesFilter(s.ToLower()));
                },
                Draw = (ex, tabId) =>
                {
                    if (ex.UnlockPointEx.Row != 0)
                    {
                        ImGui.Text((ex.UnlockPointEx.Value?.FormattedNameShort ?? "").ToString());
                    }
                }
            },
            new("Rank Required", 100, ImGuiTableColumnFlags.WidthFixed)
            {
                Sort = (specs, exes) =>
                {
                    if (specs == null)
                    {
                        return exes;
                    }

                    return specs == ImGuiSortDirection.Ascending ? exes.OrderBy(c => c.RankReq) : exes.OrderByDescending(c => c.RankReq);
                },
                Filter = (s, exes) =>
                {
                    if (s == null)
                    {
                        return exes;
                    }
                    return s == "" ? exes : exes.Where(c => (c.RankReq.ToString() ?? "").ToLower().PassesFilter(s.ToLower()));
                },
                Draw = (ex, tabId) =>
                {
                    if (ex.UnlockPointEx.Row != 0)
                    {
                        ImGui.Text((ex.RankReq.ToString() ?? "").ToString());
                    }
                }
            },
            new("Drops", 200, ImGuiTableColumnFlags.WidthFixed | ImGuiTableColumnFlags.NoSort)
            {
                Sort = (specs, exes) =>
                {
                    return exes;
                },
                Draw = (ex, contentTypeId) =>
                {
                    var drops = ex.Drops;
                    ImGui.BeginChild("DropScroll", new Vector2(ImGui.GetColumnWidth(), RowSize + ImGui.GetStyle().CellPadding.Y) * ImGui.GetIO().FontGlobalScale, false);
                    var maxItems = (int)Math.Floor(ImGui.GetColumnWidth() / RowSize);
                    maxItems = maxItems == 0 ? 1 : maxItems;
                    for (var index = 0; index < drops.Count; index++)
                    {
                        var drop = drops[index];
                        if (drop.Value != null)
                        {
                            var sourceIcon = PluginService.IconStorage[drop.Value.Icon];
                            ImGui.Image(sourceIcon.ImGuiHandle,
                                new Vector2(RowSize, RowSize) * ImGui.GetIO().FontGlobalScale);
                            if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled &
                                                    ImGuiHoveredFlags.AllowWhenOverlapped &
                                                    ImGuiHoveredFlags.AllowWhenBlockedByPopup &
                                                    ImGuiHoveredFlags.AllowWhenBlockedByActiveItem &
                                                    ImGuiHoveredFlags.AnyWindow) &&
                                ImGui.IsMouseReleased(ImGuiMouseButton.Right))
                            {
                                ImGui.OpenPopup("RightClick" + drop.Value.RowId);
                            }

                            if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled &
                                                    ImGuiHoveredFlags.AllowWhenOverlapped &
                                                    ImGuiHoveredFlags.AllowWhenBlockedByPopup &
                                                    ImGuiHoveredFlags.AllowWhenBlockedByActiveItem &
                                                    ImGuiHoveredFlags.AnyWindow) &&
                                ImGui.IsMouseReleased(ImGuiMouseButton.Left))
                            {
                                PluginService.WindowService.OpenItemWindow(drop.Value.RowId);
                            }

                            if (ImGui.BeginPopup("RightClick" + drop.Value.RowId))
                            {
                                drop.Value.DrawRightClickPopup();
                                ImGui.EndPopup();
                            }


                            ImGuiUtil.HoverTooltip(drop.Value.NameString);
                            if ((index + 1) % maxItems != 0)
                            {
                                ImGui.SameLine();
                            }
                        }
                    }
                    ImGui.EndChild();
                }
            },
        };
        _tabs = Service.ExcelCache.GetSubmarineMapSheet().Where(c => c.Name.ToDalamudString().ToString() != "").ToDictionary(c => c.RowId, c =>c.Name.ToString());
        _items = new Dictionary<uint, List<SubmarineExplorationEx>>();
        _filteredItems = new Dictionary<uint, List<SubmarineExplorationEx>>();        
    }
    
    private bool OnLeftClick(SubmarineExplorationEx arg)
    {
        PluginService.WindowService.OpenSubmarineWindow(arg.RowId);
        return true;
    }

    public override string Key => AsKey;
    public override bool DestroyOnClose => false;
    public override bool SaveState => true;
    public override Vector2 MaxSize { get; } = new(2000, 2000);
    public override Vector2 MinSize { get; } = new(200, 200);
    public override Vector2 DefaultSize { get; } = new(600, 600);

    public override void Draw()
    {
        DrawTabs();
    }
    
    public static string AsKey
    {
        get { return "submarines"; }
    }

    public override void Invalidate()
    {
    }

    public override FilterConfiguration? SelectedConfiguration { get; } = null;
    public override int GetRowId(SubmarineExplorationEx item)
    {
        return (int)item.RowId;
    }

    public override Dictionary<uint, List<SubmarineExplorationEx>> Items => _items;

    public override Dictionary<uint, List<SubmarineExplorationEx>> FilteredItems => _filteredItems;

    public override List<TableColumn<SubmarineExplorationEx>> Columns => _columns;
    
    private List<TableColumn<SubmarineExplorationEx>> _columns;
    private Dictionary<uint, List<SubmarineExplorationEx>> _items;
    private Dictionary<uint, List<SubmarineExplorationEx>> _filteredItems;
    private Dictionary<uint, string> _tabs;

    public override ImGuiTableFlags TableFlags => _flags;
    
    private ImGuiTableFlags _flags = ImGuiTableFlags.RowBg | ImGuiTableFlags.BordersV |
                                     ImGuiTableFlags.BordersOuterV | ImGuiTableFlags.BordersInnerV |
                                     ImGuiTableFlags.BordersH | ImGuiTableFlags.BordersOuterH |
                                     ImGuiTableFlags.BordersInnerH |
                                     ImGuiTableFlags.Resizable | ImGuiTableFlags.Sortable |
                                     ImGuiTableFlags.Hideable | ImGuiTableFlags.ScrollX |
                                     ImGuiTableFlags.ScrollY;
    public override List<SubmarineExplorationEx> GetItems(uint tabId)
    {
        if (!_items.ContainsKey(tabId))
        {
            if (tabId == 0)
            {
                var duties = Service.ExcelCache.GetSubmarineExplorationExSheet().Where(c => c.FormattedName.ToString() != "").ToList();
                _items.Add(tabId, duties);
            }
            else
            {
                var duties = Service.ExcelCache.GetSubmarineExplorationExSheet().Where(c => c.FormattedName.ToString() != "" && c.Map.Row == tabId).ToList();
                _items.Add(tabId, duties);
            }
        }

        if (!_filteredItems.ContainsKey(tabId) && _items.ContainsKey(tabId))
        {
            var unfilteredList = _items[tabId];
            if (SortColumn != null && _columns[(int)SortColumn].Sort != null)
            {
                unfilteredList = _columns[(int)SortColumn].Sort?.Invoke(SortDirection, unfilteredList).ToList();
            }

            foreach (var column in _columns)
            {
                if (column.Filter != null && column.FilterText != "")
                {
                    unfilteredList = column.Filter(column.FilterText, unfilteredList).ToList();
                }
                if (column.FilterBool != null && column.FilterBoolean != null)
                {
                    unfilteredList = column.FilterBool(column.FilterBoolean, unfilteredList).ToList();
                }
            }

            _filteredItems.Add(tabId, unfilteredList);
        }

        return _filteredItems[tabId];
    }

    public override Dictionary<uint, string> Tabs => _tabs;
    public override string TableName => _tableName;

    public override bool UseClipper => _useClipper;
    
    private string _tableName = "submarines";
    private bool _useClipper = false;
}