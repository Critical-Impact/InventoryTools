using Dalamud.Interface.Colors;
using ImGuiNET;

namespace InventoryTools.Logic.Filters
{
    public abstract class StringFilter : Filter<string>
    {
        public override bool HasValueSet(FilterConfiguration configuration)
        {
            return CurrentValue(configuration) != "";
        }
        
        public override string CurrentValue(FilterConfiguration configuration)
        {
            return (configuration.GetStringFilter(Key) ?? "").Trim();
        }

        public override void Draw(FilterConfiguration configuration)
        {
            var value = CurrentValue(configuration) ?? "";
            ImGui.SetNextItemWidth(200);
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
        }

        public override void UpdateFilterConfiguration(FilterConfiguration configuration, string newValue)
        {
            configuration.UpdateStringFilter(Key, newValue);
        }
    }
}