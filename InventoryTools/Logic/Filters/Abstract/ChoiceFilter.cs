using System.Collections.Generic;
using Dalamud.Interface.Colors;
using ImGuiNET;
using Dalamud.Interface.Utility.Raii;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters.Abstract
{
    public abstract class ChoiceFilter<T> : Filter<T?>
    {
        public override bool HasValueSet(FilterConfiguration configuration)
        {
            var keyValuePair = CurrentValue(configuration);
            return keyValuePair != null && !Equals(keyValuePair, DefaultValue);
        }

        public abstract List<T> GetChoices(FilterConfiguration configuration);

        public abstract string GetFormattedChoice(FilterConfiguration filterConfiguration, T choice);

        public override void Draw(FilterConfiguration configuration)
        {
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

            ImGui.Indent();
            using (ImRaii.PushColor(ImGuiCol.Text, ImGuiColors.DalamudGrey))
            {
                ImGui.PushTextWrapPos();
                ImGui.TextUnformatted(HelpText);
                ImGui.PopTextWrapPos();
            }

            var choices = GetChoices(configuration);
            var activeChoice = CurrentValue(configuration);

            var currentSearchCategory = activeChoice != null ? GetFormattedChoice(configuration, activeChoice) : "";
            ImGui.SetNextItemWidth(InputSize);
            using (var combo = ImRaii.Combo("##" + Key + "Combo", currentSearchCategory))
            {
                if (combo.Success)
                {
                    foreach (var item in choices)
                    {
                        if (item == null)
                        {
                            if (ImGui.Selectable("", currentSearchCategory == ""))
                            {
                                UpdateFilterConfiguration(configuration, item);
                            }
                        }

                        var text = GetFormattedChoice(configuration, item).Replace("\u0002\u001F\u0001\u0003", "-");
                        if (text == "")
                        {
                            continue;
                        }

                        if (ImGui.Selectable(text, currentSearchCategory == text))
                        {
                            UpdateFilterConfiguration(configuration, item);
                        }
                    }
                }
            }

            if (HasValueSet(configuration) && ShowReset)
            {
                ImGui.SameLine();
                if (ImGui.Button("Reset##" + Key + "Reset"))
                {
                    ResetFilter(configuration);
                }
            }
            ImGui.Unindent();
        }

        protected ChoiceFilter(ILogger logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }
    }
}