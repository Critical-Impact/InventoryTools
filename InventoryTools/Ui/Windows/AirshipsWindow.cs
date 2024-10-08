using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using CriticalCommonLib.Services.Mediator;
using CriticalCommonLib.Sheets;
using ImGuiNET;
using InventoryTools.Extensions;
using InventoryTools.Logic;
using InventoryTools.Mediator;
using InventoryTools.Services;
using InventoryTools.Services.Interfaces;
using Microsoft.Extensions.Logging;
using ImGuiUtil = OtterGui.ImGuiUtil;

namespace InventoryTools.Ui;

public class AirshipsWindow : GenericTabbedTable<AirshipExplorationPointEx>, IMenuWindow
{
    private readonly ExcelCache _excelCache;

    public AirshipsWindow(ILogger<AirshipsWindow> logger, MediatorService mediator, ImGuiService imGuiService, InventoryToolsConfiguration configuration, ExcelCache excelCache, string name = "Airships Window") : base(logger, mediator, imGuiService, configuration, name)
    {
        _excelCache = excelCache;
    }
    public override void Initialize()
    {
        WindowName = GenericName;
        Key = GenericKey;
        _columns = new List<TableColumn<AirshipExplorationPointEx>>()
        {
            new("Icon", 32, ImGuiTableColumnFlags.WidthFixed)
            {
                OnLeftClick = OnLeftClick,
                Draw = (ex, contentTypeId) =>
                {
                    if (ImGui.ImageButton(ImGuiService.GetIconTexture(Icons.AirshipIcon).ImGuiHandle,
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
                    ImGui.TextUnformatted(ex.FormattedNameShort.ToString());
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
                        ImGui.TextUnformatted((ex.UnlockPointEx.Value?.FormattedNameShort ?? "").ToString());
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
                    ImGui.TextUnformatted((ex.RankReq.ToString() ?? "").ToString());
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
                    ImGui.TextUnformatted((ex.CeruleumTankReq.ToString() ?? "").ToString());
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
                    ImGui.TextUnformatted((ex.SurveyDurationmin.ToString() ?? "").ToString());
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
                    ImGui.TextUnformatted((ex.SurveillanceReq.ToString() ?? "").ToString());
                }
            },
            new("Drops", 200, ImGuiTableColumnFlags.WidthFixed | ImGuiTableColumnFlags.NoSort)
            {
                Sort = (specs, exes) =>
                {
                    return exes;
                },
                Filter = (s, npcs) =>
                {
                    return s == null ? npcs : npcs.Where(c =>
                    {
                        var currentValue = c.Drops.Where(c => c.Value != null);

                        return currentValue.Any(c => c.Value!.NameString.ToLower().PassesFilter(s));
                    });
                },
                Draw = (ex, contentTypeId) =>
                {
                    var drops = ex.Drops.Where(c => c.Value != null);
                    ImGuiService.WrapTableColumnElements("Drops" + ex.RowId, drops,
                    RowSize * ImGui.GetIO().FontGlobalScale - ImGui.GetStyle().FramePadding.X,
                    drop =>
                    {
                        if (drop.Value != null)
                        {
                            ImGui.Image(ImGuiService.GetIconTexture(drop.Value.Icon).ImGuiHandle,
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
                                MediatorService.Publish(new OpenUintWindowMessage(typeof(ItemWindow), drop.Value.RowId));
                            }

                            if (ImGui.BeginPopup("RightClick" + drop.Value.RowId))
                            {
                                MediatorService.Publish(ImGuiService.RightClickService.DrawRightClickPopup(drop.Value));
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
        MediatorService.Publish(new OpenUintWindowMessage(typeof(AirshipWindow), arg.RowId));
        return true;
    }

    public override string GenericKey => "airships";
    public override string GenericName => "Airships";
    public override bool DestroyOnClose => false;
    public override bool SaveState => true;
    public override Vector2? MaxSize { get; } = new(2000, 2000);
    public override Vector2? MinSize { get; } = new(200, 200);
    public override Vector2? DefaultSize { get; } = new(600, 600);

    public override void Draw()
    {
        DrawTabs();
    }

    public override void Invalidate()
    {
    }

    public override FilterConfiguration? SelectedConfiguration => null;

    public override int GetRowId(AirshipExplorationPointEx item)
    {
        return (int)item.RowId;
    }

    public override Dictionary<uint, List<AirshipExplorationPointEx>> Items => _items;

    public override Dictionary<uint, List<AirshipExplorationPointEx>> FilteredItems => _filteredItems;

    public override List<TableColumn<AirshipExplorationPointEx>> Columns => _columns;

    private List<TableColumn<AirshipExplorationPointEx>> _columns = null!;
    private Dictionary<uint, List<AirshipExplorationPointEx>> _items = null!;
    private Dictionary<uint, List<AirshipExplorationPointEx>> _filteredItems = null!;
    private Dictionary<uint, string> _tabs = null!;

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
                var duties = _excelCache.GetAirshipExplorationPointExSheet().Where(c => c.FormattedName.ToString() != "" && c.Passengers == false).ToList();
                _items.Add(tabId, duties);
            }
        }

        if (!_filteredItems.ContainsKey(tabId) && _items.ContainsKey(tabId))
        {
            var unfilteredList = _items[tabId];
            if (SortColumn != null && _columns[(int)SortColumn].Sort != null)
            {
                unfilteredList = _columns[(int)SortColumn].Sort?.Invoke(SortDirection, unfilteredList).ToList() ?? unfilteredList;
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