using System.Collections.Generic;
using System.Linq;
using AllaganLib.GameSheets.Sheets.Rows;
using CharacterTools.Logic.Editors;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;
using OtterGui;

namespace InventoryTools.Logic.Filters.Abstract;

public abstract class CharacterScopeFilter : Filter<List<CharacterSearchScope>?>
{
    private readonly CharacterScopePicker _scopePicker;
    private readonly CharacterScopeCalculator _scopeCalculator;
    private readonly ICharacterMonitor _characterMonitor;
    private List<Character>? _characters;

    public CharacterScopeFilter(CharacterScopePicker scopePicker, CharacterScopeCalculator scopeCalculator, ICharacterMonitor characterMonitor, ILogger<CharacterScopeFilter> logger, ImGuiService imGuiService) : base(logger, imGuiService)
    {
        _scopePicker = scopePicker;
        _scopeCalculator = scopeCalculator;
        _characterMonitor = characterMonitor;
    }

    public override List<CharacterSearchScope>? CurrentValue(FilterConfiguration configuration)
    {
        configuration.GetFilter(configuration.Key, out List<CharacterSearchScope>? list);
        return list;
    }

    public override bool HasValueSet(FilterConfiguration configuration)
    {
        configuration.GetFilter(configuration.Key, out List<CharacterSearchScope>? searchScopeList);
        return searchScopeList is not null;
    }

    public override bool? FilterItem(FilterConfiguration configuration, InventoryItem item)
    {
        return null;
    }

    public override bool? FilterItem(FilterConfiguration configuration, ItemRow item)
    {
        return null;
    }

    public override void Draw(FilterConfiguration configuration)
    {
        var characterSearchScopes = CurrentValue(configuration) ?? new();
        if (_scopePicker.Draw("##ScopePicker" + configuration.Key, characterSearchScopes))
        {
            this.UpdateFilterConfiguration(configuration, characterSearchScopes);
        }

        if (HelpText != string.Empty)
        {
            ImGui.SameLine();
            using (ImRaii.PushColor(ImGuiCol.Text, ImGuiColors.DalamudWhite))
            {
                ImGui.Text("?");
            }
            ImGuiUtil.HoverTooltip(HelpText);
        }

        var currentValue = CurrentValue(configuration);
        using var disabled = ImRaii.Disabled(currentValue == null);
        if (ImGui.Button("Test Scopes"))
        {
            if (currentValue != null)
            {
                _characters = _characterMonitor.Characters.Where(c => _scopeCalculator.Filter(currentValue, c.Value)).Select(c => _characterMonitor.GetCharacterById(c.Key)!).ToList();

            }
        }

        if (_characters is not null && currentValue is not null)
        {
            ImGui.Separator();
            ImGui.Text("The following characters will be searched in: ");
            foreach (var s in _characters)
            {
                ImGui.TextUnformatted(s.Name);
            }
        }
    }

    public override void ResetFilter(FilterConfiguration configuration)
    {
        UpdateFilterConfiguration(configuration, DefaultValue);
    }

    public override void UpdateFilterConfiguration(FilterConfiguration configuration, List<CharacterSearchScope>? newValue)
    {
        if (newValue is { Count: 0 } or null)
        {
            configuration.SetFilter(Key, (List<CharacterSearchScope>?)null);
            _characters = null;
        }
        else
        {
            configuration.SetFilter(Key, newValue);
            _characters = null;
        }
    }
}