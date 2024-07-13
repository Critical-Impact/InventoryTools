using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Settings.Abstract
{
    public abstract class IntegerSetting : Setting<int>
    {
        public IntegerSetting(ILogger logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }
        public override void Draw(InventoryToolsConfiguration configuration)
        {
            var value = CurrentValue(configuration).ToString();
            ImGui.SetNextItemWidth(LabelSize);
            if (ColourModified && HasValueSet(configuration))
            {
                ImGui.PushStyleColor(ImGuiCol.Text,ImGuiColors.HealerGreen);
                ImGui.LabelText("##" + Key + "Label", Name);
                ImGui.PopStyleColor();
            }
            else
            {
                ImGui.LabelText("##" + Key + "Label", Name);
            }
            ImGui.SetNextItemWidth(InputSize);
            if (ImGui.InputText("##"+Key+"Input", ref value, 100, ImGuiInputTextFlags.CharsDecimal))
            {
                int parsedNumber;
                if(int.TryParse(value, out parsedNumber))
                {
                    UpdateFilterConfiguration(configuration, parsedNumber);
                }
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