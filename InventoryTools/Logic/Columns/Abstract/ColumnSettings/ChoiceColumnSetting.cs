using System.Collections.Generic;
using System.Linq;
using AllaganLib.Shared.Extensions;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns.Abstract.ColumnSettings;

public abstract class ChoiceColumnSetting<T> : ColumnSetting<T?>
{
    public ILogger Logger { get; }
    public ImGuiService ImGuiService { get; }

    public virtual string EmptyText { get; } = "";

    private string _filter = "";
    private bool _forceScroll = false;
    private int _selectedIdx = -1;

    public ChoiceColumnSetting(ILogger logger, ImGuiService imGuiService)
    {
        Logger = logger;
        ImGuiService = imGuiService;
    }
    public override bool HasValueSet(ColumnConfiguration configuration)
    {
        var keyValuePair = CurrentValue(configuration);
        return keyValuePair != null && !Equals(keyValuePair, DefaultValue);
    }

    public abstract List<T> GetChoices(ColumnConfiguration configuration);

    public abstract string GetFormattedChoice(ColumnConfiguration filterConfiguration, T choice);

    public override bool DrawFilter(ColumnConfiguration configuration, string? helpText)
    {
        var success = false;

        var choices = GetChoices(configuration);
        var activeChoice = CurrentValue(configuration);

        var currentSearchCategory = activeChoice != null ? GetFormattedChoice(configuration, activeChoice) : Name;
        ImGui.SameLine();
        ImGui.SetNextItemWidth(InputSize);
        using (var combo = ImRaii.Combo("##" + Key + "Combo", currentSearchCategory))
        {
            if (combo.Success)
            {
                if (ImGui.IsWindowAppearing())
                {
                    ImGui.SetKeyboardFocusHere();
                    _selectedIdx = -1;
                    _filter = "";
                }

                ImGui.SetNextItemWidth(-1);
                var tmp   = _filter;
                var searching = false;
                if (ImGui.InputTextWithHint("##filter", "Filter...", ref tmp, 255,
                        ImGuiInputTextFlags.EnterReturnsTrue))
                {
                    var selectedText = new FilterComparisonText(tmp.ToLowerInvariant());
                    var selectedChoice = tmp == string.Empty ? choices.ToList() : choices.Where(c => GetFormattedChoice(configuration, c).ToLowerInvariant().PassesFilter(selectedText)).ToList();
                    if (_selectedIdx >= 0 && _selectedIdx < selectedChoice.Count)
                    {
                        UpdateColumnConfiguration(configuration, selectedChoice[_selectedIdx]);
                        configuration.IsDirty = true;
                        success = true;
                        ImGui.CloseCurrentPopup();
                    }
                }

                if (ImGui.IsItemActive())
                {
                    searching = true;
                    if (ImGui.IsKeyDown(ImGuiKey.DownArrow))
                    {
                        _selectedIdx++;
                        _forceScroll = true;
                        if (_selectedIdx < 0)
                        {
                            _selectedIdx = 0;
                        }
                    }
                    else if (ImGui.IsKeyDown(ImGuiKey.UpArrow))
                    {
                        _selectedIdx--;
                        _forceScroll = true;
                    }
                }

                if (EmptyText != "")
                {
                    if (ImGui.Selectable(EmptyText, currentSearchCategory == ""))
                    {
                        ResetFilter(configuration);
                        configuration.IsDirty = true;
                        success = true;
                    }
                }

                var filterText = new FilterComparisonText(tmp.ToLowerInvariant());
                var filteredChoices = tmp == string.Empty ? choices.ToList() : choices.Where(c => GetFormattedChoice(configuration, c).ToLowerInvariant().PassesFilter(filterText)).ToList();

                if (_selectedIdx >= filteredChoices.Count)
                {
                    _selectedIdx = filteredChoices.Count - 1;
                }
                for (var index = 0; index < filteredChoices.Count; index++)
                {
                    var item = filteredChoices[index];
                    var text = GetFormattedChoice(configuration, item).Replace("\u0002\u001F\u0001\u0003", "-");
                    if (text == "")
                    {
                        continue;
                    }


                    if (ImGui.Selectable(text, (!searching && currentSearchCategory == text) || (searching && _selectedIdx == index)))
                    {

                    }

                    //Selectable seems to break otherwise
                    if (ImGui.IsItemClicked())
                    {
                        UpdateColumnConfiguration(configuration, item);
                        configuration.IsDirty = true;
                        success = true;
                        ImGui.CloseCurrentPopup();
                    }

                    if (_selectedIdx == index && !ImGui.IsItemVisible() && _forceScroll)
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
        var activeChoice = CurrentValue(configuration);

        var currentSearchCategory = activeChoice != null ? GetFormattedChoice(configuration, activeChoice) : EmptyText;
        ImGui.SameLine();
        ImGui.SetNextItemWidth(InputSize);
        using (var combo = ImRaii.Combo("##" + Key + "Combo", currentSearchCategory))
        {
            if (combo.Success)
            {
                if (EmptyText != "")
                {
                    if (ImGui.Selectable(EmptyText, currentSearchCategory == ""))
                    {
                        ResetFilter(configuration);
                        configuration.IsDirty = true;
                        success = true;
                    }
                }
                foreach (var item in choices)
                {


                    var text = GetFormattedChoice(configuration, item).Replace("\u0002\u001F\u0001\u0003", "-");
                    if (text == "")
                    {
                        continue;
                    }

                    if (ImGui.Selectable(text, currentSearchCategory == text))
                    {
                        UpdateColumnConfiguration(configuration, item);
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