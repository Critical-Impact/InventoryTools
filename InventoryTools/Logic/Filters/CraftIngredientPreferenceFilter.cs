using System.Collections.Generic;
using System.Linq;
using AllaganLib.GameSheets.Sheets;
using AllaganLib.GameSheets.Sheets.Rows;
using AllaganLib.Shared.Extensions;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Extensions;
using CriticalCommonLib.Models;
using ImGuiNET;
using InventoryTools.Logic.Filters.Abstract;
using Dalamud.Interface.Utility.Raii;
using InventoryTools.Extensions;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters;

public class CraftIngredientPreferenceFilter : SortedListFilter<(IngredientPreferenceType, uint?), (IngredientPreferenceType, uint?)>
{
    private readonly ItemSheet _itemSheet;

    public CraftIngredientPreferenceFilter(ILogger<CraftIngredientPreferenceFilter> logger, ImGuiService imGuiService, ItemSheet itemSheet) : base(logger, imGuiService)
    {
        _itemSheet = itemSheet;
    }

    public override Dictionary<(IngredientPreferenceType, uint?), (string, string?)> CurrentValue(FilterConfiguration configuration)
    {
        (string, string?) GetIngredientPreferenceDetails((IngredientPreferenceType, uint?) c)
        {
            var itemName = "";
            if (c.Item2 != null)
            {
                var itemRow = _itemSheet.GetRowOrDefault((uint)c.Item2);
                if (itemRow != null)
                {
                    itemName = " (" + itemRow.NameString + ")";
                }
                else
                {
                    itemName = " (Unknown Item)";
                }
            }
            return (c.Item1.FormattedName() + itemName, null);
        }

        return configuration.CraftList.IngredientPreferenceTypeOrder.Distinct().ToDictionary(c => c, GetIngredientPreferenceDetails);
    }

    public override void ResetFilter(FilterConfiguration configuration)
    {
        configuration.CraftList.ResetIngredientPreferences();
        configuration.NotifyConfigurationChange();
    }

    public override void UpdateFilterConfiguration(FilterConfiguration configuration, Dictionary<(IngredientPreferenceType, uint?), (string, string?)> newValue)
    {
        configuration.CraftList.IngredientPreferenceTypeOrder = newValue.Select(c => c.Key).ToList();
        configuration.NotifyConfigurationChange();
    }

    public override string Key { get; set; } = "CraftIngredientPreference";
    public override string Name { get; set; } = "Default Ingredient Sourcing";

    public override string HelpText { get; set; } =
        "When generating the materials for a craft, the 'Ingredient Sourcing' setting determines the preferred method of acquisition. The craft list will refer to this sorted list to determine the appropriate method. Please note that this assumes the item in the craft list can be obtained through this method. If not, the next item in the ingredient sourcing list will be considered.";

    public override FilterCategory FilterCategory { get; set; } = FilterCategory.IngredientSourcing;
    public override Dictionary<(IngredientPreferenceType, uint?), (string, string?)> DefaultValue { get; set; } = new();

    public override bool HasValueSet(FilterConfiguration configuration)
    {
        return true;
    }

    public override FilterType AvailableIn { get; set; } = FilterType.CraftFilter;
    public override bool? FilterItem(FilterConfiguration configuration, InventoryItem item)
    {
        return null;
    }

    public override bool? FilterItem(FilterConfiguration configuration, ItemRow item)
    {
        return null;
    }

    public override bool CanRemove { get; set; } = true;
    public override bool CanRemoveItem(FilterConfiguration configuration, (IngredientPreferenceType, uint?) item)
    {
        return true;
    }

    public override (IngredientPreferenceType, uint?) GetItem(FilterConfiguration configuration, (IngredientPreferenceType, uint?) item)
    {
        return item;
    }

    private List<IngredientPreferenceType> _preferenceTypes = new List<IngredientPreferenceType>()
    {
        IngredientPreferenceType.Botany,
        IngredientPreferenceType.Buy,
        IngredientPreferenceType.Crafting,
        IngredientPreferenceType.Desynthesis,
        IngredientPreferenceType.Fishing,
        IngredientPreferenceType.Gardening,
        IngredientPreferenceType.Marketboard,
        IngredientPreferenceType.Mining,
        IngredientPreferenceType.Mobs,
        IngredientPreferenceType.Reduction,
        IngredientPreferenceType.Venture,
        IngredientPreferenceType.ExplorationVenture,
        IngredientPreferenceType.ResourceInspection,
        IngredientPreferenceType.HouseVendor,
        IngredientPreferenceType.Empty,
    };

    public void AddItem(FilterConfiguration configuration, IngredientPreferenceType type, uint? itemId = null)
    {
        var value = CurrentValue(configuration);
        value.Add((type, itemId), ("", null));
        UpdateFilterConfiguration(configuration, value);
    }


    public override void Draw(FilterConfiguration configuration)
    {
        ImGui.TextUnformatted(Name);
        ImGui.Separator();
        DrawTable(configuration);
        ImGui.SameLine();
        ImGuiService.HelpMarker(HelpText);

        var currentAddColumn = "";
        ImGui.SetNextItemWidth(LabelSize);
        ImGui.LabelText("##" + Key + "Label", "Add new preference: ");
        ImGui.SameLine();
        var currentValue = CurrentValue(configuration);
        using (var combo = ImRaii.Combo("##Add" + Key, currentAddColumn, ImGuiComboFlags.HeightLarge))
        {
            if (combo.Success)
            {
                if (ImGui.Selectable("None", false))
                {
                }
                foreach (var preferenceType in _preferenceTypes.Where(c => !currentValue.ContainsKey((c, null))))
                {
                    var formattedName = preferenceType.FormattedName();
                    if (ImGui.Selectable(formattedName, currentAddColumn == formattedName))
                    {
                        AddItem(configuration,preferenceType);
                    }
                }
            }
        }
        ImGui.SetNextItemWidth(LabelSize);
        ImGui.LabelText("##" + Key + "Label", "Add new item preference: ");
        ImGui.SameLine();
        var currentAddItem = "";
        using (var combo = ImRaii.Combo("##AddItem" + Key, currentAddItem, ImGuiComboFlags.HeightLarge))
        {
            if (combo.Success)
            {
                var searchString = SearchString;
                ImGui.InputText("##ItemSearch", ref searchString, 50);
                if (_searchString != searchString)
                {
                    SearchString = searchString;
                }

                ImGui.Separator();
                if (_searchString == "")
                {
                    ImGui.TextUnformatted("Start typing to search...");
                }
                foreach (var item in SearchItems.Where(c => !currentValue.ContainsKey((IngredientPreferenceType.Item, c.RowId))))
                {
                    var formattedName = item.NameString;
                    if (ImGui.Selectable(formattedName, currentAddItem == formattedName))
                    {
                        AddItem(configuration,IngredientPreferenceType.Item, item.RowId);
                    }
                }
            }
        }
    }

    private string _searchString = "";
    private List<ItemRow>? _searchItems = null;
    public List<ItemRow> SearchItems
    {
        get
        {
            if (SearchString == "")
            {
                _searchItems = new List<ItemRow>();
                return _searchItems;
            }
            if (_searchItems == null)
            {
                _searchItems = _itemSheet.Where(c => c.NameString.ToLower().PassesFilter(SearchString.ToLower())).Take(100)
                    .Select(c => _itemSheet.GetRow(c.RowId)).ToList();
            }

            return _searchItems;
        }
    }

    public string SearchString
    {
        get => _searchString;
        set
        {
            _searchString = value;
            _searchItems = null;
        }
    }
}