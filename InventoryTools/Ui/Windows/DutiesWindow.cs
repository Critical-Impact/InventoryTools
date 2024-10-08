using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using CriticalCommonLib.Services;
using CriticalCommonLib.Services.Mediator;
using CriticalCommonLib.Sheets;
using Dalamud.Utility;
using ImGuiNET;
using InventoryTools.Extensions;
using InventoryTools.Logic;
using InventoryTools.Mediator;
using InventoryTools.Services;
using InventoryTools.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Ui;

public class DutiesWindow : GenericTabbedTable<ContentFinderConditionEx>, IMenuWindow
{
    private readonly ImGuiService _imGuiService;
    private readonly ExcelCache _excelCache;

    public DutiesWindow(ILogger<DutiesWindow> logger, MediatorService mediator, ImGuiService imGuiService, InventoryToolsConfiguration configuration, ExcelCache excelCache, string name = "Duties Window") : base(logger, mediator, imGuiService, configuration, name)
    {
        _imGuiService = imGuiService;
        _excelCache = excelCache;
    }
    public override void Initialize()
    {
        WindowName = "Duties";
        Key = "duties";

        _columns = new List<TableColumn<ContentFinderConditionEx>>()
        {
            new("Icon", 32, ImGuiTableColumnFlags.WidthFixed)
            {
                OnLeftClick = OnLeftClick,
                Draw = (ex, contentTypeId) =>
                {
                    if (ImGui.ImageButton(ImGuiService.GetIconTexture((int)ex.ContentType.Value!.IconDutyFinder).ImGuiHandle,
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

                    return specs == ImGuiSortDirection.Ascending ? exes.OrderBy(c => c.FormattedName) : exes.OrderByDescending(c => c.FormattedName);
                },
                Filter = (s, exes) =>
                {
                    if (s == null)
                    {
                        return exes;
                    }
                    return s == "" ? exes : exes.Where(c => c.FormattedName.ToLower().PassesFilter(s.ToLower()));
                },
                Draw = (ex, contentTypeId) =>
                {
                    ImGui.TextUnformatted(ex.FormattedName);
                }
            },
            new("Roulettes", 200, ImGuiTableColumnFlags.WidthFixed)
            {
                Sort = (specs, exes) =>
                {
                    if (specs == null)
                    {
                        return exes;
                    }

                    return specs == ImGuiSortDirection.Ascending ? exes.OrderBy(c => c.Roulettes) : exes.OrderByDescending(c => c.Roulettes);
                },
                Filter = (s, exes) =>
                {
                    if (s == null)
                    {
                        return exes;
                    }
                    return s == "" ? exes : exes.Where(c => c.Roulettes.ToLower().PassesFilter(s.ToLower()));
                },
                Draw = (ex, contentTypeId) =>
                {
                    ImGui.TextUnformatted(ex.Roulettes);
                }
            },
            new("Level", 100, ImGuiTableColumnFlags.WidthFixed)
            {
                Sort = (specs, exes) =>
                {
                    if (specs == null)
                    {
                        return exes;
                    }

                    return specs == ImGuiSortDirection.Ascending ? exes.OrderBy(c => c.ClassJobLevelRequired) : exes.OrderByDescending(c => c.ClassJobLevelRequired);
                },
                Filter = (s, exes) =>
                {
                    if (s == null)
                    {
                        return exes;
                    }
                    return s == "" ? exes : exes.Where(c => ((int)c.ClassJobLevelRequired).PassesFilter(s.ToLower()));
                },
                Draw = (ex, contentTypeId) =>
                {
                    ImGui.TextUnformatted(ex.ClassJobLevelRequired.ToString());
                }
            },
            new("Sync Level", 100, ImGuiTableColumnFlags.WidthFixed)
            {
                Sort = (specs, exes) =>
                {
                    if (specs == null)
                    {
                        return exes;
                    }

                    return specs == ImGuiSortDirection.Ascending ? exes.OrderBy(c => c.ClassJobLevelSync) : exes.OrderByDescending(c => c.ClassJobLevelSync);
                },
                Filter = (s, exes) =>
                {
                    if (s == null)
                    {
                        return exes;
                    }
                    return s == "" ? exes : exes.Where(c => ((int)c.ClassJobLevelSync).PassesFilter(s.ToLower()));
                },
                Draw = (ex, contentTypeId) =>
                {
                    ImGui.TextUnformatted(ex.ClassJobLevelSync.ToString());
                }
            },
            new("Item Level", 100, ImGuiTableColumnFlags.WidthFixed)
            {
                Sort = (specs, exes) =>
                {
                    if (specs == null)
                    {
                        return exes;
                    }

                    return specs == ImGuiSortDirection.Ascending ? exes.OrderBy(c => c.ItemLevelRequired) : exes.OrderByDescending(c => c.ItemLevelRequired);
                },
                Filter = (s, exes) =>
                {
                    if (s == null)
                    {
                        return exes;
                    }
                    return s == "" ? exes : exes.Where(c => ((int)c.ItemLevelRequired).PassesFilter(s.ToLower()));
                },
                Draw = (ex, contentTypeId) =>
                {
                    ImGui.TextUnformatted(ex.ItemLevelRequired.ToString());
                }
            },
            new("Item Level Sync", 100, ImGuiTableColumnFlags.WidthFixed)
            {
                Sort = (specs, exes) =>
                {
                    if (specs == null)
                    {
                        return exes;
                    }

                    return specs == ImGuiSortDirection.Ascending ? exes.OrderBy(c => c.ItemLevelSync) : exes.OrderByDescending(c => c.ItemLevelSync);
                },
                Filter = (s, exes) =>
                {
                    if (s == null)
                    {
                        return exes;
                    }
                    return s == "" ? exes : exes.Where(c => ((int)c.ItemLevelSync).PassesFilter(s.ToLower()));
                },
                Draw = (ex, contentTypeId) =>
                {
                    ImGui.TextUnformatted(ex.ItemLevelSync.ToString());
                }
            },
            new("Allows Undersized", 80, ImGuiTableColumnFlags.WidthFixed)
            {
                Sort = (specs, exes) =>
                {
                    if (specs == null)
                    {
                        return exes;
                    }

                    return specs == ImGuiSortDirection.Ascending ? exes.OrderBy(c => c.AllowUndersized) : exes.OrderByDescending(c => c.AllowUndersized);
                },
                FilterBool = (s, exes) =>
                {
                    if (s == null)
                    {
                        return exes;
                    }
                    return exes.Where(c => c.AllowUndersized == s);
                },
                Draw = (ex, contentTypeId) =>
                {
                    ImGui.SetCursorPosX(ImGui.GetCursorPosX() + (ImGui.GetContentRegionAvail().X / 2) - RowSize / 2.0f);
                    _imGuiService.DrawUldIcon(ex.AllowUndersized ? _imGuiService.TickIcon : _imGuiService.CrossIcon, new Vector2(RowSize, RowSize));
                }
            },
            new("Allows Explorer Mode", 80, ImGuiTableColumnFlags.WidthFixed)
            {
                Sort = (specs, exes) =>
                {
                    if (specs == null)
                    {
                        return exes;
                    }

                    return specs == ImGuiSortDirection.Ascending ? exes.OrderBy(c => c.AllowExplorerMode) : exes.OrderByDescending(c => c.AllowExplorerMode);
                },
                FilterBool = (s, exes) =>
                {
                    if (s == null)
                    {
                        return exes;
                    }
                    return exes.Where(c => c.AllowExplorerMode == s);
                },
                Draw = (ex, contentTypeId) =>
                {
                    ImGui.SetCursorPosX(ImGui.GetCursorPosX() + (ImGui.GetContentRegionAvail().X / 2) - RowSize / 2.0f);
                    _imGuiService.DrawUldIcon(ex.AllowExplorerMode ? _imGuiService.TickIcon : _imGuiService.CrossIcon, new Vector2(RowSize, RowSize));
                }
            },
            new("PVP", 50, ImGuiTableColumnFlags.WidthFixed)
            {
                Sort = (specs, exes) =>
                {
                    if (specs == null)
                    {
                        return exes;
                    }

                    return specs == ImGuiSortDirection.Ascending ? exes.OrderBy(c => c.PvP) : exes.OrderByDescending(c => c.PvP);
                },
                FilterBool = (s, exes) =>
                {
                    if (s == null)
                    {
                        return exes;
                    }
                    return exes.Where(c => c.PvP == s);
                },
                Draw = (ex, contentTypeId) =>
                {
                    ImGui.SetCursorPosX(ImGui.GetCursorPosX() + (ImGui.GetContentRegionAvail().X / 2) - RowSize / 2.0f);
                    _imGuiService.DrawUldIcon(ex.PvP ? _imGuiService.TickIcon : _imGuiService.CrossIcon, new Vector2(RowSize, RowSize));
                }
            },
            new("Accepted Classes", 100, ImGuiTableColumnFlags.WidthFixed)
            {
                Sort = (specs, exes) =>
                {
                    if (specs == null)
                    {
                        return exes;
                    }

                    return specs == ImGuiSortDirection.Ascending ? exes.OrderBy(c => c.AcceptClassJobCategoryEx.Value?.FormattedName ?? "Unknown") : exes.OrderByDescending(c => c.AcceptClassJobCategoryEx.Value?.FormattedName ?? "Unknown");
                },
                Filter = (s, exes) =>
                {
                    if (s == null)
                    {
                        return exes;
                    }
                    return s == "" ? exes : exes.Where(c => (c.AcceptClassJobCategoryEx.Value?.FormattedName ?? "Unknown").ToLower().PassesFilter(s.ToLower()));
                },
                Draw = (ex, contentTypeId) =>
                {
                    ImGui.TextUnformatted(ex.AcceptClassJobCategoryEx.Value?.FormattedName ?? "Unknown");
                }
            },
        };
        _tabs = _excelCache.GetContentTypeSheet().Where(c => c.Name.ToDalamudString().ToString() != "" && c.IconDutyFinder != 0).ToDictionary(c => c.RowId, c =>c.Name.ToString());
        _items = new Dictionary<uint, List<ContentFinderConditionEx>>();
        _filteredItems = new Dictionary<uint, List<ContentFinderConditionEx>>();
    }

    private bool OnLeftClick(ContentFinderConditionEx arg)
    {
        MediatorService.Publish(new OpenUintWindowMessage(typeof(DutyWindow), arg.RowId));
        return true;
    }

    public override List<ContentFinderConditionEx> GetItems(uint contentTypeId)
    {
        if (!_items.ContainsKey(contentTypeId))
        {
            if (contentTypeId == 0)
            {
                var duties = _excelCache.GetContentFinderConditionExSheet().Where(c => c.Name.ToDalamudString().ToString() != "").ToList();
                _items.Add(contentTypeId, duties);
            }
            else
            {
                var duties = _excelCache.GetContentFinderConditionExSheet().Where(c => c.Name.ToDalamudString().ToString() != "" && c.ContentType.Row == contentTypeId).ToList();
                _items.Add(contentTypeId, duties);
            }
        }

        if (!_filteredItems.ContainsKey(contentTypeId))
        {
            var unfilteredList = _items[contentTypeId];
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

            _filteredItems.Add(contentTypeId, unfilteredList);
        }

        return _filteredItems[contentTypeId];
    }

    public override Dictionary<uint, string> Tabs => _tabs;

    public override string TableName => _tableName;

    public override bool UseClipper => _useClipper;
    public override string GenericKey => "duties";
    public override string GenericName => "Duties";
    public override bool DestroyOnClose => false;
    public override bool SaveState => true;
    public override Vector2? MaxSize { get; } = new(2000, 2000);
    public override Vector2? MinSize { get; } = new(200, 200);
    public override Vector2? DefaultSize { get; } = new(600, 600);
    public override void Draw()
    {
        DrawTabs();
    }

    public override int GetRowId(ContentFinderConditionEx item)
    {
        return (int)item.RowId;
    }

    public override Dictionary<uint, List<ContentFinderConditionEx>> Items => _items;

    public override Dictionary<uint, List<ContentFinderConditionEx>> FilteredItems => _filteredItems;

    public override List<TableColumn<ContentFinderConditionEx>> Columns => _columns;

    public override ImGuiTableFlags TableFlags => _flags;

    private List<TableColumn<ContentFinderConditionEx>> _columns = null!;
    private Dictionary<uint, List<ContentFinderConditionEx>> _items = null!;
    private Dictionary<uint, List<ContentFinderConditionEx>> _filteredItems = null!;
    private ImGuiTableFlags _flags = ImGuiTableFlags.RowBg | ImGuiTableFlags.BordersV |
                                                   ImGuiTableFlags.BordersOuterV | ImGuiTableFlags.BordersInnerV |
                                                   ImGuiTableFlags.BordersH | ImGuiTableFlags.BordersOuterH |
                                                   ImGuiTableFlags.BordersInnerH |
                                                   ImGuiTableFlags.Resizable | ImGuiTableFlags.Sortable |
                                                   ImGuiTableFlags.Hideable | ImGuiTableFlags.ScrollX |
                                                   ImGuiTableFlags.ScrollY;
    private Dictionary<uint, string> _tabs = null!;
    private string _tableName = "duties";
    private bool _useClipper = false;

    public override void Invalidate()
    {
    }

    public override FilterConfiguration? SelectedConfiguration => null;
}