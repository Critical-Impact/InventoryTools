using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using CriticalCommonLib;
using CriticalCommonLib.Sheets;
using ImGuiNET;
using InventoryTools.Extensions;
using InventoryTools.Logic;
using InventoryTools.Ui.Widgets;
using ImGuiUtil = OtterGui.ImGuiUtil;

namespace InventoryTools.Ui;

public class AirshipsWindow : GenericTabbedTable<AirshipExplorationPointEx>
{
    public AirshipsWindow(string name = "Allagan Tools - Airships", ImGuiWindowFlags flags = ImGuiWindowFlags.None, bool forceMainWindow = false) : base(name, flags, forceMainWindow)
    {
        SetupWindow();
    }

    public AirshipsWindow() : base("Allagan Tools - Airships")
    {
        SetupWindow();
    }

    private void SetupWindow()
    {
        _columns = new List<TableColumn<AirshipExplorationPointEx>>()
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
            new("Name", 150, ImGuiTableColumnFlags.WidthFixed)
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
            new("Unlock Zone", 150, ImGuiTableColumnFlags.WidthFixed)
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

                    return specs == ImGuiSortDirection.Ascending ? exes.OrderBy(c => c.RankReq.ToString() ?? "") : exes.OrderByDescending(c => c.RankReq.ToString() ?? "");
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
                    ImGui.Text((ex.RankReq.ToString() ?? "").ToString());
                }
            },
            new("Ceruleum Required", 100, ImGuiTableColumnFlags.WidthFixed)
            {
                Sort = (specs, exes) =>
                {
                    if (specs == null)
                    {
                        return exes;
                    }

                    return specs == ImGuiSortDirection.Ascending ? exes.OrderBy(c => c.CeruleumTankReq) : exes.OrderByDescending(c => c.CeruleumTankReq);
                },
                Filter = (s, exes) =>
                {
                    if (s == null)
                    {
                        return exes;
                    }
                    return s == "" ? exes : exes.Where(c => c.CeruleumTankReq.PassesFilter(s.ToLower()));
                },
                Draw = (ex, tabId) =>
                {
                    ImGui.Text((ex.CeruleumTankReq.ToString() ?? "").ToString());
                }
            },
            new("Survey Duration (minutes)", 100, ImGuiTableColumnFlags.WidthFixed)
            {
                Sort = (specs, exes) =>
                {
                    if (specs == null)
                    {
                        return exes;
                    }

                    return specs == ImGuiSortDirection.Ascending ? exes.OrderBy(c => c.SurveyDurationmin) : exes.OrderByDescending(c => c.SurveyDurationmin);
                },
                Filter = (s, exes) =>
                {
                    if (s == null)
                    {
                        return exes;
                    }
                    return s == "" ? exes : exes.Where(c => c.SurveyDurationmin.PassesFilter(s.ToLower()));
                },
                Draw = (ex, tabId) =>
                {
                    ImGui.Text((ex.SurveyDurationmin.ToString() ?? "").ToString());
                }
            },
            new("Surveillance Required", 100, ImGuiTableColumnFlags.WidthFixed)
            {
                Sort = (specs, exes) =>
                {
                    if (specs == null)
                    {
                        return exes;
                    }

                    return specs == ImGuiSortDirection.Ascending ? exes.OrderBy(c => c.SurveillanceReq) : exes.OrderByDescending(c => c.SurveillanceReq);
                },
                Filter = (s, exes) =>
                {
                    if (s == null)
                    {
                        return exes;
                    }
                    return s == "" ? exes : exes.Where(c => ((uint)c.SurveillanceReq).PassesFilter(s.ToLower()));
                },
                Draw = (ex, tabId) =>
                {
                    ImGui.Text((ex.SurveillanceReq.ToString() ?? "").ToString());
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
                    var drops = ex.Drops.Where(c => c.Value != null);
                    UiHelpers.WrapTableColumnElements("Drops" + ex.RowId, drops,
                    RowSize - ImGui.GetStyle().FramePadding.X,
                    drop =>
                    {
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
                        }

                        return true;
                    });

                }
            },
        };
        _tabs = new Dictionary<uint, string>()
        {
            {0, "All"}
        };
        _items = new Dictionary<uint, List<AirshipExplorationPointEx>>();
        _filteredItems = new Dictionary<uint, List<AirshipExplorationPointEx>>();        
    }
    
    private bool OnLeftClick(AirshipExplorationPointEx arg)
    {
        PluginService.WindowService.OpenAirshipWindow(arg.RowId);
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
        get { return "airships"; }
    }

    public override void Invalidate()
    {
    }

    public override FilterConfiguration? SelectedConfiguration { get; } = null;
    public override int GetRowId(AirshipExplorationPointEx item)
    {
        return (int)item.RowId;
    }

    public override Dictionary<uint, List<AirshipExplorationPointEx>> Items => _items;

    public override Dictionary<uint, List<AirshipExplorationPointEx>> FilteredItems => _filteredItems;

    public override List<TableColumn<AirshipExplorationPointEx>> Columns => _columns;
    
    private List<TableColumn<AirshipExplorationPointEx>> _columns;
    private Dictionary<uint, List<AirshipExplorationPointEx>> _items;
    private Dictionary<uint, List<AirshipExplorationPointEx>> _filteredItems;
    private Dictionary<uint, string> _tabs;

    public override ImGuiTableFlags TableFlags => _flags;
    
    private ImGuiTableFlags _flags = ImGuiTableFlags.RowBg | ImGuiTableFlags.BordersV |
                                     ImGuiTableFlags.BordersOuterV | ImGuiTableFlags.BordersInnerV |
                                     ImGuiTableFlags.BordersH | ImGuiTableFlags.BordersOuterH |
                                     ImGuiTableFlags.BordersInnerH |
                                     ImGuiTableFlags.Resizable | ImGuiTableFlags.Sortable |
                                     ImGuiTableFlags.Hideable | ImGuiTableFlags.ScrollX |
                                     ImGuiTableFlags.ScrollY;
    public override List<AirshipExplorationPointEx> GetItems(uint tabId)
    {
        if (!_items.ContainsKey(tabId))
        {
            if (tabId == 0)
            {
                var duties = Service.ExcelCache.GetAirshipExplorationPointExSheet().Where(c => c.FormattedName.ToString() != "" && c.Passengers == false).ToList();
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
    
    private string _tableName = "airships";
    private bool _useClipper = false;
}