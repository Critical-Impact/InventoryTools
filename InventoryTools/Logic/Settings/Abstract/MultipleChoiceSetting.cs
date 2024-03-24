using System.Collections.Generic;
using System.Linq;
using CriticalCommonLib.Extensions;
using Dalamud.Interface.Colors;
using FFXIVClientStructs.FFXIV.Common.Math;
using ImGuiNET;
using InventoryTools.Extensions;
using Dalamud.Interface.Utility.Raii;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Settings.Abstract;

public abstract class MultipleChoiceSetting<T> : Setting<List<T>> where T:notnull
{
    public MultipleChoiceSetting(ILogger logger, ImGuiService imGuiService) : base(logger, imGuiService)
    {
    }
    public override void Draw(InventoryToolsConfiguration configuration)
    {
        DrawSearchBox(configuration);
        DrawResults(configuration);
    }
    
    public virtual void DrawSearchBox(InventoryToolsConfiguration configuration)
    {
        ImGui.SetNextItemWidth(LabelSize);
        if (ColourModified && HasValueSet(configuration))
        {
            ImGui.PushStyleColor(ImGuiCol.Text,ImGuiColors.HealerGreen);
            ImGui.LabelText("##" + Key + "Label", Name + ":");
            ImGui.PopStyleColor();
        }
        else
        {
            ImGui.LabelText("##" + Key + "Label", Name + ":");
        }

        var choices = GetChoices(configuration);
        var selectedChoices = CurrentValue(configuration);
        var currentSearchCategory = "";
        ImGui.SameLine();
        ImGui.SetNextItemWidth(InputSize);
        using (var combo = ImRaii.Combo("##"+Key+"Combo", currentSearchCategory, ImGuiComboFlags.HeightLarge))
        {
            if (combo.Success)
            {
                var searchString = SearchString;
                ImGui.InputText("Start typing to search..##ItemSearch", ref searchString, 50);
                if (_searchString != searchString)
                {
                    SearchString = searchString;
                }
                var activeChoices = GetActiveChoices(configuration);
                ImGui.SameLine();
                if (ImGui.Button("Add All"))
                {
                    foreach (var item in activeChoices)
                    {
                        if (!selectedChoices.Contains(item.Key))
                        {
                            selectedChoices.Add(item.Key);
                        }
                    }

                    _cachedChoices = null;
                    UpdateFilterConfiguration(configuration, selectedChoices);
                }
                ImGui.Separator();
                using (ImRaii.Child("searchBox", new Vector2(0, 250)))
                {
                    foreach (var item in activeChoices)
                    {
                        if (item.Value == "")
                        {
                            continue;
                        }

                        if (ImGui.Selectable(item.Value.Replace("\u0002\u001F\u0001\u0003", "-"),
                                currentSearchCategory == item.Value))
                        {
                            if (!selectedChoices.Contains(item.Key))
                            {
                                selectedChoices.Add(item.Key);
                                UpdateFilterConfiguration(configuration, selectedChoices);
                                _cachedChoices = null;
                            }
                        }
                    }
                }
            }
        }
        ImGui.SameLine();
        ImGuiService.HelpMarker(HelpText, Image, ImageSize);
        if (!HideReset && HasValueSet(configuration))
        {
            ImGui.SameLine();
            if (ImGui.Button("Reset##" + Key + "Reset"))
            {
                Reset(configuration);
            }
        }
    }

    public virtual void DrawResults(InventoryToolsConfiguration configuration)
    {
        var choices = GetChoices(configuration);
        var selectedChoices = CurrentValue(configuration);

        for (var index = 0; index < selectedChoices.Count; index++)
        {
            var item = selectedChoices[index];
            var actualItem = choices.ContainsKey(item) ? choices[item] : null;
            var selectedChoicesCount = selectedChoices.Count;
            if (actualItem != null)
            {
                var itemSearchCategoryName = actualItem
                    .Replace("\u0002\u001F\u0001\u0003", "-");
                if (ImGui.Button(itemSearchCategoryName + " X" + "##" + Key + index))
                {
                    if (selectedChoices.Contains(item))
                    {
                        selectedChoices.Remove(item);
                        UpdateFilterConfiguration(configuration, selectedChoices);
                    }
                }
            }

            if (index != selectedChoicesCount - 1 &&
                (index % 4 != 0 || index == 0))
            {
                ImGui.SameLine();
            }
        }
    }
    
    public abstract Dictionary<T, string> GetChoices(InventoryToolsConfiguration configuration);

    private Dictionary<T, string>? _cachedChoices;
    public virtual Dictionary<T, string> GetActiveChoices(InventoryToolsConfiguration configuration)
    {
        if (_cachedChoices != null)
        {
            return _cachedChoices;
        }
        var choices = GetChoices(configuration);
        IEnumerable<KeyValuePair<T, string>> filteredChoices;
        var searchString = SearchString.ToParseable();
        if (HideAlreadyPicked)
        {
            var currentChoices = CurrentValue(configuration);
            filteredChoices = choices.Where(c => FilterSearch(c.Key, c.Value, searchString) && !currentChoices.Contains(c.Key));
                
        }
        else
        {
            filteredChoices = choices.Where(c => FilterSearch(c.Key, c.Value, searchString));
        }

        if (ResultLimit != null)
        {
            filteredChoices = filteredChoices.Take(ResultLimit.Value);
        }

        _cachedChoices = filteredChoices.ToDictionary(c => c.Key, c => c.Value);;
        return _cachedChoices;
    }
    
    public abstract bool HideAlreadyPicked { get; set; }

    public virtual int? ResultLimit { get; } = null;

        
    private string _searchString = "";

    public virtual bool FilterSearch(T itemId, string itemName, string searchString)
    {
        if (searchString == "")
        {
            return true;
        }

        return itemName.ToParseable().PassesFilter(searchString);
    }
    
    public string SearchString
    {
        get => _searchString;
        set
        {
            _searchString = value;
            _cachedChoices = null;
        }
    }
    
    public override bool HasValueSet(InventoryToolsConfiguration configuration)
    {
        var currentValue = CurrentValue(configuration).Distinct().ToHashSet();
        return !currentValue.SetEquals(DefaultValue.Distinct().ToHashSet());
    }

    public override void Reset(InventoryToolsConfiguration configuration)
    {
        base.Reset(configuration);
    }
}