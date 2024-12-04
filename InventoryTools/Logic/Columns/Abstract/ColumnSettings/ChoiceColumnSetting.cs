using System.Collections.Generic;
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
    
    public override void Draw(ColumnConfiguration configuration, string? helpText)
    {
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
            }
        }
    }
}