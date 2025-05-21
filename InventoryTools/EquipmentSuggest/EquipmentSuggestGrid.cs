using System.Collections.Generic;
using System.Linq;
using AllaganLib.Data.Service;
using AllaganLib.GameSheets.Model;
using AllaganLib.GameSheets.Sheets;
using AllaganLib.GameSheets.Sheets.Rows;
using AllaganLib.Interface.Grid;
using AllaganLib.Shared.Extensions;
using CriticalCommonLib.Services.Mediator;
using ImGuiNET;
using InventoryTools.Logic;

namespace InventoryTools.EquipmentSuggest;

public sealed class EquipmentSuggestGrid : RenderTable<EquipmentSuggestConfig, EquipmentSuggestItem, MessageBase>
{
    private readonly EquipmentSuggestSuggestionColumn.Factory _columnFactory;
    private readonly ItemSheet _itemSheet;
    private readonly EquipmentSuggestClassJobFormField _classJobFormField;
    private readonly EquipmentSuggestSourceTypeField _sourceTypesField;
    private readonly EquipmentSuggestExcludeSourceTypeField _excludeSourcesField;
    private readonly EquipmentSuggestLevelFormField _levelField;
    private readonly ClassJobSheet _classJobSheet;
    private readonly ClassJobCategorySheet _classJobCategorySheet;
    private readonly EquipmentSuggestFilterStatsField _filterStatsField;
    private readonly EquipmentSuggestModeSetting _modeSetting;
    private readonly EquipmentSuggestToolModeCategorySetting _toolModeSetting;
    private readonly InventoryToolsConfiguration _configuration;
    private readonly EquipmentSuggestConfig _config;
    private EquipmentSuggestMode? _mode;
    private EquipmentSuggestToolModeCategory? _toolMode;
    private List<ItemRow>? applicableItems;
    private List<EquipmentSuggestItem> items = [];
    private List<EquipmentSuggestSuggestionColumn> suggestionColumns = [];
    public
        EquipmentSuggestGrid(CsvLoaderService csvLoaderService, EquipmentSuggestConfig searchFilter,
            EquipmentSuggestSlotColumn slotColumn, EquipmentSuggestSelectedItemColumn selectedItemColumn,
            EquipmentSuggestSuggestionColumn.Factory columnFactory, ItemSheet itemSheet,
            EquipmentSuggestClassJobFormField classJobFormField, EquipmentSuggestSourceTypeField sourceTypesField,
            EquipmentSuggestExcludeSourceTypeField excludeSourcesField, EquipmentSuggestLevelFormField levelField,
            ClassJobSheet classJobSheet, ClassJobCategorySheet classJobCategorySheet,
            EquipmentSuggestFilterStatsField filterStatsField, EquipmentSuggestModeSetting modeSetting,
            EquipmentSuggestToolModeCategorySetting toolModeSetting, EquipmentSuggestSelectedSecondaryItemColumn secondarySlotColumn,
            InventoryToolsConfiguration configuration,
            EquipmentSuggestConfig config) : base(
        csvLoaderService, searchFilter, [slotColumn, selectedItemColumn, secondarySlotColumn],
        ImGuiTableFlags.SizingStretchProp | ImGuiTableFlags.Hideable | ImGuiTableFlags.Resizable |
        ImGuiTableFlags.RowBg | ImGuiTableFlags.BordersInnerH | ImGuiTableFlags.BordersOuterH |
        ImGuiTableFlags.BordersInnerV | ImGuiTableFlags.BordersOuterV | ImGuiTableFlags.BordersH |
        ImGuiTableFlags.BordersV | ImGuiTableFlags.BordersInner | ImGuiTableFlags.BordersOuter |
        ImGuiTableFlags.Borders | ImGuiTableFlags.ScrollX | ImGuiTableFlags.ScrollY, "Equipment Recommendations",
        "equipmentSuggest")
    {
        _columnFactory = columnFactory;
        _itemSheet = itemSheet;
        _classJobFormField = classJobFormField;
        _sourceTypesField = sourceTypesField;
        _excludeSourcesField = excludeSourcesField;
        _levelField = levelField;
        _classJobSheet = classJobSheet;
        _classJobCategorySheet = classJobCategorySheet;
        _filterStatsField = filterStatsField;
        _modeSetting = modeSetting;
        _toolModeSetting = toolModeSetting;
        _configuration = configuration;
        _config = config;
        this.GenerateItems();
        for (int i = 0; i < 5; i++)
        {
            var column = _columnFactory.Invoke(i);
            this.Columns.Add(column);
            this.suggestionColumns.Add(column);
        }

        this.FreezeRows = 1;
        this.HasFooter = true;
        this.UseClipper = false;
    }

