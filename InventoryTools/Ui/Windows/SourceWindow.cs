using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using AllaganLib.GameSheets.Caches;
using AllaganLib.GameSheets.ItemSources;
using AllaganLib.Shared.Extensions;
using AllaganLib.Shared.Misc;
using DalaMock.Host.Mediator;
using Dalamud.Bindings.ImGui;
using InventoryTools.Logic;
using InventoryTools.Mediator;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Ui;

public enum SourceType
{
    Source,
    Use
}

public class SourceWindow : GenericTabbedTable<(SourceType, ItemInfoType)>
{
    private readonly ItemInfoRenderService _itemInfoRenderService;
    private readonly ItemInfoCache _infoCache;
    private Dictionary<uint, List<(SourceType, ItemInfoType)>> _items;
    private Dictionary<uint, List<(SourceType, ItemInfoType)>> _filteredItems;
    private List<TableColumn<(SourceType, ItemInfoType)>> _columns;
    private readonly List<(SourceType, ItemInfoType)> _sourceTypes;
    private readonly List<(SourceType, ItemInfoType)> _useTypes;
    private Dictionary<uint, string> _tabs;

    public SourceWindow(ILogger<SourceWindow> logger, MediatorService mediator, ImGuiService imGuiService, InventoryToolsConfiguration configuration, ItemInfoRenderService itemInfoRenderService, ItemInfoCache infoCache) : base(logger, mediator, imGuiService, configuration, "Sources Window")
    {
        _itemInfoRenderService = itemInfoRenderService;
        _infoCache = infoCache;
        _sourceTypes = _itemInfoRenderService.SourceRenderers.Select(c => (SourceType.Source, c.Value.Type)).Distinct().ToList();
        _useTypes = _itemInfoRenderService.UseRenderers.Select(c => (SourceType.Use, c.Value.Type)).Distinct().ToList();
    }

    public override void DrawWindow()
    {
        DrawTabs();
    }

    public override void Invalidate()
    {
    }

    public RelationshipType GetRelationType(SourceType sourceType, ItemInfoType infoType)
    {
        return sourceType == SourceType.Source ? (_infoCache.GetItemSourcesByType(infoType).FirstOrDefault()?.RelationshipType ?? RelationshipType.None) : (_infoCache.GetItemUsesByType(infoType).FirstOrDefault()?.RelationshipType ?? RelationshipType.None);
    }

    public ItemSource? GetSampleItem(SourceType sourceType, ItemInfoType infoType)
    {
        return sourceType == SourceType.Source ? (_infoCache.GetItemSourcesByType(infoType).FirstOrDefault()) : (_infoCache.GetItemUsesByType(infoType).FirstOrDefault());
    }

