using System.Collections.Generic;
using System.Linq;
using System.Numerics;
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
using Lumina.Excel;
using Lumina.Excel.Sheets;
using Microsoft.Extensions.Logging;
using ImGuiUtil = OtterGui.ImGuiUtil;

namespace InventoryTools.Ui;

public class SubmarinesWindow : GenericTabbedTable<SubmarineExplorationRow>, IMenuWindow
{
    private readonly ImGuiService _imGuiService;
    private readonly SubmarineExplorationSheet _submarineExplorationSheet;
    private readonly ExcelSheet<SubmarineMap> _submarineMapSheet;

    public SubmarinesWindow(ILogger<SubmarinesWindow> logger, MediatorService mediator, ImGuiService imGuiService, InventoryToolsConfiguration configuration, SubmarineExplorationSheet submarineExplorationSheet, ExcelSheet<SubmarineMap> submarineMapSheet, string name = "Submarines Window") : base(logger, mediator, imGuiService, configuration, name)
    {
        _imGuiService = imGuiService;
        _submarineExplorationSheet = submarineExplorationSheet;
        _submarineMapSheet = submarineMapSheet;
    }
    public override void Initialize()
    {
        Key = "submarines";
        WindowName = "Submarines";
        _columns = new List<TableColumn<SubmarineExplorationRow>>()
        {
            new("Icon", 32, ImGuiTableColumnFlags.WidthFixed)
            {
                OnLeftClick = OnLeftClick,
                Draw = (ex, contentTypeId) =>
                {
                    if (ImGui.ImageButton(_imGuiService.GetIconTexture(Icons.AirshipIcon).ImGuiHandle,
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

                    return specs == ImGuiSortDirection.Ascending ? exes.OrderBy(c => c.Base.Location.ExtractText()) : exes.OrderByDescending(c => c.Base.Location.ExtractText());
                },
                Filter = (s, exes) =>
                {
                    if (s == null)
                    {
                        return exes;
                    }
                    return s == "" ? exes : exes.Where(c => c.Base.Location.ExtractText().ToLower().PassesFilter(s.ToLower()));
                },
                Draw = (ex, tabId) =>
                {
                    ImGui.TextUnformatted(ex.Base.Location.ExtractText());
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

                    return specs == ImGuiSortDirection.Ascending ? exes.OrderBy(c => c.Unlock?.Base.Location.ExtractText() ?? "") : exes.OrderByDescending(c => c.Unlock?.Base.Location.ExtractText() ?? "");
                },
                Filter = (s, exes) =>
                {
                    if (s == null)
                    {
                        return exes;
                    }
                    return s == "" ? exes : exes.Where(c => (c.Unlock?.Base.Location.ExtractText() ?? "").ToLower().PassesFilter(s.ToLower()));
                },
                Draw = (ex, tabId) =>
                {
                    if (ex.Unlock != null)
                    {
                        ImGui.TextUnformatted((ex.Unlock?.Base.Location.ExtractText() ?? "").ToString());
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

                    return specs == ImGuiSortDirection.Ascending ? exes.OrderBy(c => c.Base.RankReq) : exes.OrderByDescending(c => c.Base.RankReq);
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
                    if (ex.Unlock != null)
                    {
                        ImGui.TextUnformatted((ex.Base.RankReq.ToString() ?? "").ToString());
                    }
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
                        var currentValue = c.DropItems;

                        return currentValue.Any(c => c.NameString.ToLower().PassesFilter(s));
                    });
                },
                Draw = (ex, contentTypeId) =>
                {
                    var drops = ex.DropItems;
                    _imGuiService.WrapTableColumnElements("Drops" + ex.RowId, drops,
                    RowSize * ImGui.GetIO().FontGlobalScale - ImGui.GetStyle().FramePadding.X,
                    drop =>
                    {

                        var sourceIcon = _imGuiService.GetIconTexture(drop.Icon);
                        ImGui.Image(sourceIcon.ImGuiHandle,
                            new Vector2(RowSize, RowSize) * ImGui.GetIO().FontGlobalScale);
                        if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled &
                                                ImGuiHoveredFlags.AllowWhenOverlapped &
                                                ImGuiHoveredFlags.AllowWhenBlockedByPopup &
                                                ImGuiHoveredFlags.AllowWhenBlockedByActiveItem &
                                                ImGuiHoveredFlags.AnyWindow) &&
                            ImGui.IsMouseReleased(ImGuiMouseButton.Right))
                        {
                            ImGui.OpenPopup("RightClick" + drop);
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

                        if (ImGui.BeginPopup("RightClick" + drop))
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
        _tabs = _submarineMapSheet.Where(c => c.Name.ExtractText().ToString() != "").ToDictionary(c => c.RowId, c =>c.Name.ToString());
        _items = new Dictionary<uint, List<SubmarineExplorationRow>>();
        _filteredItems = new Dictionary<uint, List<SubmarineExplorationRow>>();
    }

    private bool OnLeftClick(SubmarineExplorationRow arg)
    {
        MediatorService.Publish(new OpenGenericWindowMessage(typeof(SubmarineWindow)));
        return true;
    }

    public override string GenericKey { get; } = "submarines";
    public override string GenericName { get; } = "Submarines";
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

    public override int GetRowId(SubmarineExplorationRow item)
    {
        return (int)item.RowId;
    }

    public override Dictionary<uint, List<SubmarineExplorationRow>> Items => _items;

    public override Dictionary<uint, List<SubmarineExplorationRow>> FilteredItems => _filteredItems;

    public override List<TableColumn<SubmarineExplorationRow>> Columns => _columns;

    private List<TableColumn<SubmarineExplorationRow>> _columns = null!;
    private Dictionary<uint, List<SubmarineExplorationRow>> _items= null!;
    private Dictionary<uint, List<SubmarineExplorationRow>> _filteredItems= null!;
    private Dictionary<uint, string> _tabs= null!;

    public override ImGuiTableFlags TableFlags => _flags;

    private ImGuiTableFlags _flags = ImGuiTableFlags.RowBg | ImGuiTableFlags.BordersV |
                                     ImGuiTableFlags.BordersOuterV | ImGuiTableFlags.BordersInnerV |
                                     ImGuiTableFlags.BordersH | ImGuiTableFlags.BordersOuterH |
                                     ImGuiTableFlags.BordersInnerH |
                                     ImGuiTableFlags.Resizable | ImGuiTableFlags.Sortable |
                                     ImGuiTableFlags.Hideable | ImGuiTableFlags.ScrollX |
                                     ImGuiTableFlags.ScrollY;
    public override List<SubmarineExplorationRow> GetItems(uint tabId)
    {
        if (!_items.ContainsKey(tabId))
        {
            if (tabId == 0)
            {
                var duties = _submarineExplorationSheet.Where(c => c.Base.Location.ExtractText() != "").ToList();
                _items.Add(tabId, duties);
            }
            else
            {
                var duties = _submarineExplorationSheet.Where(c => c.Base.Location.ExtractText() != "" && c.Base.Map.RowId == tabId).ToList();
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

    private string _tableName = "submarines";
    private bool _useClipper = false;

}