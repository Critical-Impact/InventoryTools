using Dalamud.Interface.Colors;
using ImGuiNET;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Settings.Abstract
{
    public abstract class StringSetting : Setting<string>
    {
        public StringSetting(ILogger logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }
        public override bool HasValueSet(InventoryToolsConfiguration configuration)
        {
            return CurrentValue(configuration) != "";
        }

        public override void Draw(InventoryToolsConfiguration configuration)
        {
            var value = CurrentValue(configuration) ?? "";
            ImGui.SetNextItemWidth(LabelSize);
            if (ColourModified && HasValueSet(configuration))
            {
                ImGui.PushStyleColor(ImGuiCol.Text,ImGuiColors.HealerGreen);
                ImGui.LabelText("##" + Key + "Label", Name + ":");
                ImGui.PopStyleColor();
            }
            else
            {
                ImGui.LabelText("##" + Key + "Label", Name + ":");
            }
            ImGui.SameLine();
            if (ImGui.InputText("##"+Key+"Input", ref value, 500))
            {
                UpdateFilterConfiguration(configuration, value);
            }
            ImGui.SameLine();
            ImGuiService.HelpMarker(HelpText, Image, ImageSize);
            if (!HideReset && HasValueSet(configuration))
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