    public void GenerateItems()
    {
        var currentMode = _modeSetting.CurrentValue(_configuration);
        var toolMode = _toolModeSetting.CurrentValue(_config);
        if (_mode == null || currentMode != _mode || _toolMode == null || toolMode != _toolMode)
        {
            items = new();
            _mode = currentMode;
            _toolMode = toolMode;
            if (currentMode == EquipmentSuggestMode.Tool)
            {
                if (_toolMode == EquipmentSuggestToolModeCategory.Crafting)
                {
                    var classes = _classJobSheet.Where(c => _classJobCategorySheet.GetRow(c.Base.ClassJobCategory.RowId).IsCrafting).OrderBy(c => c.Base.Name.ToImGuiString());
                    foreach (var classRow in classes)
                    {
                        items.Add(new EquipmentSuggestItem(classRow));
                    }
                }
                else if (_toolMode == EquipmentSuggestToolModeCategory.Combat)
                {
                    var classes = _classJobSheet.Where(c => _classJobCategorySheet.GetRow(c.Base.ClassJobCategory.RowId).IsCombat && c.RowId != 0).OrderBy(c => c.Base.ClassJobCategory.RowId).ThenBy(c => c.Base.Name.ToImGuiString());
                    foreach (var classRow in classes)
                    {
                        items.Add(new EquipmentSuggestItem(classRow));
                    }
                }
                else if (_toolMode == EquipmentSuggestToolModeCategory.CombatTank)
                {
                    var classes = _classJobSheet.Where(c => _classJobCategorySheet.GetRow(c.Base.ClassJobCategory.RowId).IsCombat && c.Base.Role == 1 && c.RowId != 0).OrderBy(c => c.Base.ClassJobCategory.RowId).ThenBy(c => c.Base.Name.ToImGuiString());
                    foreach (var classRow in classes)
                    {
                        items.Add(new EquipmentSuggestItem(classRow));
                    }
                }
                else if (_toolMode == EquipmentSuggestToolModeCategory.CombatRanged)
                {
                    var classes = _classJobSheet.Where(c => _classJobCategorySheet.GetRow(c.Base.ClassJobCategory.RowId).IsCombat && c.Base.Role == 3 && c.RowId != 0).OrderBy(c => c.Base.ClassJobCategory.RowId).ThenBy(c => c.Base.Name.ToImGuiString());
                    foreach (var classRow in classes)
                    {
                        items.Add(new EquipmentSuggestItem(classRow));
                    }
                }
                else if (_toolMode == EquipmentSuggestToolModeCategory.CombatHealer)
                {
                    var classes = _classJobSheet.Where(c => _classJobCategorySheet.GetRow(c.Base.ClassJobCategory.RowId).IsCombat && c.Base.Role == 4 && c.RowId != 0).OrderBy(c => c.Base.ClassJobCategory.RowId).ThenBy(c => c.Base.Name.ToImGuiString());
                    foreach (var classRow in classes)
                    {
                        items.Add(new EquipmentSuggestItem(classRow));
                    }
                }
                else if (_toolMode == EquipmentSuggestToolModeCategory.CombatMelee)
                {
                    var classes = _classJobSheet.Where(c => _classJobCategorySheet.GetRow(c.Base.ClassJobCategory.RowId).IsCombat && c.Base.Role == 2 && c.RowId != 0).OrderBy(c => c.Base.ClassJobCategory.RowId).ThenBy(c => c.Base.Name.ToImGuiString());
                    foreach (var classRow in classes)
                    {
                        items.Add(new EquipmentSuggestItem(classRow));
                    }
                }
                else if (_toolMode == EquipmentSuggestToolModeCategory.Gathering)
                {
                    var classes = _classJobSheet.Where(c => _classJobCategorySheet.GetRow(c.Base.ClassJobCategory.RowId).IsGathering).OrderBy(c => c.Base.Name.ToImGuiString());
                    foreach (var classRow in classes)
                    {
                        items.Add(new EquipmentSuggestItem(classRow));
                    }
                }
            }
            else if (currentMode == EquipmentSuggestMode.Class)
            {
                items.Add(new EquipmentSuggestItem(EquipSlot.MainHand));
                items.Add(new EquipmentSuggestItem(EquipSlot.OffHand));
                items.Add(new EquipmentSuggestItem(EquipSlot.Head));
                items.Add(new EquipmentSuggestItem(EquipSlot.Body));
                items.Add(new EquipmentSuggestItem(EquipSlot.Gloves));
                items.Add(new EquipmentSuggestItem(EquipSlot.Legs));
                items.Add(new EquipmentSuggestItem(EquipSlot.Feet));
                items.Add(new EquipmentSuggestItem(EquipSlot.Ears));
                items.Add(new EquipmentSuggestItem(EquipSlot.Neck));
                items.Add(new EquipmentSuggestItem(EquipSlot.Wrists));
                items.Add(new EquipmentSuggestItem(EquipSlot.FingerL));
                items.Add(new EquipmentSuggestItem(EquipSlot.FingerR));
            }
        }

    }

