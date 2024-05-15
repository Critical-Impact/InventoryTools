using System.Collections.Generic;
using System.Linq;
using CriticalCommonLib;
using CriticalCommonLib.Extensions;
using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using ImGuiNET;
using InventoryTools.Extensions;
using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;
using OtterGui.Raii;

namespace InventoryTools.Logic.Filters;

public class CraftWorldPricePreference : SortedListFilter<uint, uint>
{
    public CraftWorldPricePreference(ILogger<CraftWorldPricePreference> logger, ImGuiService imGuiService) : base(logger, imGuiService)
    {
    }
    
    public override Dictionary<uint, (string, string?)> CurrentValue(FilterConfiguration configuration)
    {
        (string, string?) GetWorldDetails(uint c)
        {
            string worldName;
            var worldEx = Service.ExcelCache.GetWorldSheet().GetRow(c);
            if (worldEx != null)
            {
                worldName = worldEx.FormattedName;
            }
            else
            {
                worldName = " (Unknown World)";
            }
            return (worldName, null);
        }

        return configuration.CraftList.WorldPricePreference.Distinct().ToDictionary(c => c, GetWorldDetails);
    }

    public override void ResetFilter(FilterConfiguration configuration)
    {
        configuration.CraftList.ResetWorldPricePreferences();
        configuration.NotifyConfigurationChange();
    }

    public override void UpdateFilterConfiguration(FilterConfiguration configuration, Dictionary<uint, (string, string?)> newValue)
    {
        configuration.CraftList.WorldPricePreference = newValue.Select(c => c.Key).ToList();
        configuration.NotifyConfigurationChange();
    }

    public override string Key { get; set; } = "CraftWorldPricePreference";
    public override string Name { get; set; } = "World Price Preference";

    public override string HelpText { get; set; } =
        "Which worlds should prices be sourced from?";

    public override FilterCategory FilterCategory { get; set; } = FilterCategory.WorldPricePreference;
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

    public override bool? FilterItem(FilterConfiguration configuration, ItemEx item)
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
    
    public void AddItem(FilterConfiguration configuration, uint worldId)
    {
        var value = CurrentValue(configuration);
        value.Add(worldId, ("", null));
        UpdateFilterConfiguration(configuration, value);
    }

    public Dictionary<uint, string>? _worlds;

    private Dictionary<uint, string> Worlds
    {
        get
        {
            if (_worlds == null)
            {
                _worlds = Service.ExcelCache.GetWorldSheet().ToDictionary(c => c.RowId, c => c.FormattedName);
            }

            return _worlds;
        }
    }
    
    public override void Draw(FilterConfiguration configuration)
    {
        ImGui.TextUnformatted(Name);
        ImGuiService.HelpMarker(HelpText);
        ImGui.Separator();
        DrawTable(configuration);
        ImGui.SameLine();
        ImGuiService.HelpMarker(HelpText);
        
        var currentValue = CurrentValue(configuration);
        ImGui.SetNextItemWidth(LabelSize);
        ImGui.LabelText("##" + Key + "Label", "Add new world: ");
        ImGui.SameLine();
        var currentAddItem = "";
        using (var combo = ImRaii.Combo("##AddWorld" + Key, currentAddItem, ImGuiComboFlags.HeightLarge))
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
                foreach (var item in SearchWorlds.Where(c => !currentValue.ContainsKey(c.RowId)))
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
    private List<WorldEx>? _searchWorlds = null;
    public List<WorldEx> SearchWorlds
    {
        get
        {
            if (SearchString == "")
            {
                _searchWorlds = new List<WorldEx>();
                return _searchWorlds;
            }
            if (_searchWorlds == null)
            {
                _searchWorlds = Service.ExcelCache.GetWorldSheet().Where(c => c.IsPublic && c.FormattedName.ToParseable().PassesFilter(SearchString.ToParseable())).Take(100)
                    .Select(c => Service.ExcelCache.GetWorldSheet().GetRow(c.RowId)!).ToList();
            }

            return _searchWorlds;
        }
    }
    
    public string SearchString
    {
        get => _searchString;
        set
        {
            _searchString = value;
            _searchWorlds = null;
        }
    }
}
