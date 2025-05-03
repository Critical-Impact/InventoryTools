using System.Collections.Generic;
using System.Linq;
using AllaganLib.Shared.Extensions;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;
using InventoryTools.Extensions;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns.Abstract.ColumnSettings;

public abstract class MultiChoiceColumnSetting<T> : ColumnSetting<List<T>?>
{
    public ILogger Logger { get; }
    public ImGuiService ImGuiService { get; }

    public virtual string EmptyText { get; } = "";

    private string _filter = "";
    private bool _forceScroll = false;
    private int _selectedIdx = -1;
    private bool _needsScrollFix = false;

    public MultiChoiceColumnSetting(ILogger logger, ImGuiService imGuiService)
    {
        Logger = logger;
        ImGuiService = imGuiService;
    }

    public override bool HasValueSet(ColumnConfiguration configuration)
    {
        var keyValuePair = CurrentValue(configuration);
        if (DefaultValue == null)
            return keyValuePair != null;

        return keyValuePair != null && !keyValuePair.ToHashSet().SetEquals(DefaultValue.ToHashSet());
    }

    public abstract List<T> GetChoices(ColumnConfiguration configuration);
    public abstract string GetFormattedChoice(ColumnConfiguration filterConfiguration, T choice);

    public override bool DrawFilter(ColumnConfiguration configuration, string? helpText)
    {
        var success = false;
        var choices = GetChoices(configuration);
        var activeChoices = CurrentValue(configuration);

        var displayText = activeChoices != null
            ? string.Join(", ", activeChoices.Select(c => GetFormattedChoice(configuration, c)))
            : Name;

        ImGui.SetNextItemWidth(InputSize);
        var currentSearchCategory = activeChoices != null ? string.Join(", ", activeChoices.Select(activeChoice => GetFormattedChoice(configuration, activeChoice))) : Name;
        using (var combo = ImRaii.Combo("##" + Key + "Combo", currentSearchCategory))
        {
            if (combo.Success)
            {
                var isWindowAppearing = ImGui.IsWindowAppearing();
                if (isWindowAppearing)
                {
                    ImGui.SetKeyboardFocusHere();
                    _needsScrollFix = true;
                    _filter = "";
                    _selectedIdx = -1;
                }

                if (_needsScrollFix && ImGui.GetScrollY() > 0)
                {
                    ImGui.SetScrollY(0);
                    _needsScrollFix = false;
                }

                ImGui.SetNextItemWidth(-1);
                var tmp = _filter;
                var searching = false;

                if (ImGui.InputTextWithHint("##filter", "Filter...", ref tmp, 255,
                        ImGuiInputTextFlags.EnterReturnsTrue))
                {
                    var filtered = FilterChoices(choices, configuration, tmp);
                    if (_selectedIdx >= 0 && _selectedIdx < filtered.Count)
                    {
                        ToggleSelection(filtered[_selectedIdx], configuration);
                        configuration.IsDirty = true;
                        success = true;
                    }
                    ImGui.CloseCurrentPopup();
                }


                if (ImGui.IsItemActive())
                {
                    searching = true;
                    if (ImGui.IsKeyPressed(ImGuiKey.DownArrow))
                    {
                        _selectedIdx++;
                        _forceScroll = true;
                    }
                    else if (ImGui.IsKeyPressed(ImGuiKey.UpArrow))
                    {
                        _selectedIdx--;
                        _forceScroll = true;
                        if (_selectedIdx < 0)
                        {
                            _selectedIdx = 0;
                        }
                    }
                }

                var filteredChoices = FilterChoices(choices, configuration, tmp);
                if (_selectedIdx >= filteredChoices.Count)
                {
                    _selectedIdx = filteredChoices.Count - 1;
                }

                _filter = tmp;

                if (!string.IsNullOrWhiteSpace(EmptyText))
                {
                    if (ImGui.Selectable(EmptyText, displayText == ""))
                    {
                        ResetFilter(configuration);
                        configuration.IsDirty = true;
                        success = true;
                    }
                }

                for (var i = 0; i < filteredChoices.Count; i++)
                {
                    var item = filteredChoices[i];
                    var label = GetFormattedChoice(configuration, item).Replace("\u0002\u001F\u0001\u0003", "-");
                    if (string.IsNullOrWhiteSpace(label))
                        continue;

                    bool isSelected = activeChoices?.Contains(item) ?? false;
                    bool isHighlighted = searching && _selectedIdx == i;

                    if (ImGui.Selectable(label, isSelected || isHighlighted))
                    {
                    }

                    if (ImGui.IsItemClicked())
                    {
                        ToggleSelection(item, configuration);
                        configuration.IsDirty = true;
                        success = true;
                    }

                    if (_selectedIdx == i && !ImGui.IsItemVisible() && _forceScroll)
                    {
                        ImGui.SetScrollHereY();
                    }
                }
                _forceScroll = false;
            }
        }

        ImGui.SameLine();
        ImGuiService.HelpMarker(HelpText);
        if (HasValueSet(configuration) && ShowReset)
        {
            ImGui.SameLine();
            if (ImGui.Button("Reset##" + Key + "Reset"))
            {
                ResetFilter(configuration);
                success = true;
            }
        }

        return success;
    }