    public override List<EquipmentSuggestItem> GetItems()
    {
        GenerateItems();
        var count = 0;
        Dictionary<int, Dictionary<int, List<SearchResult>>> resultCache = new Dictionary<int, Dictionary<int, List<SearchResult>>>();

        foreach(var currentLevel in _levelField.GetCenteredRange(_config))
        {
            for (var index = 0; index < items.Count; index++)
            {
                resultCache.TryAdd(index, new Dictionary<int, List<SearchResult>>());
                var item = items[index];
                resultCache[index].TryAdd(count, new List<SearchResult>());
                resultCache[index][count] = GetSuggestedItems(item, currentLevel);
            }

            count++;
        }

        if (_modeSetting.CurrentValue(_configuration) == EquipmentSuggestMode.Class)
        {
            //Check each item's cache to see if we have any items available for the level selected, if not, we'll need to iterate back until we find some items
            for (var index = 0; index < items.Count; index++)
            {
                if (resultCache.TryGetValue(index, out var value))
                {
                    var maxLevel = _levelField.GetCenteredValue(_config, 2);
                    var hasItem = false;
                    var lowestLevel = maxLevel;
                    count = 0;
                    foreach (var currentLevel in _levelField.GetCenteredRange(_config))
                    {
                        if (currentLevel < lowestLevel)
                        {
                            lowestLevel = currentLevel;
                        }

                        if (currentLevel > maxLevel)
                        {
                            break;
                        }

                        if (value.TryGetValue(count, out var result))
                        {
                            if (result.Count > 0)
                            {
                                hasItem = true;
                            }
                        }

                        count++;
                    }

                    if (!hasItem)
                    {
                        while (lowestLevel != 1)
                        {
                            lowestLevel--;
                            var item = items[index];
                            var suggestedItems = GetSuggestedItems(item, lowestLevel);
                            if (suggestedItems.Count > 0)
                            {
                                resultCache[index][0] = suggestedItems;
                                break;
                            }
                        }
                    }


                }
            }
        }
        else
        {
            //Check each item's cache to see if we have any items available for the level selected, if not, we'll need to iterate back until we find some items
            for (var index = 0; index < items.Count; index++)
            {
                if (resultCache.TryGetValue(index, out var value))
                {
                    var maxLevel = _levelField.GetCenteredValue(_config, 2);
                    var hasMainHand = false;
                    var hasOffHand = false;
                    var lowestLevel = maxLevel;
                    count = 0;
                    foreach (var currentLevel in _levelField.GetCenteredRange(_config))
                    {
                        if (currentLevel < lowestLevel)
                        {
                            lowestLevel = currentLevel;
                        }

                        if (currentLevel > maxLevel)
                        {
                            break;
                        }

                        if (value.TryGetValue(count, out var result))
                        {
                            if (result.Any(c => c.Item.Base.EquipSlotCategory.Value.MainHand == 1))
                            {
                                hasMainHand = true;
                            }
                            if (result.Any(c => c.Item.Base.EquipSlotCategory.Value.OffHand == 1))
                            {
                                hasOffHand = true;
                            }
                        }

                        count++;
                    }

                    if (!hasOffHand || !hasMainHand)
                    {
                        while (lowestLevel != 1)
                        {
                            lowestLevel--;
                            var item = items[index];
                            var suggestedItems = GetSuggestedItems(item, lowestLevel);
                            var includeResults = false;
                            if (!hasOffHand && suggestedItems.Any(c => c.Item.Base.EquipSlotCategory.Value.OffHand == 1))
                            {
                                hasOffHand = true;
                                includeResults = true;
                            }
                            if (!hasMainHand && suggestedItems.Any(c => c.Item.Base.EquipSlotCategory.Value.MainHand == 1))
                            {
                                hasMainHand = true;
                                includeResults = true;
                            }

                            if (includeResults)
                            {
                                resultCache[index].TryAdd(0, []);
                                resultCache[index][0].AddRange(suggestedItems);
                                if (hasMainHand && hasOffHand)
                                {
                                    break;
                                }
                            }
                        }
                    }


                }
            }
        }

        count = 0;
        foreach(var unused in _levelField.GetCenteredRange(_config))
        {
            for (var index = 0; index < items.Count; index++)
            {
                var item = items[index];
                if (resultCache.TryGetValue(index, out var dict))
                {
                    if (dict.TryGetValue(count, out var searchResults))
                    {
                        item.SuggestedItems[count] = searchResults;
                    }
                }
            }

            count++;
        }

        return items;
    }

