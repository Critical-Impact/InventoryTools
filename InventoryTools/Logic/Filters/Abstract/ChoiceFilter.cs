using System.Collections.Generic;
using Dalamud.Interface.Colors;
using ImGuiNET;
using Dalamud.Interface.Utility.Raii;

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

        public abstract string GetFormattedChoice(T choice);

        public override void Draw(FilterConfiguration configuration)
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

            var currentSearchCategory = activeChoice != null ? GetFormattedChoice(activeChoice) : "";
            ImGui.SameLine();
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

                        var text = GetFormattedChoice(item).Replace("\u0002\u001F\u0001\u0003", "-");
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

            ImGui.SameLine();
            UiHelpers.HelpMarker(HelpText);
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
}