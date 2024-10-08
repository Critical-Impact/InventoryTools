using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using CriticalCommonLib.Extensions;
using CriticalCommonLib.Services;
using CriticalCommonLib.Services.Mediator;
using CriticalCommonLib.Sheets;
using ImGuiNET;
using InventoryTools.Extensions;
using InventoryTools.Logic;
using InventoryTools.Mediator;
using InventoryTools.Services;
using InventoryTools.Services.Interfaces;
using Microsoft.Extensions.Logging;
using ImGuiUtil = OtterGui.ImGuiUtil;

namespace InventoryTools.Ui;

public class RetainerTasksWindow : GenericTabbedTable<RetainerTaskEx>, IMenuWindow
{
    private readonly ExcelCache _excelCache;

    public RetainerTasksWindow(ILogger<RetainerTasksWindow> logger, MediatorService mediator, ImGuiService imGuiService, InventoryToolsConfiguration configuration, ExcelCache excelCache, string name = "Retainer Ventures") : base(logger, mediator, imGuiService, configuration, name)
    {
        _excelCache = excelCache;
    }
    public override void Initialize()
    {
        WindowName = "Retainer Ventures";
        Key = "retainerTasks";
        _columns = new List<TableColumn<RetainerTaskEx>>()
        {
            new("Icon", 32, ImGuiTableColumnFlags.WidthFixed)
            {
                OnLeftClick = OnLeftClick,
                Draw = (ex, contentTypeId) =>
                {
                    if (ImGui.ImageButton(ImGuiService.GetIconTexture(65049).ImGuiHandle,
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

                    return specs == ImGuiSortDirection.Ascending ? exes.OrderBy(c => c.NameString) : exes.OrderByDescending(c => c.NameString);
                },
                Filter = (s, exes) =>
                {
                    if (s == null)
                    {
                        return exes;
                    }
                    return s == "" ? exes : exes.Where(c => c.NameString.ToLower().PassesFilter(s.ToLower()));
                },
                Draw = (ex, tabId) =>
                {
                    ImGui.TextUnformatted(ex.NameString.ToString());
                }
            },
            new("Task Type", 200, ImGuiTableColumnFlags.WidthFixed)
            {
                Sort = (specs, exes) =>
                {
                    if (specs == null)
                    {
                        return exes;
                    }

                    return specs == ImGuiSortDirection.Ascending ? exes.OrderBy(c => c.RetainerTaskType.FormattedName()) : exes.OrderByDescending(c => c.RetainerTaskType.FormattedName());
                },
                Filter = (s, exes) =>
                {
                    if (s == null)
                    {
                        return exes;
                    }
                    return s == "" ? exes : exes.Where(c => c.RetainerTaskType.FormattedName().ToLower().PassesFilter(s.ToLower()));
                },
                Draw = (ex, tabId) =>
                {
                    ImGui.TextUnformatted(ex.RetainerTaskType.FormattedName());
                },
                AllTabOnly = true
            },
            new("Level", 200, ImGuiTableColumnFlags.WidthFixed)
            {
                Sort = (specs, exes) =>
                {
                    if (specs == null)
                    {
                        return exes;
                    }

                    return specs == ImGuiSortDirection.Ascending ? exes.OrderBy(c => c.RetainerLevel) : exes.OrderByDescending(c => c.RetainerLevel);
                },
                Filter = (s, exes) =>
                {
                    if (s == null)
                    {
                        return exes;
                    }
                    return s == "" ? exes : exes.Where(c => c.RetainerLevel.ToString().PassesFilter(s.ToLower()));
                },
                Draw = (ex, tabId) =>
                {
                    ImGui.TextUnformatted(ex.RetainerLevel.ToString());
                }
            },
            new("Duration", 200, ImGuiTableColumnFlags.WidthFixed)
            {
                Sort = (specs, exes) =>
                {
                    if (specs == null)
                    {
                        return exes;
                    }

                    return specs == ImGuiSortDirection.Ascending ? exes.OrderBy(c => c.DurationString) : exes.OrderByDescending(c => c.DurationString);
                },
                Filter = (s, exes) =>
                {
                    if (s == null)
                    {
                        return exes;
                    }
                    return s == "" ? exes : exes.Where(c => c.DurationString.PassesFilter(s.ToLower()));
                },
                Draw = (ex, tabId) =>
                {
                    ImGui.TextUnformatted(ex.DurationString);
                }
            },
            new("Experience", 200, ImGuiTableColumnFlags.WidthFixed)
            {
                Sort = (specs, exes) =>
                {
                    if (specs == null)
                    {
                        return exes;
                    }

                    return specs == ImGuiSortDirection.Ascending ? exes.OrderBy(c => c.Experience) : exes.OrderByDescending(c => c.Experience);
                },
                Filter = (s, exes) =>
                {
                    if (s == null)
                    {
                        return exes;
                    }
                    return s == "" ? exes : exes.Where(c => c.ExperienceString.PassesFilter(s.ToLower()));
                },
                Draw = (ex, tabId) =>
                {
                    ImGui.TextUnformatted(ex.ExperienceString.ToString());
                }
            },
            new("Venture Cost", 200, ImGuiTableColumnFlags.WidthFixed)
            {
                Sort = (specs, exes) =>
                {
                    if (specs == null)
                    {
                        return exes;
                    }

                    return specs == ImGuiSortDirection.Ascending ? exes.OrderBy(c => c.VentureCost) : exes.OrderByDescending(c => c.VentureCost);
                },
                Filter = (s, exes) =>
                {
                    if (s == null)
                    {
                        return exes;
                    }
                    return s == "" ? exes : exes.Where(c => c.VentureCost.PassesFilter(s.ToLower()));
                },
                Draw = (ex, tabId) =>
                {
                    ImGui.TextUnformatted(ex.VentureCost.ToString());
                }
            },
            new("Average iLvl", 200, ImGuiTableColumnFlags.WidthFixed)
            {
                Sort = (specs, exes) =>
                {
                    if (specs == null)
                    {
                        return exes;
                    }

                    return specs == ImGuiSortDirection.Ascending ? exes.OrderBy(c => c.RequiredItemLevel) : exes.OrderByDescending(c => c.RequiredItemLevel);
                },
                Filter = (s, exes) =>
                {
                    if (s == null)
                    {
                        return exes;
                    }
                    return s == "" ? exes : exes.Where(c => c.RequiredItemLevel.PassesFilter(s.ToLower()));
                },
                Draw = (ex, tabId) =>
                {
                    ImGui.TextUnformatted(ex.RequiredItemLevel.ToString());
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
                        var currentValue = c.Drops;

                        return currentValue.Any(c => c.NameString.ToLower().PassesFilter(s));
                    });
                },
                Draw = (ex, contentTypeId) =>
                {
                    var drops = ex.Drops;
                    ImGuiService.WrapTableColumnElements("Drops" + ex.RowId, drops,
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
        _tabs = Enum.GetValues<RetainerTaskType>().Where(c => c != RetainerTaskType.Unknown).ToDictionary(c => (uint)c, c =>c.FormattedName());
        _items = new Dictionary<uint, List<RetainerTaskEx>>();
        _filteredItems = new Dictionary<uint, List<RetainerTaskEx>>();
    }

    private bool OnLeftClick(RetainerTaskEx arg)
    {
        MediatorService.Publish(new OpenUintWindowMessage(typeof(RetainerTasksWindow), arg.RowId));
        return true;
    }

    public override string GenericKey { get; } = "retainerTasks";
    public override string GenericName { get; } = "Retainer Tasks";
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

    public override int GetRowId(RetainerTaskEx item)
    {
        return (int)item.RowId;
    }

    public override Dictionary<uint, List<RetainerTaskEx>> Items => _items;

    public override Dictionary<uint, List<RetainerTaskEx>> FilteredItems => _filteredItems;

    public override List<TableColumn<RetainerTaskEx>> Columns => _columns;

    private List<TableColumn<RetainerTaskEx>> _columns = null!;
    private Dictionary<uint, List<RetainerTaskEx>> _items= null!;
    private Dictionary<uint, List<RetainerTaskEx>> _filteredItems= null!;
    private Dictionary<uint, string> _tabs= null!;

    public override ImGuiTableFlags TableFlags => _flags;

    private ImGuiTableFlags _flags = ImGuiTableFlags.RowBg | ImGuiTableFlags.BordersV |
                                     ImGuiTableFlags.BordersOuterV | ImGuiTableFlags.BordersInnerV |
                                     ImGuiTableFlags.BordersH | ImGuiTableFlags.BordersOuterH |
                                     ImGuiTableFlags.BordersInnerH |
                                     ImGuiTableFlags.Resizable | ImGuiTableFlags.Sortable |
                                     ImGuiTableFlags.Hideable | ImGuiTableFlags.ScrollX |
                                     ImGuiTableFlags.ScrollY;
    public override List<RetainerTaskEx> GetItems(uint tabId)
    {
        if (!_items.ContainsKey(tabId))
        {
            if (tabId == 0)
            {
                var duties = _excelCache.GetRetainerTaskExSheet().Where(c => c.Task != 0).ToList();
                _items.Add(tabId, duties);
            }
            else
            {
                var duties = _excelCache.GetRetainerTaskExSheet().Where(c => c.FormattedName.ToString() != "" && (uint)c.RetainerTaskType == tabId && c.Task != 0).ToList();
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

    private string _tableName = "retainerTasks";
    private bool _useClipper = false;
}