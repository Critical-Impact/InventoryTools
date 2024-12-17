using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using AllaganLib.GameSheets.Caches;
using AllaganLib.GameSheets.Sheets;
using AllaganLib.GameSheets.Sheets.Rows;
using AllaganLib.Shared.Extensions;
using CriticalCommonLib.Services;
using CriticalCommonLib.Services.Mediator;

using ImGuiNET;
using InventoryTools.Extensions;
using InventoryTools.Logic;
using OtterGui;
using Dalamud.Interface.Utility.Raii;
using InventoryTools.Mediator;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Ui;

public class ENpcsWindow : GenericTabbedTable<ENpcResidentRow>, IMenuWindow
{
    private readonly IChatUtilities _chatUtilities;
    private readonly ItemInfoCache _itemInfoCache;
    private readonly ENpcResidentSheet _eNpcResidentSheet;

    public ENpcsWindow(ILogger<ENpcsWindow> logger,
        MediatorService mediator,
        ImGuiService imGuiService,
        InventoryToolsConfiguration configuration,
        IChatUtilities chatUtilities,
        ItemInfoCache itemInfoCache,
        ENpcResidentSheet eNpcResidentSheet,
        string name = "NPCs Window") : base(logger,
        mediator,
        imGuiService,
        configuration,
        name)
    {
        _chatUtilities = chatUtilities;
        _itemInfoCache = itemInfoCache;
        _eNpcResidentSheet = eNpcResidentSheet;
    }

    public override void Initialize()
    {
        WindowName = "NPCs";
        Key = "enpcs";
         _columns = new List<TableColumn<ENpcResidentRow>>()
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
                        ? exes.OrderBy(c => c.Base.Singular.ExtractText() ?? "")
                        : exes.OrderByDescending(c => c.Base.Singular.ExtractText() ?? "");
                },
                Filter = (s, exes) =>
                {
                    if (s == null)
                    {
                        return exes;
                    }

                    return s == "" ? exes : exes.Where(c => (c.Base.Singular.ExtractText() ?? "").ToLower().PassesFilter(s.ToLower()));
                },
                Draw = (ex, contentTypeId) =>
                {
                    ImGui.TextUnformatted(ex.Base.Singular.ExtractText() ?? "");
                }
            },
            new("Locations", 200, ImGuiTableColumnFlags.WidthFixed | ImGuiTableColumnFlags.NoSort)
            {
                Draw = (ex, contentTypeId) =>
                {
                    var positions = ex.ENpcBase.Locations;
                    ImGuiService.WrapTableColumnElements("Scroll" + ex.RowId,positions, RowSize * ImGui.GetIO().FontGlobalScale - ImGui.GetStyle().FramePadding.X, position =>
                    {
                        if (ImGui.ImageButton(ImGuiService.GetIconTexture(60561).ImGuiHandle,
                                new Vector2(RowSize * ImGui.GetIO().FontGlobalScale,
                                    RowSize * ImGui.GetIO().FontGlobalScale), new Vector2(0, 0),
                                new Vector2(1, 1), 0))
                        {
                            _chatUtilities.PrintFullMapLink(position, ex.Base.Singular.ExtractText() ?? "");
                        }

                        if (ImGui.IsItemHovered())
                        {
                            using var tt = ImRaii.Tooltip();
                            ImGui.TextUnformatted((position.PlaceName.ValueNullable?.Name.ExtractText() ?? "Unknown") + " - " +
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

                    return exes.Where(c => c.ENpcBase.IsVendor && s == true || !c.ENpcBase.IsVendor && s == false);
                },
                FilterBoolean = null,
                Draw = (ex, contentTypeId) =>
                {
                    if (ex.ENpcBase.IsVendor)
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

                        return currentValue.Any(c => c.NameString.ToLower().PassesFilter(s));
                    });
                },
                Draw = (ex, contentTypeId) =>
                {

                    var drops = GetShopItems(ex);
                    if (drops != null)
                    {
                        ImGuiService.WrapTableColumnElements("ScrollDrops" + ex.RowId, drops,
                            RowSize * ImGui.GetIO().FontGlobalScale - ImGui.GetStyle().FramePadding.X,
                            drop =>
                            {
                                var sourceIcon = ImGuiService.GetIconTexture(drop.Icon);
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
                                    MediatorService.Publish(new OpenUintWindowMessage(typeof(ItemWindow), drop.RowId));
                                }

                                if (ImGui.BeginPopup("RightClick" + drop.RowId))
                                {
                                    MediatorService.Publish(
                                        ImGuiService.ImGuiMenuService.DrawRightClickPopup(drop));

                                    ImGui.EndPopup();
                                }


                                ImGuiUtil.HoverTooltip(drop.NameString);
                                return true;
                            });
                    }
                }
            },
        };
         _tabs = _eNpcResidentSheet.Where(c => (c.Base.Singular.ExtractText() ?? "") != "").SelectMany(c => c.ENpcBase.Locations.Select(c => c.PlaceName)).DistinctBy(c => c.RowId).ToDictionary(c => c.RowId, c => c.ValueNullable?.Name.ExtractText().ToString() ?? "");
        _items = new Dictionary<uint, List<ENpcResidentRow>>();
        _filteredItems = new Dictionary<uint, List<ENpcResidentRow>>();
    }

    private Dictionary<uint, List<ItemRow>?> _shopItems = new();

    private List<ItemRow>? GetShopItems(ENpcResidentRow npc)
    {
        if (_shopItems.TryGetValue(npc.RowId, out List<ItemRow>? value))
        {
            return value;
        }

        var npcShops = _itemInfoCache.GetNpcShops(npc.RowId);
        if (npcShops != null)
        {
            IEnumerable<ItemRow> items = new List<ItemRow>();
            foreach (var shop in npcShops)
            {
                items = items.Concat(shop.Items);
            }
            var shopItems = items.ToList();
            _shopItems[npc.RowId] = shopItems;
            return shopItems;
        }

        _shopItems[npc.RowId] = null;
        return null;
    }

    private bool OnLeftClick(ENpcResidentRow arg)
    {
        MediatorService.Publish(new OpenUintWindowMessage(typeof(ENpcWindow), arg.RowId));
        return true;
    }

    public override List<ENpcResidentRow> GetItems(uint placeNameId)
    {
        if (!_items.ContainsKey(placeNameId))
        {
            if (placeNameId == 0)
            {
                var enpcs = _eNpcResidentSheet.Where(c => (c.Base.Singular.ExtractText() ?? "") != "").ToList();
                _items.Add(placeNameId, enpcs);
            }
            else
            {
                var enpcs = _eNpcResidentSheet.Where(c => (c.Base.Singular.ExtractText() ?? "") != "" && c.ENpcBase.Locations.Any(c => c.PlaceName.RowId == placeNameId)).ToList();
                _items.Add(placeNameId, enpcs);
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

    public override int GetRowId(ENpcResidentRow item)
    {
        return (int)item.RowId;
    }

    public override Dictionary<uint, List<ENpcResidentRow>> Items => _items;

    public override Dictionary<uint, List<ENpcResidentRow>> FilteredItems => _filteredItems;

    public override List<TableColumn<ENpcResidentRow>> Columns => _columns;

    public override ImGuiTableFlags TableFlags => _flags;

    private List<TableColumn<ENpcResidentRow>> _columns = null!;
    private Dictionary<uint, List<ENpcResidentRow>> _items = null!;
    private Dictionary<uint, List<ENpcResidentRow>> _filteredItems = null!;
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