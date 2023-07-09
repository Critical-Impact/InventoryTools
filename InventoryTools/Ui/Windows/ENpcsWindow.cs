using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using CriticalCommonLib;
using CriticalCommonLib.Sheets;
using Dalamud.Utility;
using ImGuiNET;
using InventoryTools.Extensions;
using InventoryTools.Logic;
using OtterGui.Raii;

namespace InventoryTools.Ui;

public class ENpcsWindow : GenericTabbedTable<ENpc>
{
    public ENpcsWindow(string name = "NPCs", ImGuiWindowFlags flags = ImGuiWindowFlags.None, bool forceMainWindow = false) : base(name, flags, forceMainWindow)
    {
        SetupWindow();
    }

    public ENpcsWindow() : base("NPCs")
    {
        SetupWindow();
    }

    public void SetupWindow()
    {
        _columns = new List<TableColumn<ENpc>>()
        {
            new("Icon", 32, ImGuiTableColumnFlags.WidthFixed)
            {
                OnLeftClick = OnLeftClick,
                Draw = (ex, contentTypeId) =>
                {
                    if (ImGui.ImageButton(
                            PluginService.IconStorage[62043].ImGuiHandle,
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
                    UiHelpers.WrapTableColumnElements("Scroll" + ex.Key,positions, RowSize * ImGui.GetIO().FontGlobalScale - ImGui.GetStyle().FramePadding.X, position =>
                    {
                        if (ImGui.ImageButton(PluginService.IconStorage[60561].ImGuiHandle,
                                new Vector2(RowSize * ImGui.GetIO().FontGlobalScale,
                                    RowSize * ImGui.GetIO().FontGlobalScale), new Vector2(0, 0),
                                new Vector2(1, 1), 0))
                        {
                            PluginService.ChatUtilities.PrintFullMapLink(position, ex.Resident?.FormattedSingular ?? "");
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
        };
        _tabs = Service.ExcelCache.ENpcCollection.Where(c => (c.Resident?.FormattedSingular ?? "") != "").SelectMany(c => c.Locations.Select(c => c.PlaceNameEx)).DistinctBy(c => c.Row).ToDictionary(c => c.Row, c => c.Value?.Name.ToDalamudString().ToString() ?? "");
        _items = new Dictionary<uint, List<ENpc>>();
        _filteredItems = new Dictionary<uint, List<ENpc>>();
    }

    private bool OnLeftClick(ENpc arg)
    {
        PluginService.WindowService.OpenENpcWindow(arg.Key);
        return true;
    }

    public override List<ENpc> GetItems(uint placeNameId)
    {
        if (!_items.ContainsKey(placeNameId))
        {
            if (placeNameId == 0)
            {
                var enpcs = Service.ExcelCache.ENpcCollection.Where(c => (c.Resident?.FormattedSingular ?? "") != "").ToList();
                _items.Add(placeNameId, enpcs);
            }
            else
            {
                var enpcs = Service.ExcelCache.ENpcCollection.Where(c => (c.Resident?.FormattedSingular ?? "") != "" && c.Locations.Any(c => c.PlaceNameEx.Row == placeNameId)).ToList();
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

    public static string AsKey
    {
        get { return "enpcs"; }
    }

    public override string Key => AsKey;
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