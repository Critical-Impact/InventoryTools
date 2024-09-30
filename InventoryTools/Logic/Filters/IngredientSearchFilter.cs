using System;
using System.Collections.Generic;
using System.Linq;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using CriticalCommonLib.Sheets;
using ImGuiNET;
using InventoryTools.Logic.Filters.Abstract;
using Dalamud.Interface.Utility.Raii;
using InventoryTools.Lists;
using InventoryTools.Services;
using InventoryTools.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters;

public class IngredientSearchFilter : UintMultipleChoiceFilter
{
    private readonly IListService _listService;
    private readonly ExcelCache _excelCache;
    private readonly Lazy<ListFilterService> _listFilterService;

    public IngredientSearchFilter(ILogger<IngredientSearchFilter> logger, ImGuiService imGuiService,IListService listService, ExcelCache excelCache, Lazy<ListFilterService> listFilterService) : base(logger, imGuiService)
    {
        _listService = listService;
        _excelCache = excelCache;
        _listFilterService = listFilterService;
    }
    public override string Key { get; set; } = "IngredientSearchFilter";
    public override string Name { get; set; } = "Ingredient Search Filter";

    public override string HelpText { get; set; } = "Select craftable items and the filter will determine the ingredients used in the craft and will only list those ingredients. The add all from filter button will add all the items from the selected filter to the list.";

    public override FilterCategory FilterCategory { get; set; } = FilterCategory.Searching;

    public override List<uint> DefaultValue { get; set; } = new();

    public override FilterType AvailableIn { get; set; } = FilterType.SearchFilter | FilterType.SortingFilter |
                                                           FilterType.GameItemFilter;
    public override bool? FilterItem(FilterConfiguration configuration, InventoryItem item)
    {
        return FilterItem(configuration, item.Item);
    }

    public override bool? FilterItem(FilterConfiguration configuration, ItemEx item)
    {
        var searchItems = CurrentValue(configuration).ToList();
        if (searchItems.Count == 0)
        {
            return null;
        }

        foreach (var searchItem in searchItems)
        {
            if (GetRelatedIngredients(searchItem).Contains(item.RowId))
            {
                return true;
            }
        }

        return false;
    }

    public override void DrawSearchBox(FilterConfiguration configuration)
    {
        base.DrawSearchBox(configuration);
        ImGui.SameLine();
        if (ImGui.Button("Add all from filter"))
        {
            ImGui.OpenPopup("AddAllFilterSelect");
        }
        
        var currentValue = CurrentValue(configuration).ToHashSet();
        using (var popup = ImRaii.Popup("AddAllFilterSelect"))
        {
            if (popup.Success)
            {
                var filters =
                    _listService.Lists.Where(c =>
                        c.FilterType is Logic.FilterType.SearchFilter or FilterType.SortingFilter or FilterType.GameItemFilter && c != configuration).ToArray();
                foreach (var filter in filters)
                {
                    if (ImGui.Selectable("Add all from " + filter.Name))
                    {
                        var filterResult = _listFilterService.Value.RefreshList(filter);
                        foreach (var item in filterResult)
                        {
                            if (item.Item.CanBeCrafted && item.Item.SearchString != "")
                            {
                                currentValue.Add(item.Item.RowId);
                            }
                        }
                        UpdateFilterConfiguration(configuration,currentValue.ToList());
                    }
                }
            }
        }
    }

    private Dictionary<uint, HashSet<uint>>? _relatedCrafts;

    public HashSet<uint> GetRelatedIngredients(uint itemId)
    {
        if (_relatedCrafts == null)
        {
            _relatedCrafts = new Dictionary<uint, HashSet<uint>>();
        }

        if (!_relatedCrafts.ContainsKey(itemId))
        {
            var ingredients = new HashSet<uint>();
            var craftList = new CraftList();
            craftList.AddCraftItem(itemId);
            craftList.GenerateCraftChildren();
            foreach (var material in craftList.GetFlattenedMaterials())
            {
                if (!material.IsOutputItem)
                {
                    ingredients.Add(material.ItemId);
                }
            }

            _relatedCrafts[itemId] = ingredients;
        }

        return _relatedCrafts[itemId];
    }

    public override bool FilterSearch(uint itemId, string itemName, string searchString)
    {
        if (!_excelCache.CanCraftItem(itemId))
        {
            return false;
        }
        return base.FilterSearch(itemId, itemName, searchString);
    }

    public override Dictionary<uint, string> GetChoices(FilterConfiguration configuration)
    {
        return _excelCache.ItemNamesById;
    }

    public override bool HideAlreadyPicked { get; set; } = true;
    public override int? ResultLimit { get; } = 50;
}