using Dalamud.Interface.Colors;
using ImGuiNET;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Settings.Abstract
{
    public abstract class BooleanSetting : Setting<bool>
    {
        public BooleanSetting(ILogger logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }

        private readonly string[] Choices = new []{"N/A", "Yes", "No"};

        public override void Draw(InventoryToolsConfiguration configuration, string? customName, bool? disableReset,
            bool? disableColouring)
        {
            var currentValue = CurrentValue(configuration);
            if (disableColouring != true && HasValueSet(configuration))
            {
                ImGui.PushStyleColor(ImGuiCol.Text,ImGuiColors.HealerGreen);
                ImGui.LabelText("##" + Key + "Label", customName ?? Name);
                ImGui.PopStyleColor();
            }
            else
            {
                ImGui.LabelText("##" + Key + "Label", customName ?? Name);
            }
            if (ImGui.Checkbox("##"+Key+"Boolean", ref currentValue))
            {
                if (currentValue != CurrentValue(configuration))
                {
                    UpdateFilterConfiguration(configuration, currentValue);
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