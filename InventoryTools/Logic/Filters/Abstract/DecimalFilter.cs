using Dalamud.Interface.Colors;
using ImGuiNET;

namespace InventoryTools.Logic.Filters.Abstract
{
    public abstract class DecimalFilter : Filter<decimal?>
    {
        public override bool HasValueSet(FilterConfiguration configuration)
        {
            return CurrentValue(configuration) != null;
        }
        
        public override decimal? CurrentValue(FilterConfiguration configuration)
        {
            return configuration.GetDecimalFilter(Key);
        }

        public override void Draw(FilterConfiguration configuration)
        {
            var value = CurrentValue(configuration)?.ToString() ?? "";
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
            if (ImGui.InputText("##"+Key+"Input", ref value, 100, ImGuiInputTextFlags.CharsDecimal))
            {
                int parsedNumber;
                if(int.TryParse(value, out parsedNumber))
                {
                    UpdateFilterConfiguration(configuration, parsedNumber);
                }
                else
                {
                    UpdateFilterConfiguration(configuration, null);
                }
            }
            ImGui.SameLine();
            UiHelpers.HelpMarker(HelpText);
        }

        public override void UpdateFilterConfiguration(FilterConfiguration configuration, decimal? newValue)
        {
            configuration.UpdateDecimalFilter(Key, newValue);
        }
        
        public override void ResetFilter(FilterConfiguration configuration)
        {
            UpdateFilterConfiguration(configuration, null);
        }
    }
}