    public override FilterConfiguration? SelectedConfiguration => null;
    public override string GenericKey => "sources";
    public override string GenericName => "Sources";
    public override bool DestroyOnClose => true;
    public override bool SaveState => false;
    public override Vector2? DefaultSize => new Vector2(500, 500);
    public override Vector2? MaxSize => null;
    public override Vector2? MinSize => null;
    public override void Initialize()
    {
        Key = "submarines";
        WindowName = "Submarines";
        _columns = new List<TableColumn<(SourceType, ItemInfoType)>>()
        {
            new("Name", 200, ImGuiTableColumnFlags.WidthFixed)
            {
                Sort = (specs, exes) =>
                {
                    if (specs == null)
                    {
                        return exes;
                    }

                    return specs == ImGuiSortDirection.Ascending ? exes.OrderBy(c => c.Item1 == SourceType.Source ? _itemInfoRenderService.GetSourceTypeName(c.Item2).Singular : _itemInfoRenderService.GetUseTypeName(c.Item2).Singular) : exes.OrderByDescending(c => c.Item1 == SourceType.Source ? _itemInfoRenderService.GetSourceTypeName(c.Item2).Singular : _itemInfoRenderService.GetUseTypeName(c.Item2).Singular);
                },
                Filter = (s, exes) =>
                {
                    if (s == null)
                    {
                        return exes;
                    }
                    return s == "" ? exes : exes.Where(c => (c.Item1 == SourceType.Source ? _itemInfoRenderService.GetSourceTypeName(c.Item2) : _itemInfoRenderService.GetUseTypeName(c.Item2)).Singular.PassesFilter(s.ToLower()));
                },
                Draw = (ex, tabId) =>
                {
                    ImGui.TextUnformatted((ex.Item1 == SourceType.Source ? _itemInfoRenderService.GetSourceTypeName(ex.Item2) : _itemInfoRenderService.GetUseTypeName(ex.Item2)).Singular);
                }
            },
            new("Help Text", 200, ImGuiTableColumnFlags.WidthFixed)
            {
                Sort = (specs, exes) =>
                {
                    if (specs == null)
                    {
                        return exes;
                    }

                    return specs == ImGuiSortDirection.Ascending ? exes.OrderBy(c => c.Item1 == SourceType.Source ? _itemInfoRenderService.GetSourceHelpText(c.Item2) : _itemInfoRenderService.GetUseHelpText(c.Item2)) : exes.OrderByDescending(c => c.Item1 == SourceType.Source ? _itemInfoRenderService.GetSourceHelpText(c.Item2) : _itemInfoRenderService.GetUseHelpText(c.Item2));
                },
                Filter = (s, exes) =>
                {
                    if (s == null)
                    {
                        return exes;
                    }
                    return s == "" ? exes : exes.Where(c => (c.Item1 == SourceType.Source ? _itemInfoRenderService.GetSourceHelpText(c.Item2) : _itemInfoRenderService.GetUseHelpText(c.Item2)).PassesFilter(s.ToLower()));
                },
                Draw = (ex, tabId) =>
                {
                    ImGui.TextUnformatted((ex.Item1 == SourceType.Source ? _itemInfoRenderService.GetSourceHelpText(ex.Item2) : _itemInfoRenderService.GetUseHelpText(ex.Item2)));
                }
            },
            new("Relationship Type", 200, ImGuiTableColumnFlags.WidthFixed)
            {
                Sort = (specs, exes) =>
                {
                    if (specs == null)
                    {
                        return exes;
                    }

                    return specs == ImGuiSortDirection.Ascending ? exes.OrderBy(c => _itemInfoRenderService.GetRelationshipName(GetRelationType(c.Item1, c.Item2))) : exes.OrderByDescending(c => _itemInfoRenderService.GetRelationshipName(GetRelationType(c.Item1, c.Item2)));
                },
                Filter = (s, exes) =>
                {
                    if (s == null)
                    {
                        return exes;
                    }
                    return s == "" ? exes : exes.Where(c => _itemInfoRenderService.GetRelationshipName(GetRelationType(c.Item1, c.Item2)).PassesFilter(s.ToLower()));
                },
                Draw = (ex, tabId) =>
                {
                    ImGui.TextUnformatted(_itemInfoRenderService.GetRelationshipName(GetRelationType(ex.Item1, ex.Item2)));
                }
            },
            new("Sample", 200, ImGuiTableColumnFlags.WidthFixed)
            {
                Sort = (specs, exes) =>
                {
                    return exes;
                },
                Filter = (s, exes) =>
                {
                    return exes;
                },
                Draw = (ex, tabId) =>
                {
                    var sampleItem = GetSampleItem(ex.Item1, ex.Item2);
                    if (sampleItem != null)
                    {
                        if (ex.Item1 == SourceType.Source)
                        {
                            _itemInfoRenderService.DrawItemSourceIcons(
                                "sampleItem_s_" + (uint)ex.Item2,
                                new Vector2(32, 32),
                                [sampleItem]);
                        }
                        else
                        {
                            _itemInfoRenderService.DrawItemUseIcons(
                                "sampleItem_u_" +  + (uint)ex.Item2,
                                new Vector2(32, 32),
                                [sampleItem]);
                        }
                    }
                }
            },
        };
        _tabs = new Dictionary<uint, string>() {{0, "All"}, {1, "Sources"}, {2, "Uses"}};
        _items = new Dictionary<uint, List<(SourceType, ItemInfoType)>>();
        _filteredItems = new Dictionary<uint, List<(SourceType, ItemInfoType)>>();
    }

    public override int GetRowId((SourceType, ItemInfoType) item)
    {
        return item.Item1 == SourceType.Source ? (int)item.Item2 : -(int)item.Item2;
    }

    public override Dictionary<uint, List<(SourceType, ItemInfoType)>> Items => _items;
    public override Dictionary<uint, List<(SourceType, ItemInfoType)>> FilteredItems => _filteredItems;
    public override List<TableColumn<(SourceType, ItemInfoType)>> Columns => _columns;

    public override ImGuiTableFlags TableFlags => _flags;

    private ImGuiTableFlags _flags = ImGuiTableFlags.RowBg | ImGuiTableFlags.BordersV |
                                     ImGuiTableFlags.BordersOuterV | ImGuiTableFlags.BordersInnerV |
                                     ImGuiTableFlags.BordersH | ImGuiTableFlags.BordersOuterH |
                                     ImGuiTableFlags.BordersInnerH |
                                     ImGuiTableFlags.Resizable | ImGuiTableFlags.Sortable |
                                     ImGuiTableFlags.Hideable | ImGuiTableFlags.ScrollX |
                                     ImGuiTableFlags.ScrollY;
    public override List<(SourceType, ItemInfoType)> GetItems(uint tabId)
    {
        if (!_items.ContainsKey(tabId))
        {
            if (tabId == 0)
            {
                var types = new List<(SourceType, ItemInfoType)>();
                types.AddRange(_sourceTypes);
                types.AddRange(_useTypes);
                _items.Add(tabId, types);
            }
            else
            {
                if (tabId == 1)
                {
                    _items.Add(tabId, _sourceTypes);
                }
                if (tabId == 2)
                {
                    _items.Add(tabId, _useTypes);
                }
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
    public override string TableName => "sources";
    public override bool UseClipper => false;
}