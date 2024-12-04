using System.Numerics;
using Dalamud.Interface.Colors;
using ImGuiNET;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;
using OtterGui.Raii;

namespace InventoryTools.Logic.Filters.Abstract
{
    public abstract class ColorFilter : Filter<Vector4?>
    {
        public override Vector4? DefaultValue { get; set; } = null;

        public override bool HasValueSet(FilterConfiguration configuration)
        {
            return CurrentValue(configuration) != null;
        }

        public override Vector4? CurrentValue(FilterConfiguration configuration)
        {
            return configuration.GetColorFilter(Key);
        }

        public override void Draw(FilterConfiguration configuration)
        {
            var value = CurrentValue(configuration) ?? Vector4.Zero;
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
            if (ImGui.ColorEdit4("##" + Key + "Color", ref value, ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.NoLabel))
            {
                UpdateFilterConfiguration(configuration, value);
            }
            if (HasValueSet(configuration))
            {
                ImGui.SameLine();
                if (ImGui.Button("Clear Color"))
                {
                    UpdateFilterConfiguration(configuration, null);
                }
            }
            if (HasValueSet(configuration) && value.W == 0)
            {
                ImGui.SameLine();
                ImGui.TextColored(ImGuiColors.DalamudRed, "The alpha is currently set to 0, this will be invisible.");
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

        public override void UpdateFilterConfiguration(FilterConfiguration configuration, Vector4? newValue)
        {
            if (newValue.HasValue)
            {
                configuration.UpdateColorFilter(Key, newValue.Value);
            }
            else
            {
                configuration.RemoveColorFilter(Key);
            }
        }

        public override void ResetFilter(FilterConfiguration configuration)
        {
            UpdateFilterConfiguration(configuration, DefaultValue);
        }

        protected ColorFilter(ILogger logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }
    }
}