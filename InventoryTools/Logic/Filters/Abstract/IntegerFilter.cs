using Dalamud.Interface.Colors;
using ImGuiNET;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;
using OtterGui.Raii;

namespace InventoryTools.Logic.Filters.Abstract
{
    public abstract class IntegerFilter : Filter<int?>
    {
        public override int? DefaultValue { get; set; } = null;

        public override bool HasValueSet(FilterConfiguration configuration)
        {
            return CurrentValue(configuration) != null;
        }

        public override int? CurrentValue(FilterConfiguration configuration)
        {
            return configuration.GetIntegerFilter(Key);
        }

        public override void Draw(FilterConfiguration configuration)
        {
            var value = CurrentValue(configuration)?.ToString() ?? "";
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
            ImGui.Indent();
            using (ImRaii.PushColor(ImGuiCol.Text, ImGuiColors.DalamudGrey))
            {
                ImGui.PushTextWrapPos();
                ImGui.TextUnformatted(HelpText);
                ImGui.PopTextWrapPos();
            }
            ImGui.SetNextItemWidth(InputSize);
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
            if (HasValueSet(configuration) && ShowReset)
            {
                ImGui.SameLine();
                if (ImGui.Button("Reset##" + Key + "Reset"))
                {
                    ResetFilter(configuration);
                }
            }

            ImGui.Unindent();
        }

        public override void UpdateFilterConfiguration(FilterConfiguration configuration, int? newValue)
        {
            configuration.UpdateIntegerFilter(Key, newValue);
        }

        public override void ResetFilter(FilterConfiguration configuration)
        {
            UpdateFilterConfiguration(configuration, DefaultValue);
        }

        protected IntegerFilter(ILogger logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }
    }
}