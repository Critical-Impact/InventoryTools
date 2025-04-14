using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using CharacterTools.Logic.Editors;
using CriticalCommonLib.Extensions;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;
using InventoryTools.Logic.Columns.Abstract.ColumnSettings;
using InventoryTools.Logic.Editors;
using InventoryTools.Services;
using OtterGui;

namespace InventoryTools.Logic.Columns.ColumnSettings;

public class CharacterScopePickerColumnSetting : ColumnSetting<List<CharacterSearchScope>?>
{
    private readonly CharacterScopePicker _scopePicker;
    private readonly CharacterScopeCalculator _scopeCalculator;
    private readonly ICharacterMonitor _characterMonitor;
    private List<Character>? _characters;

    public CharacterScopePickerColumnSetting(CharacterScopePicker scopePicker, CharacterScopeCalculator scopeCalculator, ICharacterMonitor characterMonitor)
    {
        _scopePicker = scopePicker;
        _scopeCalculator = scopeCalculator;
        _characterMonitor = characterMonitor;
    }
    public override List<CharacterSearchScope>? CurrentValue(ColumnConfiguration configuration)
    {
        configuration.GetSetting(configuration.Key, out List<CharacterSearchScope>? list);
        return list;
    }

    public override bool HasValueSet(ColumnConfiguration configuration)
    {
        configuration.GetSetting(configuration.Key, out List<CharacterSearchScope>? searchScopeList);
        return searchScopeList is not null;
    }

    public override void UpdateColumnConfiguration(ColumnConfiguration configuration, List<CharacterSearchScope>? newValue)
    {
        if (newValue is { Count: 0 } or null)
        {
            configuration.SetSetting(configuration.Key, (List<CharacterSearchScope>?)null);
            _characters = null;
        }
        else
        {
            configuration.SetSetting(configuration.Key, newValue);
            _characters = null;
        }
    }

    public override string Key { get; set; } = "CharacterScopePicker";
    public override string Name { get; set; } = "Character Search Scope";
    public override string HelpText { get; set; } = "Select the characters you want to search inside.";
    public override List<CharacterSearchScope>? DefaultValue { get; set; } = null;

    public override bool Draw(ColumnConfiguration configuration, string? helpText)
    {
        var success = false;
        var characterSearchScopes = CurrentValue(configuration) ?? new();
        if (_scopePicker.Draw("##ScopePicker" + configuration.Key, characterSearchScopes))
        {
            this.UpdateColumnConfiguration(configuration, characterSearchScopes);
            success = true;
        }

        if (helpText != null)
        {
            ImGui.SameLine();
            using (ImRaii.PushColor(ImGuiCol.Text, ImGuiColors.DalamudWhite))
            {
                ImGui.Text("?");
            }
            ImGuiUtil.HoverTooltip(helpText);
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

        return success;
    }

    public override void ResetFilter(ColumnConfiguration configuration)
    {
        UpdateColumnConfiguration(configuration, DefaultValue);
    }
}