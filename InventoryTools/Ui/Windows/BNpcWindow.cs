using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using CriticalCommonLib;
using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using Dalamud.Utility;
using ImGuiNET;
using InventoryTools.Extensions;
using InventoryTools.Images;
using InventoryTools.Logic;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;
using LuminaSupplemental.Excel.Model;
using OtterGui;
using OtterGui.Raii;

namespace InventoryTools.Ui;

public class BNpcWindow : GenericTabbedTable<(BNpcNameEx, BNpcBaseEx)>
{
    public BNpcWindow(string name = "Allagan Tools - Mobs", ImGuiWindowFlags flags = ImGuiWindowFlags.None, bool forceMainWindow = false) : base(name, flags, forceMainWindow)
    {
        SetupWindow();
    }

    public BNpcWindow() : base("Allagan Tools - Mobs")
    {
        SetupWindow();
    }

    public HashSet<uint>? GetTerritory(uint bNpcNameId)
    {
        if (_mappedMobs.ContainsKey(bNpcNameId))
        {
            return _mappedMobs[bNpcNameId];
        }

        return null;
    }

    private Dictionary<uint, List<MobSpawnPositionEx>> _spawnPositions = new Dictionary<uint, List<MobSpawnPositionEx>>();
    private Dictionary<uint, List<ItemEx>> _mobDrops = new Dictionary<uint, List<ItemEx>>();

    public List<MobSpawnPositionEx> GetPositions(uint bNpcNameId)
    {
        if (_spawnPositions.ContainsKey(bNpcNameId))
        {
            return _spawnPositions[bNpcNameId];
        }

        var spawns = Service.ExcelCache.MobSpawns.Where(c => c.BNpcNameId == bNpcNameId).ToList();
        _spawnPositions[bNpcNameId] = spawns;
        return _spawnPositions[bNpcNameId];
    }

    public List<ItemEx> GetDrops(uint bNpcNameId)
    {
        if (_mobDrops.ContainsKey(bNpcNameId))
        {
            return _mobDrops[bNpcNameId];
        }

        var mobDrops = Service.ExcelCache.MobDrops.Where(c => c.BNpcNameId == bNpcNameId).Select(c => Service.ExcelCache.GetItemExSheet().GetRow(c.ItemId)).Where(c => c != null).Select(c => c!).ToList();
        _mobDrops[bNpcNameId] = mobDrops;
        return _mobDrops[bNpcNameId];
    }
    
