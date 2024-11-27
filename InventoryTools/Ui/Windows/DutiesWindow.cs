using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using AllaganLib.GameSheets.Sheets;
using AllaganLib.GameSheets.Sheets.Rows;
using AllaganLib.Shared.Extensions;
using CriticalCommonLib.Services.Mediator;

using ImGuiNET;
using InventoryTools.Extensions;
using InventoryTools.Logic;
using InventoryTools.Mediator;
using InventoryTools.Services;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Ui;

public class DutiesWindow : GenericTabbedTable<ContentFinderConditionRow>, IMenuWindow
{
    private readonly ImGuiService _imGuiService;
    private readonly ExcelSheet<ContentType> _contentTypeSheet;
    private readonly ContentFinderConditionSheet _contentFinderConditionSheet;

    public DutiesWindow(ILogger<DutiesWindow> logger, MediatorService mediator, ImGuiService imGuiService, InventoryToolsConfiguration configuration, ExcelSheet<ContentType> contentTypeSheet, ContentFinderConditionSheet contentFinderConditionSheet, string name = "Duties Window") : base(logger, mediator, imGuiService, configuration, name)
    {
        _imGuiService = imGuiService;
        _contentTypeSheet = contentTypeSheet;
        _contentFinderConditionSheet = contentFinderConditionSheet;
    }
    public override void Initialize()
    {
        WindowName = "Duties";
        Key = "duties";

        _columns = new List<TableColumn<ContentFinderConditionRow>>()
        {
            new("Icon", 32, ImGuiTableColumnFlags.WidthFixed)
            {
                OnLeftClick = OnLeftClick,
                Draw = (ex, contentTypeId) =>
                {
                    if (ImGui.ImageButton(ImGuiService.GetIconTexture((int)ex.Base.ContentType.Value.IconDutyFinder).ImGuiHandle,
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

                    return specs == ImGuiSortDirection.Ascending ? exes.OrderBy(c => c.Base.ClassJobLevelRequired) : exes.OrderByDescending(c => c.Base.ClassJobLevelRequired);
                },
                Filter = (s, exes) =>
                {
                    if (s == null)
                    {
                        return exes;
                    }
                    return s == "" ? exes : exes.Where(c => ((int)c.Base.ClassJobLevelRequired).PassesFilter(s.ToLower()));
                },
                Draw = (ex, contentTypeId) =>
                {
                    ImGui.TextUnformatted(ex.Base.ClassJobLevelRequired.ToString());
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

                    return specs == ImGuiSortDirection.Ascending ? exes.OrderBy(c => c.Base.ClassJobLevelSync) : exes.OrderByDescending(c => c.Base.ClassJobLevelSync);
                },
                Filter = (s, exes) =>
                {
                    if (s == null)
                    {
                        return exes;
                    }
                    return s == "" ? exes : exes.Where(c => ((int)c.Base.ClassJobLevelSync).PassesFilter(s.ToLower()));
                },
                Draw = (ex, contentTypeId) =>
                {
                    ImGui.TextUnformatted(ex.Base.ClassJobLevelSync.ToString());
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

                    return specs == ImGuiSortDirection.Ascending ? exes.OrderBy(c => c.Base.ItemLevelRequired) : exes.OrderByDescending(c => c.Base.ItemLevelRequired);
                },
                Filter = (s, exes) =>
                {
                    if (s == null)
                    {
                        return exes;
                    }
                    return s == "" ? exes : exes.Where(c => ((int)c.Base.ItemLevelRequired).PassesFilter(s.ToLower()));
                },
                Draw = (ex, contentTypeId) =>
                {
                    ImGui.TextUnformatted(ex.Base.ItemLevelRequired.ToString());
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

                    return specs == ImGuiSortDirection.Ascending ? exes.OrderBy(c => c.Base.ItemLevelSync) : exes.OrderByDescending(c => c.Base.ItemLevelSync);
                },
                Filter = (s, exes) =>
                {
                    if (s == null)
                    {
                        return exes;
                    }
                    return s == "" ? exes : exes.Where(c => ((int)c.Base.ItemLevelSync).PassesFilter(s.ToLower()));
                },
                Draw = (ex, contentTypeId) =>
                {
                    ImGui.TextUnformatted(ex.Base.ItemLevelSync.ToString());
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

                    return specs == ImGuiSortDirection.Ascending ? exes.OrderBy(c => c.Base.AllowUndersized) : exes.OrderByDescending(c => c.Base.AllowUndersized);
                },
                FilterBool = (s, exes) =>
                {
                    if (s == null)
                    {
                        return exes;
                    }
                    return exes.Where(c => c.Base.AllowUndersized == s);
                },
                Draw = (ex, contentTypeId) =>
                {
                    ImGui.SetCursorPosX(ImGui.GetCursorPosX() + (ImGui.GetContentRegionAvail().X / 2) - RowSize / 2.0f);
                    _imGuiService.DrawUldIcon(ex.Base.AllowUndersized ? _imGuiService.TickIcon : _imGuiService.CrossIcon, new Vector2(RowSize, RowSize));
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

                    return specs == ImGuiSortDirection.Ascending ? exes.OrderBy(c => c.Base.AllowExplorerMode) : exes.OrderByDescending(c => c.Base.AllowExplorerMode);
                },
                FilterBool = (s, exes) =>
                {
                    if (s == null)
                    {
                        return exes;
                    }
                    return exes.Where(c => c.Base.AllowExplorerMode == s);
                },
                Draw = (ex, contentTypeId) =>
                {
                    ImGui.SetCursorPosX(ImGui.GetCursorPosX() + (ImGui.GetContentRegionAvail().X / 2) - RowSize / 2.0f);
                    _imGuiService.DrawUldIcon(ex.Base.AllowExplorerMode ? _imGuiService.TickIcon : _imGuiService.CrossIcon, new Vector2(RowSize, RowSize));
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

                    return specs == ImGuiSortDirection.Ascending ? exes.OrderBy(c => c.Base.PvP) : exes.OrderByDescending(c => c.Base.PvP);
                },
                FilterBool = (s, exes) =>
                {
                    if (s == null)
                    {
                        return exes;
                    }
                    return exes.Where(c => c.Base.PvP == s);
                },
                Draw = (ex, contentTypeId) =>
                {
                    ImGui.SetCursorPosX(ImGui.GetCursorPosX() + (ImGui.GetContentRegionAvail().X / 2) - RowSize / 2.0f);
                    _imGuiService.DrawUldIcon(ex.Base.PvP ? _imGuiService.TickIcon : _imGuiService.CrossIcon, new Vector2(RowSize, RowSize));
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

                    return specs == ImGuiSortDirection.Ascending ? exes.OrderBy(c => c.AcceptClassJobCategory?.Base.Name.ExtractText() ?? "Unknown") : exes.OrderByDescending(c => c.AcceptClassJobCategory?.Base.Name.ExtractText() ?? "Unknown");
                },
                Filter = (s, exes) =>
                {
                    if (s == null)
                    {
                        return exes;
                    }
                    return s == "" ? exes : exes.Where(c => (c.AcceptClassJobCategory?.Base.Name.ExtractText() ?? "Unknown").ToLower().PassesFilter(s.ToLower()));
                },
                Draw = (ex, contentTypeId) =>
                {
                    ImGui.TextUnformatted(ex.AcceptClassJobCategory?.Base.Name.ExtractText() ?? "Unknown");
                }
            },
        };
        _tabs = _contentTypeSheet.Where(c => c.Name.ExtractText().ToString() != "" && c.IconDutyFinder != 0).ToDictionary(c => c.RowId, c =>c.Name.ToString());
        _items = new Dictionary<uint, List<ContentFinderConditionRow>>();
        _filteredItems = new Dictionary<uint, List<ContentFinderConditionRow>>();
    }

    private bool OnLeftClick(ContentFinderConditionRow arg)
    {
        MediatorService.Publish(new OpenUintWindowMessage(typeof(DutyWindow), arg.RowId));
        return true;
    }

    public override List<ContentFinderConditionRow> GetItems(uint contentTypeId)
    {
        if (!_items.ContainsKey(contentTypeId))
        {
            if (contentTypeId == 0)
            {
                var duties = _contentFinderConditionSheet.Where(c => c.Base.Name.ExtractText() != "").ToList();
                _items.Add(contentTypeId, duties);
            }
            else
            {
                var duties = _contentFinderConditionSheet.Where(c => c.Base.Name.ExtractText() != "" && c.Base.ContentType.RowId == contentTypeId).ToList();
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

    public override int GetRowId(ContentFinderConditionRow item)
    {
        return (int)item.RowId;
    }

    public override Dictionary<uint, List<ContentFinderConditionRow>> Items => _items;

    public override Dictionary<uint, List<ContentFinderConditionRow>> FilteredItems => _filteredItems;

    public override List<TableColumn<ContentFinderConditionRow>> Columns => _columns;

    public override ImGuiTableFlags TableFlags => _flags;

    private List<TableColumn<ContentFinderConditionRow>> _columns = null!;
    private Dictionary<uint, List<ContentFinderConditionRow>> _items = null!;
    private Dictionary<uint, List<ContentFinderConditionRow>> _filteredItems = null!;
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