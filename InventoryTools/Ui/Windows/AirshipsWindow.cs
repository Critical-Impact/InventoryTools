using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using AllaganLib.GameSheets.Caches;
using AllaganLib.GameSheets.Sheets;
using AllaganLib.GameSheets.Sheets.Rows;
using AllaganLib.Shared.Extensions;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services.Mediator;

using ImGuiNET;
using InventoryTools.Extensions;
using InventoryTools.Logic;
using InventoryTools.Mediator;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;
using ImGuiUtil = OtterGui.ImGuiUtil;

namespace InventoryTools.Ui;

public class AirshipsWindow : GenericTabbedTable<AirshipExplorationPointRow>, IMenuWindow
{
    private readonly AirshipExplorationPointSheet _airshipExplorationPointSheet;
    private readonly ItemInfoCache _itemInfoCache;
    private readonly ItemSheet _itemSheet;

    public AirshipsWindow(ILogger<AirshipsWindow> logger, MediatorService mediator, ImGuiService imGuiService, InventoryToolsConfiguration configuration, AirshipExplorationPointSheet airshipExplorationPointSheet, ItemInfoCache itemInfoCache, ItemSheet itemSheet, string name = "Airships Window") : base(logger, mediator, imGuiService, configuration, name)
    {
        _airshipExplorationPointSheet = airshipExplorationPointSheet;
        _itemInfoCache = itemInfoCache;
        _itemSheet = itemSheet;
    }
    public override void Initialize()
    {
        WindowName = GenericName;
        Key = GenericKey;
        _columns = new List<TableColumn<AirshipExplorationPointRow>>()
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

                    return specs == ImGuiSortDirection.Ascending ? exes.OrderBy(c => c.Base.NameShort.ExtractText()) : exes.OrderByDescending(c => c.Base.NameShort.ExtractText());
                },
                Filter = (s, exes) =>
                {
                    if (s == null)
                    {
                        return exes;
                    }
                    return s == "" ? exes : exes.Where(c => c.Base.NameShort.ExtractText().ToLower().PassesFilter(s.ToLower()));
                },
                Draw = (ex, tabId) =>
                {
                    ImGui.TextUnformatted(ex.Base.NameShort.ExtractText().ToString());
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

                    return specs == ImGuiSortDirection.Ascending ? exes.OrderBy(c => c.Unlock?.Base.NameShort.ExtractText() ?? "") : exes.OrderByDescending(c => c.Unlock?.Base.NameShort.ExtractText() ?? "");
                },
                Filter = (s, exes) =>
                {
                    if (s == null)
                    {
                        return exes;
                    }
                    return s == "" ? exes : exes.Where(c => (c.Unlock?.Base.NameShort.ExtractText() ?? "").ToLower().PassesFilter(s.ToLower()));
                },
                Draw = (ex, tabId) =>
                {
                    if (ex.Unlock != null)
                    {
                        ImGui.TextUnformatted((ex.Unlock?.Base.NameShort.ExtractText() ?? "").ToString());
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

                    return specs == ImGuiSortDirection.Ascending ? exes.OrderBy(c => c.Base.RankReq.ToString() ?? "") : exes.OrderByDescending(c => c.Base.RankReq.ToString() ?? "");
                },
                Filter = (s, exes) =>
                {
                    if (s == null)
                    {
                        return exes;
                    }
                    return s == "" ? exes : exes.Where(c => (c.Base.RankReq.ToString() ?? "").ToLower().PassesFilter(s.ToLower()));
                },
                Draw = (ex, tabId) =>
                {
                    ImGui.TextUnformatted((ex.Base.RankReq.ToString() ?? "").ToString());
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

                    return specs == ImGuiSortDirection.Ascending ? exes.OrderBy(c => c.Base.CeruleumTankReq) : exes.OrderByDescending(c => c.Base.CeruleumTankReq);
                },
                Filter = (s, exes) =>
                {
                    if (s == null)
                    {
                        return exes;
                    }
                    return s == "" ? exes : exes.Where(c => c.Base.CeruleumTankReq.PassesFilter(s.ToLower()));
                },
                Draw = (ex, tabId) =>
                {
                    ImGui.TextUnformatted((ex.Base.CeruleumTankReq.ToString() ?? "").ToString());
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

                    return specs == ImGuiSortDirection.Ascending ? exes.OrderBy(c => c.Base.SurveyDurationmin) : exes.OrderByDescending(c => c.Base.SurveyDurationmin);
                },
                Filter = (s, exes) =>
                {
                    if (s == null)
                    {
                        return exes;
                    }
                    return s == "" ? exes : exes.Where(c => c.Base.SurveyDurationmin.PassesFilter(s.ToLower()));
                },
                Draw = (ex, tabId) =>
                {
                    ImGui.TextUnformatted((ex.Base.SurveyDurationmin.ToString() ?? "").ToString());
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

                    return specs == ImGuiSortDirection.Ascending ? exes.OrderBy(c => c.Base.SurveillanceReq) : exes.OrderByDescending(c => c.Base.SurveillanceReq);
                },
                Filter = (s, exes) =>
                {
                    if (s == null)
                    {
                        return exes;
                    }
                    return s == "" ? exes : exes.Where(c => ((uint)c.Base.SurveillanceReq).PassesFilter(s.ToLower()));
                },
                Draw = (ex, tabId) =>
                {
                    ImGui.TextUnformatted((ex.Base.SurveillanceReq.ToString() ?? "").ToString());
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
                        return _airshipExplorationPointSheet.GetItemsByAirshipExplorationPoint(c.RowId).Any(itemId => _itemSheet.GetRow(itemId).NameString.ToLower().PassesFilter(s));
                    });
                },
                Draw = (ex, contentTypeId) =>
                {
                    var drops = _airshipExplorationPointSheet.GetItemsByAirshipExplorationPoint(ex.RowId);
                    ImGuiService.WrapTableColumnElements("Drops" + ex.RowId, drops,
                    RowSize * ImGui.GetIO().FontGlobalScale - ImGui.GetStyle().FramePadding.X,
                    itemId =>
                    {
                        var drop = _itemSheet.GetRow(itemId);
                        ImGui.Image(ImGuiService.GetIconTexture(drop.Icon).ImGuiHandle,
                            new Vector2(RowSize, RowSize) * ImGui.GetIO().FontGlobalScale);
                        if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled &
                                                ImGuiHoveredFlags.AllowWhenOverlapped &
                                                ImGuiHoveredFlags.AllowWhenBlockedByPopup &
                                                ImGuiHoveredFlags.AllowWhenBlockedByActiveItem &
                                                ImGuiHoveredFlags.AnyWindow) &&
                            ImGui.IsMouseReleased(ImGuiMouseButton.Right))
                        {
                            ImGui.OpenPopup("RightClick" + drop.RowId);
                        }

                        if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled &
                                                ImGuiHoveredFlags.AllowWhenOverlapped &
                                                ImGuiHoveredFlags.AllowWhenBlockedByPopup &
                                                ImGuiHoveredFlags.AllowWhenBlockedByActiveItem &
                                                ImGuiHoveredFlags.AnyWindow) &&
                            ImGui.IsMouseReleased(ImGuiMouseButton.Left))
                        {
                            MediatorService.Publish(new OpenUintWindowMessage(typeof(ItemWindow), drop.Base.RowId));
                        }

                        if (ImGui.BeginPopup("RightClick" + drop.RowId))
                        {
                            MediatorService.Publish(ImGuiService.ImGuiMenuService.DrawRightClickPopup(drop));
                            ImGui.EndPopup();
                        }
                        ImGuiUtil.HoverTooltip(drop.NameString);

                        return true;
                    });

                }
            },
        };
        _tabs = new Dictionary<uint, string>()
        {
            {0, "All"}
        };
        _items = new Dictionary<uint, List<AirshipExplorationPointRow>>();
        _filteredItems = new Dictionary<uint, List<AirshipExplorationPointRow>>();
    }

    private bool OnLeftClick(AirshipExplorationPointRow arg)
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

    public override int GetRowId(AirshipExplorationPointRow item)
    {
        return (int)item.RowId;
    }

    public override Dictionary<uint, List<AirshipExplorationPointRow>> Items => _items;

    public override Dictionary<uint, List<AirshipExplorationPointRow>> FilteredItems => _filteredItems;

    public override List<TableColumn<AirshipExplorationPointRow>> Columns => _columns;

    private List<TableColumn<AirshipExplorationPointRow>> _columns = null!;
    private Dictionary<uint, List<AirshipExplorationPointRow>> _items = null!;
    private Dictionary<uint, List<AirshipExplorationPointRow>> _filteredItems = null!;
    private Dictionary<uint, string> _tabs = null!;

    public override ImGuiTableFlags TableFlags => _flags;

    private ImGuiTableFlags _flags = ImGuiTableFlags.RowBg | ImGuiTableFlags.BordersV |
                                     ImGuiTableFlags.BordersOuterV | ImGuiTableFlags.BordersInnerV |
                                     ImGuiTableFlags.BordersH | ImGuiTableFlags.BordersOuterH |
                                     ImGuiTableFlags.BordersInnerH |
                                     ImGuiTableFlags.Resizable | ImGuiTableFlags.Sortable |
                                     ImGuiTableFlags.Hideable | ImGuiTableFlags.ScrollX |
                                     ImGuiTableFlags.ScrollY;
    public override List<AirshipExplorationPointRow> GetItems(uint tabId)
    {
        if (!_items.ContainsKey(tabId))
        {
            if (tabId == 0)
            {
                var duties = _airshipExplorationPointSheet.Where(c => c.Base.Name.ExtractText().ToString() != "" && c.Base.Passengers == false).ToList();
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