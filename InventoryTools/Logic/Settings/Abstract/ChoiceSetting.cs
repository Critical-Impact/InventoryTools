using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Interface.Colors;
using ImGuiNET;
using Dalamud.Interface.Utility.Raii;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Settings.Abstract
{
    public abstract class ChoiceSetting<T> : Setting<T> where T : IComparable?
    {
        public ChoiceSetting(ILogger logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }
        public abstract Dictionary<T, string> Choices { get; }

        public virtual string GetFormattedChoice(T choice)
        {
            return Choices.SingleOrDefault(c => c.Key!.Equals(choice)).Value;
        }

        public override void Draw(InventoryToolsConfiguration configuration, string? customName, bool? disableReset,
            bool? disableColouring)
        {
            if (disableColouring != true && HasValueSet(configuration))
            {
                ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.HealerGreen);
                ImGui.LabelText("##" + Key + "Label", customName ?? Name);
                ImGui.PopStyleColor();
            }
            else
            {
                ImGui.LabelText("##" + Key + "Label", customName ?? Name);
            }

            var choices = Choices;
            var activeChoice = CurrentValue(configuration);

            var currentSearchCategory = GetFormattedChoice(activeChoice);
            ImGui.SetNextItemWidth(InputSize);
            using (var combo = ImRaii.Combo("##" + Key + "Combo", currentSearchCategory))
            {
                if (combo.Success)
                {
                    foreach (var item in choices)
                    {
                        var text = item.Value.Replace("\u0002\u001F\u0001\u0003", "-");
                        if (text == "")
                        {
                            continue;
                        }

                        if (ImGui.Selectable(text, currentSearchCategory == text))
                        {
                            UpdateFilterConfiguration(configuration, item.Key);
                        }
                    }
                }
            }

            ImGui.SameLine();
            ImGuiService.HelpMarker(HelpText, Image, ImageSize);
            if (disableReset != true && HasValueSet(configuration))
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