    public void SetupWindow()
    {
        var availableTerritories = Service.ExcelCache.MobSpawns.Select(c => c.TerritoryTypeId).ToHashSet();
        _mappedMobs = Service.ExcelCache.MobSpawns.Select(c => (c.BNpcNameId, c.TerritoryTypeId)).GroupBy(c => c.BNpcNameId).ToDictionary(c => c.Key, c => c.Select(c => c.TerritoryTypeId).ToHashSet());
        _columns = new List<TableColumn<(BNpcNameEx, BNpcBaseEx)>>()
        {
            new("ID", 50, ImGuiTableColumnFlags.WidthFixed | ImGuiTableColumnFlags.DefaultHide)
            {
                Sort = (specs, exes) =>
                {
                    if (specs == null)
                    {
                        return exes;
                    }

                    return specs == ImGuiSortDirection.Ascending ? exes.OrderBy(c => c.Item1.RowId) : exes.OrderByDescending(c => c.Item1.RowId);
                },
                Filter = (s, exes) =>
                {
                    if (s == null)
                    {
                        return exes;
                    }
                    return s == "" ? exes : exes.Where(c => (c.Item1.RowId + "," + c.Item2.RowId).ToLower().PassesFilter(s.ToLower()));
                },
                Draw = (ex, contentTypeId) =>
                {
                    ImGui.TextUnformatted((ex.Item1.RowId + "," + ex.Item2.RowId).ToString());
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

                    return specs == ImGuiSortDirection.Ascending ? exes.OrderBy(c => c.Item1.FormattedName) : exes.OrderByDescending(c => c.Item1.FormattedName);
                },
                Filter = (s, exes) =>
                {
                    if (s == null)
                    {
                        return exes;
                    }
                    return s == "" ? exes : exes.Where(c => c.Item1.FormattedName.ToLower().PassesFilter(s.ToLower()));
                },
                Draw = (ex, contentTypeId) =>
                {
                    ImGui.TextUnformatted(ex.Item1.FormattedName);
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

                    return specs == ImGuiSortDirection.Ascending ? exes.OrderBy(c => c.Item2.NpcType.ToString()) : exes.OrderByDescending(c => c.Item2.NpcType.ToString());
                },
                Filter = (s, exes) =>
                {
                    if (s == null)
                    {
                        return exes;
                    }
                    return s == "" ? exes : exes.Where(c => c.Item2.NpcType.ToString().ToLower().PassesFilter(s.ToLower()));
                },
                Draw = (ex, contentTypeId) =>
                {
                    ImGui.TextUnformatted(ex.Item2.NpcType.ToString().ToString() ?? "");
                }
            },
            new("Locations", 200, ImGuiTableColumnFlags.WidthFixed | ImGuiTableColumnFlags.NoSort)
            {
                Draw = (ex, contentTypeId) =>
                {
                    var positions = GetPositions(ex.Item1.RowId).Where(c => c.TerritoryType.Value != null && c.TerritoryType.Value.PlaceName.Value != null && (c.TerritoryTypeId == contentTypeId || contentTypeId == 0));
                    UiHelpers.WrapTableColumnElements("Scroll" + ex.Item1.RowId,positions, RowSize * ImGui.GetIO().FontGlobalScale - ImGui.GetStyle().FramePadding.X, position =>
                    {
                        var territory = Service.ExcelCache.GetTerritoryTypeExSheet()
                            .GetRow(position.TerritoryTypeId);
                        if (ImGui.ImageButton(PluginService.IconStorage[60561].ImGuiHandle,
                                new Vector2(RowSize * ImGui.GetIO().FontGlobalScale,
                                    RowSize * ImGui.GetIO().FontGlobalScale), new Vector2(0, 0),
                                new Vector2(1, 1), 0))
                        {
                            PluginService.ChatUtilities.PrintFullMapLink(
                                new GenericMapLocation(position.Position.X, position.Position.Y,
                                    territory.MapEx,
                                    territory.PlaceNameEx), ex.Item1.FormattedName);
                        }

                        if (ImGui.IsItemHovered())
                        {
                            using var tt = ImRaii.Tooltip();
                            ImGui.TextUnformatted(territory.PlaceName.Value.Name + " - " +
                                                  position.Position.X +
                                                  " : " + position.Position.Y);
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
                Draw = (ex, contentTypeId) =>
                {
                    var drops = GetDrops(ex.Item1.RowId);
                    UiHelpers.WrapTableColumnElements("ScrollDrops" + ex.Item1.RowId, drops,
                        RowSize * ImGui.GetIO().FontGlobalScale - ImGui.GetStyle().FramePadding.X,
                        drop =>
                    {
                        var sourceIcon = PluginService.IconStorage[drop.Icon];
                        ImGui.Image(sourceIcon.ImGuiHandle,
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
                            PluginService.WindowService.OpenItemWindow(drop.RowId);
                        }

                        if (ImGui.BeginPopup("RightClick" + drop.RowId))
                        {
                            drop.DrawRightClickPopup();
                            ImGui.EndPopup();
                        }


                        ImGuiUtil.HoverTooltip(drop.NameString);
                        return true;
                    });
                }
            },
        };
        _tabs = Service.ExcelCache.GetTerritoryTypeExSheet().Where(c => availableTerritories.Contains(c.RowId)).ToDictionary(c => c.RowId, c =>c.PlaceName.Value?.Name.ToString() ?? "Unknown");
        _items = new Dictionary<uint, List<(BNpcNameEx, BNpcBaseEx)>>();
        _filteredItems = new Dictionary<uint, List<(BNpcNameEx, BNpcBaseEx)>>();
    }

    public override List<(BNpcNameEx, BNpcBaseEx)> GetItems(uint placeNameId)
    {
        if (!_items.ContainsKey(placeNameId))
        {
            if (placeNameId == 0)
            {
                var availableMobs = Service.ExcelCache.MobSpawns.Select(c => (c.BNpcNameId, c.BNpcBaseId)).ToHashSet();
                var actualMobs = availableMobs.Select(c => (Service.ExcelCache.GetBNpcNameExSheet().GetRow(c.BNpcNameId),
                    Service.ExcelCache.GetBNpcBaseExSheet().GetRow(c.BNpcBaseId))).Where(c => c.Item1 != null && c.Item1.FormattedName != "" && c.Item2 != null).Select(c => (c.Item1!, c.Item2!)).ToList();
                _items.Add(placeNameId, actualMobs);
            }
            else
            {
                bool FilterNpcs(BNpcNameEx c)
                {
                    var territory = GetTerritory(c.RowId);
                    return c.FormattedName != "" && territory != null && territory.Contains(placeNameId);
                }
                var availableMobs = Service.ExcelCache.MobSpawns.Select(c => (c.BNpcNameId, c.BNpcBaseId)).ToHashSet();
                var actualMobs = availableMobs.Select(c => (Service.ExcelCache.GetBNpcNameExSheet().GetRow(c.BNpcNameId),
                    Service.ExcelCache.GetBNpcBaseExSheet().GetRow(c.BNpcBaseId))).Where(c => c.Item1 != null && FilterNpcs(c.Item1) && c.Item1.FormattedName != "" && c.Item2 != null).Select(c => (c.Item1!, c.Item2!)).ToList();
                _items.Add(placeNameId, actualMobs);
            }
        }

        if (!_filteredItems.ContainsKey(placeNameId))
        {
            var unfilteredList = _items[placeNameId];
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

            _filteredItems.Add(placeNameId, unfilteredList);
        }

        return _filteredItems[placeNameId];
    }

    public override Dictionary<uint, string> Tabs => _tabs;

    public override string TableName => _tableName;

    public static string AsKey
    {
        get { return "mobs"; }
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

    public override int GetRowId((BNpcNameEx, BNpcBaseEx) item)
    {
        return item.GetHashCode();
    }

    public override Dictionary<uint, List<(BNpcNameEx, BNpcBaseEx)>> Items => _items;

    public override Dictionary<uint, List<(BNpcNameEx, BNpcBaseEx)>> FilteredItems => _filteredItems;

    public override List<TableColumn<(BNpcNameEx, BNpcBaseEx)>> Columns => _columns;

    public override ImGuiTableFlags TableFlags => _flags;

    public override bool UseClipper => _useClipper;

    private List<TableColumn<(BNpcNameEx, BNpcBaseEx)>> _columns;
    private Dictionary<uint, List<(BNpcNameEx, BNpcBaseEx)>> _items;
    private Dictionary<uint, List<(BNpcNameEx, BNpcBaseEx)>> _filteredItems;
    private ImGuiTableFlags _flags = ImGuiTableFlags.RowBg | ImGuiTableFlags.BordersV |
                                                   ImGuiTableFlags.BordersOuterV | ImGuiTableFlags.BordersInnerV |
                                                   ImGuiTableFlags.BordersH | ImGuiTableFlags.BordersOuterH |
                                                   ImGuiTableFlags.BordersInnerH |
                                                   ImGuiTableFlags.Resizable | ImGuiTableFlags.Sortable |
                                                   ImGuiTableFlags.Hideable | ImGuiTableFlags.ScrollX |
                                                   ImGuiTableFlags.ScrollY;
    private Dictionary<uint, string> _tabs;
    private string _tableName;
    private Dictionary<uint, HashSet<uint>> _mappedMobs;
    private bool _useClipper => true;

    public override void Invalidate()
    {
    }

    public override FilterConfiguration? SelectedConfiguration => null;

}