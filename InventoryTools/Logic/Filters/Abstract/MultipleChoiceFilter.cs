using System.Collections.Generic;
using System.Linq;
using Dalamud.Interface.Colors;
using ImGuiNET;

namespace InventoryTools.Logic.Filters.Abstract
{
    public abstract class MultipleChoiceFilter<T> : Filter<List<T>> where T : notnull
    {
        public override bool HasValueSet(FilterConfiguration configuration)
        {
            return CurrentValue(configuration).Count != 0;
        }
        public abstract Dictionary<T, string> GetChoices(FilterConfiguration configuration);

        public virtual Dictionary<T, string> GetActiveChoices(FilterConfiguration configuration)
        {
            var choices = GetChoices(configuration);
            if (HideAlreadyPicked)
            {
                var currentChoices = CurrentValue(configuration);
                return choices.Where(c => !currentChoices.Contains(c.Key)).ToDictionary(c => c.Key, c => c.Value);
            }

            return choices;
        }
        
        public abstract bool HideAlreadyPicked { get; set; }

        


        public override void Draw(FilterConfiguration configuration)
        {
            ImGui.SetNextItemWidth(LabelSize);
            if (HasValueSet(configuration))
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
            var activeChoices = GetActiveChoices(configuration);
            var selectedChoices = CurrentValue(configuration);
            
            var currentSearchCategory = "";
            ImGui.SameLine();
            if (ImGui.BeginCombo("##"+Key+"Combo", currentSearchCategory))
            {
                foreach (var item in activeChoices)
                {
                    if (item.Value == "")
                    {
                        continue;
                    }

                    if (ImGui.Selectable(item.Value.Replace("\u0002\u001F\u0001\u0003", "-"), currentSearchCategory == item.Value))
                    {
                        if (!selectedChoices.Contains(item.Key))
                        {
                            selectedChoices.Add(item.Key);
                            UpdateFilterConfiguration(configuration, selectedChoices);
                        }
                    }
                }

                ImGui.EndCombo();
            }
            ImGui.SameLine();
            UiHelpers.HelpMarker(HelpText);
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
        
    }
}