using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using AllaganLib.GameSheets.Model;
using AllaganLib.GameSheets.Sheets;
using AllaganLib.GameSheets.Sheets.Rows;
using AllaganLib.Shared.Extensions;
using CriticalCommonLib.Extensions;
using CriticalCommonLib.Services.Mediator;

using ImGuiNET;
using InventoryTools.Logic;
using InventoryTools.Mediator;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;
using ImGuiUtil = OtterGui.ImGuiUtil;

namespace InventoryTools.Ui;

public class RetainerTasksWindow : GenericTabbedTable<RetainerTaskRow>, IMenuWindow
{
    private readonly RetainerTaskSheet _retainerTaskSheet;

    public RetainerTasksWindow(ILogger<RetainerTasksWindow> logger, MediatorService mediator, ImGuiService imGuiService, InventoryToolsConfiguration configuration, RetainerTaskSheet retainerTaskSheet, string name = "Retainer Ventures") : base(logger, mediator, imGuiService, configuration, name)
    {
        _retainerTaskSheet = retainerTaskSheet;
    }
    public override void Initialize()
    {
        WindowName = "Retainer Ventures";
        Key = "retainerTasks";
        _columns = new List<TableColumn<RetainerTaskRow>>()
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
                Draw = (ex, tabId) =>
                {
                    ImGui.TextUnformatted(ex.FormattedName.ToString());
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

                    return specs == ImGuiSortDirection.Ascending ? exes.OrderBy(c => c.Base.RetainerLevel) : exes.OrderByDescending(c => c.Base.RetainerLevel);
                },
                Filter = (s, exes) =>
                {
                    if (s == null)
                    {
                        return exes;
                    }
                    return s == "" ? exes : exes.Where(c => c.Base.RetainerLevel.ToString().PassesFilter(s.ToLower()));
                },
                Draw = (ex, tabId) =>
                {
                    ImGui.TextUnformatted(ex.Base.RetainerLevel.ToString());
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

                    return specs == ImGuiSortDirection.Ascending ? exes.OrderBy(c => c.Base.Experience) : exes.OrderByDescending(c => c.Base.Experience);
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

                    return specs == ImGuiSortDirection.Ascending ? exes.OrderBy(c => c.Base.VentureCost) : exes.OrderByDescending(c => c.Base.VentureCost);
                },
                Filter = (s, exes) =>
                {
                    if (s == null)
                    {
                        return exes;
                    }
                    return s == "" ? exes : exes.Where(c => c.Base.VentureCost.PassesFilter(s.ToLower()));
                },
                Draw = (ex, tabId) =>
                {
                    ImGui.TextUnformatted(ex.Base.VentureCost.ToString());
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

                    return specs == ImGuiSortDirection.Ascending ? exes.OrderBy(c => c.Base.RequiredItemLevel) : exes.OrderByDescending(c => c.Base.RequiredItemLevel);
                },
                Filter = (s, exes) =>
                {
                    if (s == null)
                    {
                        return exes;
                    }
                    return s == "" ? exes : exes.Where(c => c.Base.RequiredItemLevel.PassesFilter(s.ToLower()));
                },
                Draw = (ex, tabId) =>
                {
                    ImGui.TextUnformatted(ex.Base.RequiredItemLevel.ToString());
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
                            MediatorService.Publish(ImGuiService.ImGuiMenuService.DrawRightClickPopup(drop));
                            ImGui.EndPopup();
                        }
                        ImGuiUtil.HoverTooltip(drop.NameString);

                        return true;
                    });

                }
            },
        };
        _tabs = Enum.GetValues<RetainerTaskType>().Where(c => c != RetainerTaskType.Unknown).ToDictionary(c => (uint)c, c =>c.FormattedName());
        _items = new Dictionary<uint, List<RetainerTaskRow>>();
        _filteredItems = new Dictionary<uint, List<RetainerTaskRow>>();
    }

    private bool OnLeftClick(RetainerTaskRow arg)
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

    public override int GetRowId(RetainerTaskRow item)
    {
        return (int)item.RowId;
    }

    public override Dictionary<uint, List<RetainerTaskRow>> Items => _items;

    public override Dictionary<uint, List<RetainerTaskRow>> FilteredItems => _filteredItems;

    public override List<TableColumn<RetainerTaskRow>> Columns => _columns;

    private List<TableColumn<RetainerTaskRow>> _columns = null!;
    private Dictionary<uint, List<RetainerTaskRow>> _items= null!;
    private Dictionary<uint, List<RetainerTaskRow>> _filteredItems= null!;
    private Dictionary<uint, string> _tabs= null!;

    public override ImGuiTableFlags TableFlags => _flags;

    private ImGuiTableFlags _flags = ImGuiTableFlags.RowBg | ImGuiTableFlags.BordersV |
                                     ImGuiTableFlags.BordersOuterV | ImGuiTableFlags.BordersInnerV |
                                     ImGuiTableFlags.BordersH | ImGuiTableFlags.BordersOuterH |
                                     ImGuiTableFlags.BordersInnerH |
                                     ImGuiTableFlags.Resizable | ImGuiTableFlags.Sortable |
                                     ImGuiTableFlags.Hideable | ImGuiTableFlags.ScrollX |
                                     ImGuiTableFlags.ScrollY;
    public override List<RetainerTaskRow> GetItems(uint tabId)
    {
        if (!_items.ContainsKey(tabId))
        {
            if (tabId == 0)
            {
                var duties = _retainerTaskSheet.Where(c => c.Base.Task.RowId != 0).ToList();
                _items.Add(tabId, duties);
            }
            else
            {
                var duties = _retainerTaskSheet.Where(c => c.FormattedName.ToString() != "" && (uint)c.RetainerTaskType == tabId && c.Base.Task.RowId != 0).ToList();
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