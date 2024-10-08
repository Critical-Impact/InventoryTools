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
using Lumina.Excel;
using OtterGui;
using Dalamud.Interface.Utility.Raii;
using InventoryTools.Mediator;
using InventoryTools.Services;
using InventoryTools.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Ui;

public class ENpcsWindow : GenericTabbedTable<ENpc>, IMenuWindow
{
    private readonly ExcelCache _excelCache;
    private readonly IChatUtilities _chatUtilities;

    public ENpcsWindow(ILogger<ENpcsWindow> logger, MediatorService mediator, ImGuiService imGuiService, InventoryToolsConfiguration configuration,ExcelCache excelCache, IChatUtilities chatUtilities, string name = "NPCs Window") : base(logger, mediator, imGuiService, configuration, name)
    {
        _excelCache = excelCache;
        _chatUtilities = chatUtilities;
    }

    public override void Initialize()
    {
        WindowName = "NPCs";
        Key = "enpcs";
         _columns = new List<TableColumn<ENpc>>()
        {
            new("Icon", 32, ImGuiTableColumnFlags.WidthFixed)
            {
                OnLeftClick = OnLeftClick,
                Draw = (ex, contentTypeId) =>
                {
                    if (ImGui.ImageButton(
                            ImGuiService.GetIconTexture(62043).ImGuiHandle,
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

                    return specs == ImGuiSortDirection.Ascending
                        ? exes.OrderBy(c => c.Resident?.FormattedSingular ?? "")
                        : exes.OrderByDescending(c => c.Resident?.FormattedSingular ?? "");
                },
                Filter = (s, exes) =>
                {
                    if (s == null)
                    {
                        return exes;
                    }

                    return s == "" ? exes : exes.Where(c => (c.Resident?.FormattedSingular ?? "").ToLower().PassesFilter(s.ToLower()));
                },
                Draw = (ex, contentTypeId) =>
                {
                    ImGui.TextUnformatted(ex.Resident?.FormattedSingular ?? "");
                }
            },
            new("Locations", 200, ImGuiTableColumnFlags.WidthFixed | ImGuiTableColumnFlags.NoSort)
            {
                Draw = (ex, contentTypeId) =>
                {
                    var positions = ex.Locations;
                    ImGuiService.WrapTableColumnElements("Scroll" + ex.Key,positions, RowSize * ImGui.GetIO().FontGlobalScale - ImGui.GetStyle().FramePadding.X, position =>
                    {
                        if (ImGui.ImageButton(ImGuiService.GetIconTexture(60561).ImGuiHandle,
                                new Vector2(RowSize * ImGui.GetIO().FontGlobalScale,
                                    RowSize * ImGui.GetIO().FontGlobalScale), new Vector2(0, 0),
                                new Vector2(1, 1), 0))
                        {
                            _chatUtilities.PrintFullMapLink(position, ex.Resident?.FormattedSingular ?? "");
                        }

                        if (ImGui.IsItemHovered())
                        {
                            using var tt = ImRaii.Tooltip();
                            ImGui.TextUnformatted((position.PlaceNameEx.Value?.FormattedName ?? "Unknown") + " - " +
                                                  position.MapX +
                                                  " : " + position.MapY);
                        }

                        return true;
                    });

                }
            },
            new("Is Vendor", 200, ImGuiTableColumnFlags.WidthFixed | ImGuiTableColumnFlags.NoSort)
            {
                FilterBool = (s, exes) =>
                {
                    if (s == null)
                    {
                        return exes;
                    }

                    return exes.Where(c => c.IsVendor && s == true || !c.IsVendor && s == false);
                },
                FilterBoolean = null,
                Draw = (ex, contentTypeId) =>
                {
                    if (ex.IsVendor)
                    {
                        ImGui.Text("Yes");
                    }
                    else
                    {
                        ImGui.Text("No");
                    }
                }
            },
            new("Vendor Items", 200, ImGuiTableColumnFlags.WidthFixed | ImGuiTableColumnFlags.NoSort)
            {
                Sort = (specs, exes) =>
                {
                    if (specs == null)
                    {
                        return exes;
                    }

                    return specs == ImGuiSortDirection.Ascending
                        ? exes.OrderBy(c => GetShopItems(c)?.Count ?? 999)
                        : exes.OrderByDescending(c => GetShopItems(c)?.Count ?? 999);
                },
                Filter = (s, npcs) =>
                {
                    return s == null ? npcs : npcs.Where(c =>
                    {
                        var currentValue = GetShopItems(c);
                        if (currentValue == null)
                        {
                            return false;
                        }

                        return currentValue.Any(c => c.Value!.NameString.ToLower().PassesFilter(s));
                    });
                },
                Draw = (ex, contentTypeId) =>
                {

                    var drops = GetShopItems(ex);
                    if (drops != null)
                    {
                        ImGuiService.WrapTableColumnElements("ScrollDrops" + ex.Key, drops,
                            RowSize * ImGui.GetIO().FontGlobalScale - ImGui.GetStyle().FramePadding.X,
                            drop =>
                            {
                                var sourceIcon = ImGuiService.GetIconTexture(drop.Value!.Icon);
                                ImGui.Image(sourceIcon.ImGuiHandle,
                                    new Vector2(RowSize, RowSize) * ImGui.GetIO().FontGlobalScale);
                                if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled &
                                                        ImGuiHoveredFlags.AllowWhenOverlapped &
                                                        ImGuiHoveredFlags.AllowWhenBlockedByPopup &
                                                        ImGuiHoveredFlags.AllowWhenBlockedByActiveItem &
                                                        ImGuiHoveredFlags.AnyWindow) &&
                                    ImGui.IsMouseReleased(ImGuiMouseButton.Right))
                                {
                                    ImGui.OpenPopup("RightClick" + drop.Row);
                                }

                                if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled &
                                                        ImGuiHoveredFlags.AllowWhenOverlapped &
                                                        ImGuiHoveredFlags.AllowWhenBlockedByPopup &
                                                        ImGuiHoveredFlags.AllowWhenBlockedByActiveItem &
                                                        ImGuiHoveredFlags.AnyWindow) &&
                                    ImGui.IsMouseReleased(ImGuiMouseButton.Left))
                                {
                                    MediatorService.Publish(new OpenUintWindowMessage(typeof(ItemWindow), drop.Row));
                                }

                                if (ImGui.BeginPopup("RightClick" + drop.Row))
                                {
                                    if (drop.Value != null)
                                    {
                                        MediatorService.Publish(
                                            ImGuiService.RightClickService.DrawRightClickPopup(drop.Value));
                                    }

                                    ImGui.EndPopup();
                                }


                                ImGuiUtil.HoverTooltip(drop.Value!.NameString);
                                return true;
                            });
                    }
                }
            },
        };
         var eNpcCollection = _excelCache.ENpcCollection;
         _tabs = eNpcCollection == null ? new Dictionary<uint, string>() : eNpcCollection.Where(c => (c.Resident?.FormattedSingular ?? "") != "").SelectMany(c => c.Locations.Select(c => c.PlaceNameEx)).DistinctBy(c => c.Row).ToDictionary(c => c.Row, c => c.Value?.Name.ToDalamudString().ToString() ?? "");
        _items = new Dictionary<uint, List<ENpc>>();
        _filteredItems = new Dictionary<uint, List<ENpc>>();
    }

    private Dictionary<uint, List<LazyRow<ItemEx>>?> _shopItems = new();

    private List<LazyRow<ItemEx>>? GetShopItems(ENpc npc)
    {
        if (_shopItems.TryGetValue(npc.Key, out List<LazyRow<ItemEx>>? value))
        {
            return value;
        }
        if (npc.Shops != null)
        {
            IEnumerable<LazyRow<ItemEx>> items = new List<LazyRow<ItemEx>>();
            foreach (var shop in npc.Shops)
            {
                items = items.Concat(shop.Items);
            }
            var shopItems = items.ToList();
            _shopItems[npc.Key] = shopItems;
            return shopItems;
        }

        _shopItems[npc.Key] = null;
        return null;
    }

    private bool OnLeftClick(ENpc arg)
    {
        MediatorService.Publish(new OpenUintWindowMessage(typeof(ENpcWindow), arg.Key));
        return true;
    }

    public override List<ENpc> GetItems(uint placeNameId)
    {
        if (!_items.ContainsKey(placeNameId))
        {
            if (placeNameId == 0)
            {
                var enpcs = _excelCache.ENpcCollection?.Where(c => (c.Resident?.FormattedSingular ?? "") != "").ToList();
                if (enpcs != null)
                {
                    _items.Add(placeNameId, enpcs);
                }
            }
            else
            {
                var enpcs = _excelCache.ENpcCollection?.Where(c => (c.Resident?.FormattedSingular ?? "") != "" && c.Locations.Any(c => c.PlaceNameEx.Row == placeNameId)).ToList();
                if (enpcs != null)
                {
                    _items.Add(placeNameId, enpcs);
                }
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

    public override bool UseClipper => _useClipper;
    public override string GenericKey => "npcs";
    public override string GenericName => "Npcs";
    public override bool DestroyOnClose => false;
    public override bool SaveState => true;
    public override Vector2? MaxSize { get; } = new(2000, 2000);
    public override Vector2? MinSize { get; } = new(200, 200);
    public override Vector2? DefaultSize { get; } = new(600, 600);
    public override void Draw()
    {
        DrawTabs();
    }

    public override int GetRowId(ENpc item)
    {
        return (int)item.Key;
    }

    public override Dictionary<uint, List<ENpc>> Items => _items;

    public override Dictionary<uint, List<ENpc>> FilteredItems => _filteredItems;

    public override List<TableColumn<ENpc>> Columns => _columns;

    public override ImGuiTableFlags TableFlags => _flags;

    private List<TableColumn<ENpc>> _columns = null!;
    private Dictionary<uint, List<ENpc>> _items = null!;
    private Dictionary<uint, List<ENpc>> _filteredItems = null!;
    private ImGuiTableFlags _flags = ImGuiTableFlags.RowBg | ImGuiTableFlags.BordersV |
                                                   ImGuiTableFlags.BordersOuterV | ImGuiTableFlags.BordersInnerV |
                                                   ImGuiTableFlags.BordersH | ImGuiTableFlags.BordersOuterH |
                                                   ImGuiTableFlags.BordersInnerH |
                                                   ImGuiTableFlags.Resizable | ImGuiTableFlags.Sortable |
                                                   ImGuiTableFlags.Hideable | ImGuiTableFlags.ScrollX |
                                                   ImGuiTableFlags.ScrollY;
    private Dictionary<uint, string> _tabs = null!;
    private string _tableName = "enpcs";
    private bool _useClipper = false;

    public override void Invalidate()
    {
    }

    public override FilterConfiguration? SelectedConfiguration => null;
}