using System.Collections.Generic;
using System.Linq;
using Dalamud.Interface.Colors;
using ImGuiNET;

namespace InventoryTools.Logic.Filters
{
    public abstract class ChoiceFilter<T> : Filter<KeyValuePair<T, string>?>  where T : notnull
    {
        public abstract T EmptyValue { get; set; }
        public override bool HasValueSet(FilterConfiguration configuration)
        {
            var keyValuePair = CurrentValue(configuration);
            return keyValuePair != null && !Equals(keyValuePair.Value.Key, EmptyValue);
        }

        public abstract Dictionary<T, string> GetChoices(FilterConfiguration configuration);

        public override void Draw(FilterConfiguration configuration)
        {
            ImGui.SetNextItemWidth(200);
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

            var currentSearchCategory = activeChoice.HasValue ? activeChoice.Value.Value : "";
            ImGui.SameLine();
            if (ImGui.BeginCombo("##" + Key + "Combo", currentSearchCategory))
            {
                foreach (var item in choices)
                {
                    if (item.Value == "")
                    {
                        continue;
                    }

                    if (ImGui.Selectable(item.Value.Replace("\u0002\u001F\u0001\u0003", "-"),
                        currentSearchCategory == item.Value))
                    {
                        UpdateFilterConfiguration(configuration,item);
                    }
                }

                ImGui.EndCombo();
            }

            ImGui.SameLine();
            UiHelpers.HelpMarker(HelpText);
        }
    }
}