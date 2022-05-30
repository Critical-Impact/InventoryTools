using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Interface.Colors;
using ImGuiNET;

namespace InventoryTools.Logic.Settings.Abstract
{
    public abstract class ChoiceSetting<T> : Setting<T> where T : IComparable
    {
        public abstract Dictionary<T, string> Choices { get; }

        public virtual string GetFormattedChoice(T choice)
        {
            return Choices.SingleOrDefault(c => c.Key.Equals(choice)).Value;
        }
        
        public override void Draw(InventoryToolsConfiguration configuration)
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

            var choices = Choices;
            var activeChoice = CurrentValue(configuration);

            var currentSearchCategory = GetFormattedChoice(activeChoice);
            ImGui.SameLine();
            if (ImGui.BeginCombo("##" + Key + "Combo", currentSearchCategory))
            {
                foreach (var item in choices)
                {
                    var text = item.Value.Replace("\u0002\u001F\u0001\u0003", "-");
                    if (text == "")
                    {
                        continue;
                    }                    

                    if (ImGui.Selectable(text,currentSearchCategory == text))
                    {
                        UpdateFilterConfiguration(configuration,item.Key);
                    }
                }

                ImGui.EndCombo();
            }

            ImGui.SameLine();
            UiHelpers.HelpMarker(HelpText);
            if (HasValueSet(configuration))
            {
                ImGui.SameLine();
                if (ImGui.Button("Reset##" + Key + "Reset"))
                {
                    Reset(configuration);
                }
            }
        }
    }
}