    private void ToggleSelection(T item, ColumnConfiguration configuration)
    {
        var current = CurrentValue(configuration) ?? new List<T>();
        if (current.Contains(item))
            current.Remove(item);
        else
            current.Add(item);

        UpdateColumnConfiguration(configuration, current.Count > 0 ? current : null);
    }

    private List<T> FilterChoices(List<T> choices, ColumnConfiguration config, string filter)
    {
        if (string.IsNullOrEmpty(filter))
            return choices.ToList();

        var filterText = new FilterComparisonText(filter.ToLowerInvariant());
        return choices.Where(c => GetFormattedChoice(config, c).ToLowerInvariant().PassesFilter(filterText)).ToList();
    }

    public override bool Draw(ColumnConfiguration configuration, string? helpText)
    {
        var success = false;

        ImGui.SetNextItemWidth(LabelSize);
        if (HasValueSet(configuration))
        {
            ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.HealerGreen);
            ImGui.LabelText("##" + Key + "Label", Name + ":");
            ImGui.PopStyleColor();
        }
        else
        {
            ImGui.LabelText("##" + Key + "Label", Name + ":");
        }

        var choices = GetChoices(configuration);
        var activeChoices = CurrentValue(configuration);

        var displayText = activeChoices != null
            ? string.Join(", ", activeChoices.Select(c => GetFormattedChoice(configuration, c)))
            : EmptyText;

        ImGui.SameLine();
        ImGui.SetNextItemWidth(InputSize);
        using (var combo = ImRaii.Combo("##" + Key + "Combo", displayText))
        {
            if (combo.Success)
            {
                if (!string.IsNullOrWhiteSpace(EmptyText))
                {
                    if (ImGui.Selectable(EmptyText, displayText == ""))
                    {
                        ResetFilter(configuration);
                        configuration.IsDirty = true;
                        success = true;
                    }
                }

                foreach (var item in choices)
                {
                    var text = GetFormattedChoice(configuration, item).Replace("\u0002\u001F\u0001\u0003", "-");
                    if (string.IsNullOrWhiteSpace(text))
                        continue;

                    bool isSelected = activeChoices?.Contains(item) ?? false;
                    if (ImGui.Selectable(text, isSelected))
                    {
                        ToggleSelection(item, configuration);
                        configuration.IsDirty = true;
                        success = true;
                    }
                }
            }
        }

        ImGui.SameLine();
        ImGuiService.HelpMarker(HelpText);
        if (HasValueSet(configuration) && ShowReset)
        {
            ImGui.SameLine();
            if (ImGui.Button("Reset##" + Key + "Reset"))
            {
                ResetFilter(configuration);
                success = true;
            }
        }

        return success;
    }
}