    public List<SearchResult> GetSuggestedItems(EquipmentSuggestItem suggestItem, int level)
    {
        if (applicableItems == null)
        {
            applicableItems = _itemSheet.Where(c => c.Base.EquipSlotCategory.RowId != 0 && c.Base.ClassJobCategory.RowId != 0).ToList();
        }
        var currentClassJob = _classJobFormField.CurrentValue(this._config);
        if (currentClassJob == 0 && _modeSetting.CurrentValue(_configuration) == EquipmentSuggestMode.Class)
        {
            return [];
        }

        var classJob = _classJobSheet.GetRow(currentClassJob);
        var classJobCategory = _classJobCategorySheet.GetRow(classJob.Base.ClassJobCategory.RowId);
        var sourceTypes = _sourceTypesField.CurrentValue(this._config);
        var excludeTypes = _excludeSourcesField.CurrentValue(this._config);
        var filterStats = _filterStatsField.CurrentValue(this._config);
        var items = applicableItems.Where(c => c.Base.LevelEquip == level && c.Base.EquipSlotCategory.RowId != 0);

        if (suggestItem.EquipmentSlot != null)
        {

            switch (suggestItem.EquipmentSlot)
            {
                case EquipSlot.MainHand:
                    items = items.Where(c => c.Base.EquipSlotCategory.Value.MainHand == 1);
                    break;
                case EquipSlot.OffHand:
                    items = items.Where(c => c.Base.EquipSlotCategory.Value.OffHand == 1);
                    break;
                case EquipSlot.Head:
                    items = items.Where(c => c.Base.EquipSlotCategory.Value.Head == 1);
                    break;
                case EquipSlot.Body:
                    items = items.Where(c => c.Base.EquipSlotCategory.Value.Body == 1);
                    break;
                case EquipSlot.Gloves:
                    items = items.Where(c => c.Base.EquipSlotCategory.Value.Gloves == 1);
                    break;
                case EquipSlot.Waist:
                    return [];
                case EquipSlot.Legs:
                    items = items.Where(c => c.Base.EquipSlotCategory.Value.Legs == 1);
                    break;
                case EquipSlot.Feet:
                    items = items.Where(c => c.Base.EquipSlotCategory.Value.Feet == 1);
                    break;
                case EquipSlot.Ears:
                    items = items.Where(c => c.Base.EquipSlotCategory.Value.Ears == 1);
                    break;
                case EquipSlot.Neck:
                    items = items.Where(c => c.Base.EquipSlotCategory.Value.Neck == 1);
                    break;
                case EquipSlot.Wrists:
                    items = items.Where(c => c.Base.EquipSlotCategory.Value.Wrists == 1);
                    break;
                case EquipSlot.FingerR:
                    items = items.Where(c => c.Base.EquipSlotCategory.Value.FingerR == 1);
                    break;
                case EquipSlot.FingerL:
                    items = items.Where(c => c.Base.EquipSlotCategory.Value.FingerL == 1);
                    break;
                default:
                    return [];
            }
            items = items.Where(c =>
                c.ClassJobCategory != null && c.ClassJobCategory.ClassJobIds.Contains(currentClassJob));
            if (filterStats)
            {
                if (classJobCategory.IsCombat)
                {
                    items = items.Where(c =>
                        c.Base.BaseParam.All(c =>
                            c.RowId is not 10 and not 11 and not 70 and not 71 and not 72 and not 73));
                }

                if (classJobCategory.IsGathering)
                {
                    items = items.Where(c => c.Base.BaseParam.Any(c => c.RowId is 10 or 72 or 73));
                }

                if (classJobCategory.IsCrafting)
                {
                    items = items.Where(c => c.Base.BaseParam.Any(c => c.RowId is 11 or 70 or 71));
                }
            }
        }
        else if (suggestItem.ClassJobRow != null)
        {
            items = items.Where(c => c.Base.EquipSlotCategory.Value.MainHand == 1 ||  c.Base.EquipSlotCategory.Value.OffHand == 1);
            items = items.Where(c => c.ClassJobCategory?.ClassJobIds.Contains(suggestItem.ClassJobRow.RowId) ?? false);
        }

        return items.Where(c => c.Sources.Count != 0  && c.Base.BaseParam.Any(baseParam => baseParam.RowId != 0)).Where(c => c.HasSourcesByType(sourceTypes.ToArray())).Where(c => !c.HasSourcesByType(excludeTypes.ToArray())).Select(c => new SearchResult(c)).ToList();
    }
}