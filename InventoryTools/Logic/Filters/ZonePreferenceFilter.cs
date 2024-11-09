using System.Collections.Generic;
using System.Linq;
using AllaganLib.GameSheets.Sheets;
using AllaganLib.GameSheets.Sheets.Rows;
using AllaganLib.Shared.Extensions;
using CriticalCommonLib.Extensions;
using CriticalCommonLib.Models;
using ImGuiNET;
using InventoryTools.Logic.Filters.Abstract;
using Dalamud.Interface.Utility.Raii;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters;

public class ZonePreferenceFilter : SortedListFilter<uint, uint>
{
    private readonly MapSheet _mapSheet;

    public ZonePreferenceFilter(ILogger<ZonePreferenceFilter> logger, ImGuiService imGuiService, MapSheet mapSheet) : base(logger, imGuiService)
    {
        _mapSheet = mapSheet;
    }

    public override Dictionary<uint, (string, string?)> CurrentValue(FilterConfiguration configuration)
    {
        (string, string?) GetIngredientPreferenceDetails(uint c)
        {
            string itemName;
            var mapEx = _mapSheet.GetRowOrDefault(c);
            if (mapEx != null)
            {
                itemName = mapEx.FormattedName;
            }
            else
            {
                itemName = " (Unknown Item)";
            }
            return (itemName, null);
        }

        return configuration.CraftList.ZonePreferenceOrder.Distinct().ToDictionary(c => c, GetIngredientPreferenceDetails);
    }

    public override void ResetFilter(FilterConfiguration configuration)
    {
        configuration.CraftList.ResetIngredientPreferences();
        configuration.NotifyConfigurationChange();
    }

    public override void UpdateFilterConfiguration(FilterConfiguration configuration, Dictionary<uint, (string, string?)> newValue)
    {
        configuration.CraftList.ZonePreferenceOrder = newValue.Select(c => c.Key).ToList();
        configuration.NotifyConfigurationChange();
    }

    public override string Key { get; set; } = "CraftZonePreference";
    public override string Name { get; set; } = "Default Zone Order";

    public override string HelpText { get; set; } =
        "When grouping items by zone, which zones should take preference?";

    public override FilterCategory FilterCategory { get; set; } = FilterCategory.ZonePreference;
    public override Dictionary<uint, (string, string?)> DefaultValue { get; set; } = new();

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
    public override bool CanRemoveItem(FilterConfiguration configuration, uint item)
    {
        return true;
    }

    public override uint GetItem(FilterConfiguration configuration, uint item)
    {
        return item;
    }

    public void AddItem(FilterConfiguration configuration, uint mapId)
    {
        var value = CurrentValue(configuration);
        value.Add(mapId, ("", null));
        UpdateFilterConfiguration(configuration, value);
    }

    public Dictionary<uint, string>? _territories;

    private Dictionary<uint, string> Territories
    {
        get
        {
            if (_territories == null)
            {
                _territories = _mapSheet.ToDictionary(c => c.RowId, c => c.FormattedName);
            }

            return _territories;
        }
    }

    public override void Draw(FilterConfiguration configuration)
    {
        ImGui.TextUnformatted(Name);
        ImGui.Separator();
        DrawTable(configuration);
        ImGui.SameLine();
        ImGuiService.HelpMarker(HelpText);

        var currentValue = CurrentValue(configuration);
        ImGui.SetNextItemWidth(LabelSize);
        ImGui.LabelText("##" + Key + "Label", "Add new zone: ");
        ImGui.SameLine();
        var currentAddItem = "";
        using (var combo = ImRaii.Combo("##AddZone" + Key, currentAddItem, ImGuiComboFlags.HeightLarge))
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
                foreach (var item in SearchTerritories.Where(c => !currentValue.ContainsKey(c.RowId)))
                {
                    var formattedName = item.FormattedName;
                    if (ImGui.Selectable(formattedName, currentAddItem == formattedName))
                    {
                        AddItem(configuration,item.RowId);
                    }
                }
            }
        }
    }

    private string _searchString = "";
    private List<MapRow>? _searchTerritories = null;
    public List<MapRow> SearchTerritories
    {
        get
        {
            if (SearchString == "")
            {
                _searchTerritories = new List<MapRow>();
                return _searchTerritories;
            }
            if (_searchTerritories == null)
            {
                _searchTerritories = _mapSheet.Where(c => c.FormattedName.ToParseable().PassesFilter(SearchString.ToParseable())).Take(100).ToList();
            }

            return _searchTerritories;
        }
    }

    public string SearchString
    {
        get => _searchString;
        set
        {
            _searchString = value;
            _searchTerritories = null;
        }
    }
}