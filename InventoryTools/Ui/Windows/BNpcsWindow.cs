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
using Lumina.Excel;
using OtterGui;
using Dalamud.Interface.Utility.Raii;
using InventoryTools.Mediator;
using InventoryTools.Services;
using InventoryTools.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Ui;

public class BNpcsWindow : GenericTabbedTable<BNpcNameEx>, IMenuWindow
{
    private readonly IChatUtilities _chatUtilities;
    private readonly ExcelCache _excelCache;

    public BNpcsWindow(ILogger<BNpcsWindow> logger, MediatorService mediator, ImGuiService imGuiService, InventoryToolsConfiguration configuration, IChatUtilities chatUtilities, ExcelCache excelCache,  string name = "Mobs Window") : base(logger, mediator, imGuiService, configuration, name)
    {
        _chatUtilities = chatUtilities;
        _excelCache = excelCache;
    }
    public override void Initialize()
    {
        WindowName = "Mobs";
        Key = "mobs";
        var mobSpawns = _excelCache.MobSpawns ?? new List<MobSpawnPositionEx>();
        var availableTerritories = mobSpawns.Select(c => c.TerritoryTypeId).ToHashSet();
        _mappedMobs = mobSpawns.Select(c => (c.BNpcNameId, c.TerritoryTypeId)).GroupBy(c => c.BNpcNameId).ToDictionary(c => c.Key, c => c.Select(c => c.TerritoryTypeId).ToHashSet());
        _columns = new List<TableColumn<BNpcNameEx>>()
        {
            new("Icon", 32, ImGuiTableColumnFlags.WidthFixed)
            {
                OnLeftClick = OnLeftClick,
                Draw = (ex, contentTypeId) =>
                {
                    if (ImGui.ImageButton(ImGuiService.GetIconTexture(Icons.MobIcon).ImGuiHandle,
                            new Vector2(RowSize, RowSize)))
                    {
                        _columns[0].OnLeftClick?.Invoke(ex);
                    }
                }
            },
            new("ID", 50, ImGuiTableColumnFlags.WidthFixed | ImGuiTableColumnFlags.DefaultHide)
            {
                Sort = (specs, exes) =>
                {
                    if (specs == null)
                    {
                        return exes;
                    }

                    return specs == ImGuiSortDirection.Ascending ? exes.OrderBy(c => c.RowId) : exes.OrderByDescending(c => c.RowId);
                },
                Filter = (s, exes) =>
                {
                    if (s == null)
                    {
                        return exes;
                    }
                    return s == "" ? exes : exes.Where(c => c.RowId.ToString().ToLower().PassesFilter(s.ToLower()));
                },
                Draw = (ex, contentTypeId) =>
                {
                    ImGui.TextUnformatted(ex.RowId.ToString());
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
            new("Type", 70, ImGuiTableColumnFlags.WidthFixed)
            {
                Sort = (specs, exes) =>
                {
                    if (specs == null)
                    {
                        return exes;
                    }

                    return specs == ImGuiSortDirection.Ascending ? exes.OrderBy(c => c.MobTypes) : exes.OrderByDescending(c => c.MobTypes);
                },
                Filter = (s, exes) =>
                {
                    if (s == null)
                    {
                        return exes;
                    }
                    return s == "" ? exes : exes.Where(c => c.MobTypes.ToLower().PassesFilter(s.ToLower()));
                },
                Draw = (ex, contentTypeId) =>
                {
                    ImGui.TextUnformatted(ex.MobTypes ?? "");
                }
            },
            new("Locations", 200, ImGuiTableColumnFlags.WidthFixed | ImGuiTableColumnFlags.NoSort)
            {
                Draw = (ex, contentTypeId) =>
                {
                    var positions = GetPositions(ex.RowId).Where(c => c.TerritoryType.Value != null && c.TerritoryType.Value.PlaceName.Value != null && (c.TerritoryTypeId == contentTypeId || contentTypeId == 0));
                    ImGuiService.WrapTableColumnElements("Scroll" + ex.RowId,positions, RowSize * ImGui.GetIO().FontGlobalScale - ImGui.GetStyle().FramePadding.X, position =>
                    {
                        var territory = _excelCache.GetTerritoryTypeExSheet()
                            .GetRow(position.TerritoryTypeId);
                        if (territory != null)
                        {
                            if (ImGui.ImageButton(ImGuiService.GetIconTexture(60561).ImGuiHandle,
                                    new Vector2(RowSize * ImGui.GetIO().FontGlobalScale,
                                        RowSize * ImGui.GetIO().FontGlobalScale), new Vector2(0, 0),
                                    new Vector2(1, 1), 0))
                            {
                                _chatUtilities.PrintFullMapLink(
                                    new GenericMapLocation(position.Position.X, position.Position.Y,
                                        territory.MapEx,
                                        territory.PlaceNameEx,
                                        new LazyRow<TerritoryTypeEx>(_excelCache.GameData, territory.RowId,
                                            territory.SheetLanguage)), ex.FormattedName);
                            }

                            if (ImGui.IsItemHovered())
                            {
                                using var tt = ImRaii.Tooltip();
                                ImGui.TextUnformatted((territory.PlaceName.Value?.Name ?? "Unknown") + " - " +
                                                      position.Position.X +
                                                      " : " + position.Position.Y);
                            }
                        }

                        return true;
                    });

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
                        var currentValue = GetDrops(c.RowId);

                        return currentValue.Any(c => c.NameString.ToLower().PassesFilter(s));
                    });
                },
                Draw = (ex, contentTypeId) =>
                {
                    var drops = GetDrops(ex.RowId);
                    ImGuiService.WrapTableColumnElements("ScrollDrops" + ex.RowId, drops,
                        RowSize * ImGui.GetIO().FontGlobalScale - ImGui.GetStyle().FramePadding.X,
                        drop =>
                    {
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
                            MediatorService.Publish(new OpenUintWindowMessage(typeof(ItemWindow), drop.RowId));
                        }

                        if (ImGui.BeginPopup("RightClick" + drop.RowId))
                        {
                            MediatorService.Publish(ImGuiService.RightClickService.DrawRightClickPopup(drop));
                            ImGui.EndPopup();
                        }


                        ImGuiUtil.HoverTooltip(drop.NameString);
                        return true;
                    });
                }
            },
        };
        _tabs = _excelCache.GetTerritoryTypeExSheet().Where(c => availableTerritories.Contains(c.RowId)).OrderBy(c => c.PlaceNameEx.Value?.FormattedName ?? "Unknown").ToDictionary(c => c.RowId, c =>c.PlaceNameEx.Value?.FormattedName ?? "Unknown");
        _items = new Dictionary<uint, List<BNpcNameEx>>();
        _filteredItems = new Dictionary<uint, List<BNpcNameEx>>();
    }
    public HashSet<uint>? GetTerritory(uint bNpcNameId)
    {
        if (_mappedMobs.ContainsKey(bNpcNameId))
        {
            return _mappedMobs[bNpcNameId];
        }

        return null;
    }

    private bool OnLeftClick(BNpcNameEx arg)
    {
        MediatorService.Publish(new OpenUintWindowMessage(typeof(BNpcWindow), arg.RowId));
        return true;
    }

    private Dictionary<uint, List<MobSpawnPositionEx>> _spawnPositions = new Dictionary<uint, List<MobSpawnPositionEx>>();
    private Dictionary<uint, List<ItemEx>> _mobDrops = new Dictionary<uint, List<ItemEx>>();

    public List<MobSpawnPositionEx> GetPositions(uint bNpcNameId)
    {
        if (_spawnPositions.ContainsKey(bNpcNameId))
        {
            return _spawnPositions[bNpcNameId];
        }

        var spawns = _excelCache.MobSpawns?.Where(c => c.BNpcNameId == bNpcNameId).ToList() ?? new List<MobSpawnPositionEx>();
        _spawnPositions[bNpcNameId] = spawns;
        return _spawnPositions[bNpcNameId];
    }

    public List<ItemEx> GetDrops(uint bNpcNameId)
    {
        if (_mobDrops.ContainsKey(bNpcNameId))
        {
            return _mobDrops[bNpcNameId];
        }

        var mobDrops = _excelCache.MobDrops?.Where(c => c.BNpcNameId == bNpcNameId).Select(c => _excelCache.GetItemExSheet().GetRow(c.ItemId)).Where(c => c != null).Select(c => c!).ToList() ?? new List<ItemEx>();
        _mobDrops[bNpcNameId] = mobDrops;
        return _mobDrops[bNpcNameId];
    }

    public override List<BNpcNameEx> GetItems(uint placeNameId)
    {
        if (_excelCache.MobSpawns == null)
        {
            return new();
        }
        if (!_items.ContainsKey(placeNameId))
        {
            if (placeNameId == 0)
            {
                var availableMobs = _excelCache.MobSpawns.Select(c => c.BNpcNameId).ToHashSet();
                var actualMobs = availableMobs.Select(c => _excelCache.GetBNpcNameExSheet().GetRow(c)).ToList();
                _items.Add(placeNameId, actualMobs!);
            }
            else
            {
                bool FilterNpcs(BNpcNameEx c)
                {
                    var territory = GetTerritory(c.RowId);
                    return c.FormattedName != "" && territory != null && territory.Contains(placeNameId);
                }
                var availableMobs = _excelCache.MobSpawns.Select(c => c.BNpcNameId).ToHashSet();
                var actualMobs = availableMobs.Select(c => _excelCache.GetBNpcNameExSheet().GetRow(c)).Where(c => c != null && FilterNpcs(c) && c.FormattedName != "").ToList();
                _items.Add(placeNameId, actualMobs!);
            }
        }

        if (!_filteredItems.ContainsKey(placeNameId))
        {
            var unfilteredList = _items[placeNameId];
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

            _filteredItems.Add(placeNameId, unfilteredList);
        }

        return _filteredItems[placeNameId];
    }

    public override Dictionary<uint, string> Tabs => _tabs;

    public override string TableName => _tableName;

    public override string GenericKey => "mobs";
    public override string GenericName => "Mobs";
    public override bool DestroyOnClose => false;
    public override bool SaveState => true;
    public override Vector2? MaxSize { get; } = new(2000, 2000);
    public override Vector2? MinSize { get; } = new(200, 200);
    public override Vector2? DefaultSize { get; } = new(600, 600);

    public override void Draw()
    {
        DrawTabs();
    }

    public override int GetRowId(BNpcNameEx item)
    {
        return item.GetHashCode();
    }

    public override Dictionary<uint, List<BNpcNameEx>> Items => _items;

    public override Dictionary<uint, List<BNpcNameEx>> FilteredItems => _filteredItems;

    public override List<TableColumn<BNpcNameEx>> Columns => _columns;

    public override ImGuiTableFlags TableFlags => _flags;

    public override bool UseClipper => _useClipper;

    private List<TableColumn<BNpcNameEx>> _columns = null!;
    private Dictionary<uint, List<BNpcNameEx>> _items = null!;
    private Dictionary<uint, List<BNpcNameEx>> _filteredItems = null!;
    private ImGuiTableFlags _flags = ImGuiTableFlags.RowBg | ImGuiTableFlags.BordersV |
                                                   ImGuiTableFlags.BordersOuterV | ImGuiTableFlags.BordersInnerV |
                                                   ImGuiTableFlags.BordersH | ImGuiTableFlags.BordersOuterH |
                                                   ImGuiTableFlags.BordersInnerH |
                                                   ImGuiTableFlags.Resizable | ImGuiTableFlags.Sortable |
                                                   ImGuiTableFlags.Hideable | ImGuiTableFlags.ScrollX |
                                                   ImGuiTableFlags.ScrollY;
    private Dictionary<uint, string> _tabs = null!;
    private string _tableName = "bnpc";
    private Dictionary<uint, HashSet<uint>> _mappedMobs = null!;
    private bool _useClipper => true;

    public override void Invalidate()
    {
    }

    public override FilterConfiguration? SelectedConfiguration => null;
}