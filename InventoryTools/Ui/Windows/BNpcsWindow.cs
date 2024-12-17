using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using AllaganLib.GameSheets.Sheets;
using AllaganLib.GameSheets.Sheets.Rows;
using AllaganLib.Shared.Extensions;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using CriticalCommonLib.Services.Mediator;

using ImGuiNET;
using InventoryTools.Extensions;
using InventoryTools.Logic;
using OtterGui;
using Dalamud.Interface.Utility.Raii;
using InventoryTools.Mediator;
using InventoryTools.Services;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using LuminaSupplemental.Excel.Model;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Ui;

public class BNpcsWindow : GenericTabbedTable<BNpcNameRow>, IMenuWindow
{
    private readonly IChatUtilities _chatUtilities;
    private readonly List<MobSpawnPosition> _mobSpawnPositions;
    private readonly List<MobDrop> _mobDrops;
    private readonly TerritoryTypeSheet _territoryTypeSheet;
    private readonly ItemSheet _itemSheet;
    private readonly BNpcNameSheet _bnpcNameSheet;

    public BNpcsWindow(ILogger<BNpcsWindow> logger,
        MediatorService mediator,
        ImGuiService imGuiService,
        InventoryToolsConfiguration configuration,
        IChatUtilities chatUtilities,
        List<MobSpawnPosition> mobSpawnPositions,
        List<MobDrop> mobDrops,
        TerritoryTypeSheet territoryTypeSheet,
        ItemSheet itemSheet,
        BNpcNameSheet bnpcNameSheet,
        string name = "Mobs Window") : base(logger,
        mediator,
        imGuiService,
        configuration,
        name)
    {
        _chatUtilities = chatUtilities;
        _mobSpawnPositions = mobSpawnPositions;
        _mobDrops = mobDrops;
        _territoryTypeSheet = territoryTypeSheet;
        _itemSheet = itemSheet;
        _bnpcNameSheet = bnpcNameSheet;
    }
    public override void Initialize()
    {
        WindowName = "Mobs";
        Key = "mobs";
        var mobSpawns = _mobSpawnPositions;
        var availableTerritories = mobSpawns.Select(c => c.TerritoryTypeId).ToHashSet();
        _mappedMobs = mobSpawns.Select(c => (c.BNpcNameId, c.TerritoryTypeId)).GroupBy(c => c.BNpcNameId).ToDictionary(c => c.Key, c => c.Select(c => c.TerritoryTypeId).ToHashSet());
        _columns = new List<TableColumn<BNpcNameRow>>()
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

                    return specs == ImGuiSortDirection.Ascending ? exes.OrderBy(c => c.Base.Singular.ExtractText()) : exes.OrderByDescending(c => c.Base.Singular.ExtractText());
                },
                Filter = (s, exes) =>
                {
                    if (s == null)
                    {
                        return exes;
                    }
                    return s == "" ? exes : exes.Where(c => c.Base.Singular.ExtractText().ToLower().PassesFilter(s.ToLower()));
                },
                Draw = (ex, contentTypeId) =>
                {
                    ImGui.TextUnformatted(ex.Base.Singular.ExtractText());
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
                    return s == "" ? exes : exes.Where(c => String.Join(",", c.MobTypes.Select(d => d.ToString())).ToLower().PassesFilter(s.ToLower()));
                },
                Draw = (ex, contentTypeId) =>
                {
                    ImGui.TextUnformatted(String.Join(",", ex.MobTypes.Select(d => d.ToString())));
                }
            },
            new("Locations", 200, ImGuiTableColumnFlags.WidthFixed | ImGuiTableColumnFlags.NoSort)
            {
                Draw = (ex, contentTypeId) =>
                {
                    var positions = GetPositions(ex.RowId).Where(c => c.TerritoryType.IsValid && c.TerritoryType.Value.PlaceName.IsValid && (c.TerritoryTypeId == contentTypeId || contentTypeId == 0));
                    ImGuiService.WrapTableColumnElements("Scroll" + ex.RowId,positions, RowSize * ImGui.GetIO().FontGlobalScale - ImGui.GetStyle().FramePadding.X, position =>
                    {
                        var territory = _territoryTypeSheet
                            .GetRowOrDefault(position.TerritoryTypeId);
                        if (territory != null)
                        {
                            if (ImGui.ImageButton(ImGuiService.GetIconTexture(60561).ImGuiHandle,
                                    new Vector2(RowSize * ImGui.GetIO().FontGlobalScale,
                                        RowSize * ImGui.GetIO().FontGlobalScale), new Vector2(0, 0),
                                    new Vector2(1, 1), 0))
                            {
                                _chatUtilities.PrintFullMapLink(
                                    new GenericMapLocation(position.Position.X, position.Position.Y,
                                        territory.Base.Map,
                                        territory.Base.PlaceName,
                                        territory.RowRef), ex.Base.Singular.ExtractText());
                            }

                            if (ImGui.IsItemHovered())
                            {
                                using var tt = ImRaii.Tooltip();
                                ImGui.TextUnformatted((territory.Base.PlaceName.ValueNullable?.Name.ExtractText() ?? "Unknown") + " - " +
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
                            MediatorService.Publish(ImGuiService.ImGuiMenuService.DrawRightClickPopup(drop));
                            ImGui.EndPopup();
                        }


                        ImGuiUtil.HoverTooltip(drop.NameString);
                        return true;
                    });
                }
            },
        };
        _tabs = _territoryTypeSheet.Where(c => availableTerritories.Contains(c.RowId)).OrderBy(c => c.Base.PlaceName.ValueNullable?.Name.ExtractText() ?? "Unknown").ToDictionary(c => c.RowId, c =>c.Base.PlaceName.ValueNullable?.Name.ExtractText() ?? "Unknown");
        _items = new Dictionary<uint, List<BNpcNameRow>>();
        _filteredItems = new Dictionary<uint, List<BNpcNameRow>>();
    }
    public HashSet<uint>? GetTerritory(uint bNpcNameId)
    {
        if (_mappedMobs.ContainsKey(bNpcNameId))
        {
            return _mappedMobs[bNpcNameId];
        }

        return null;
    }

    private bool OnLeftClick(BNpcNameRow arg)
    {
        MediatorService.Publish(new OpenUintWindowMessage(typeof(BNpcWindow), arg.RowId));
        return true;
    }

    private Dictionary<uint, List<MobSpawnPosition>> _spawnPositions = new Dictionary<uint, List<MobSpawnPosition>>();
    private Dictionary<uint, List<ItemRow>> mobDropLookup = new Dictionary<uint, List<ItemRow>>();

    public List<MobSpawnPosition> GetPositions(uint bNpcNameId)
    {
        if (_spawnPositions.ContainsKey(bNpcNameId))
        {
            return _spawnPositions[bNpcNameId];
        }

        var spawns = _mobSpawnPositions.Where(c => c.BNpcNameId == bNpcNameId).ToList() ?? new List<MobSpawnPosition>();
        _spawnPositions[bNpcNameId] = spawns;
        return _spawnPositions[bNpcNameId];
    }

    public List<ItemRow> GetDrops(uint bNpcNameId)
    {
        if (mobDropLookup.ContainsKey(bNpcNameId))
        {
            return mobDropLookup[bNpcNameId];
        }

        var mobDrops = _mobDrops.Where(c => c.BNpcNameId == bNpcNameId).Select(c => _itemSheet.GetRowOrDefault(c.ItemId)).Where(c => c != null).Select(c => c!).ToList() ?? new List<ItemRow>();
        mobDropLookup[bNpcNameId] = mobDrops;
        return mobDropLookup[bNpcNameId];
    }

    public override List<BNpcNameRow> GetItems(uint placeNameId)
    {
        if (!_items.ContainsKey(placeNameId))
        {
            if (placeNameId == 0)
            {
                var availableMobs = _mobSpawnPositions.DistinctBy(c => c.BNpcNameId).ToHashSet();
                var actualMobs = availableMobs.Select(c => _bnpcNameSheet.GetRowOrDefault(c.BNpcNameId)).ToList();
                _items.Add(placeNameId, actualMobs!);
            }
            else
            {
                bool FilterNpcs(BNpcNameRow c)
                {
                    var territory = GetTerritory(c.RowId);
                    return c.Base.Singular.ExtractText() != "" && territory != null && territory.Contains(placeNameId);
                }
                var availableMobs = _mobSpawnPositions.DistinctBy(c => c.BNpcNameId).ToHashSet();
                var actualMobs = availableMobs.Select(c => _bnpcNameSheet.GetRowOrDefault(c.BNpcNameId)).Where(c => c != null && FilterNpcs(c) && c.Base.Singular.ExtractText() != "").ToList();
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

    public override int GetRowId(BNpcNameRow item)
    {
        return item.GetHashCode();
    }

    public override Dictionary<uint, List<BNpcNameRow>> Items => _items;

    public override Dictionary<uint, List<BNpcNameRow>> FilteredItems => _filteredItems;

    public override List<TableColumn<BNpcNameRow>> Columns => _columns;

    public override ImGuiTableFlags TableFlags => _flags;

    public override bool UseClipper => _useClipper;

    private List<TableColumn<BNpcNameRow>> _columns = null!;
    private Dictionary<uint, List<BNpcNameRow>> _items = null!;
    private Dictionary<uint, List<BNpcNameRow>> _filteredItems = null!;
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