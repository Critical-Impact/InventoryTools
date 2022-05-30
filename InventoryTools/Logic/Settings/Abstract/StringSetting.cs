using Dalamud.Interface.Colors;
using ImGuiNET;

namespace InventoryTools.Logic.Settings.Abstract
{
    public abstract class StringSetting : Setting<string>
    {
        public override bool HasValueSet(InventoryToolsConfiguration configuration)
        {
            return CurrentValue(configuration) != "";
        }

        public override void Draw(InventoryToolsConfiguration configuration)
        {
            var value = CurrentValue(configuration) ?? "";
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
            ImGui.SameLine();
            if (ImGui.InputText("##"+Key+"Input", ref value, 500))
            {
                UpdateFilterConfiguration(configuration, value);
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