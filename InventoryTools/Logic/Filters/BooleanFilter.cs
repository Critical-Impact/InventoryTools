using Dalamud.Interface.Colors;
using ImGuiNET;

namespace InventoryTools.Logic.Filters
{
    public abstract class BooleanFilter : Filter<bool?>
    {
        public override bool HasValueSet(FilterConfiguration configuration)
        {
            return CurrentValue(configuration) != null;
        }

        public override bool? CurrentValue(FilterConfiguration configuration)
        {
            return configuration.GetBooleanFilter(Key);
        }

        public string CurrentSelection(FilterConfiguration configuration)
        {
            var currentValue = CurrentValue(configuration);
            if (currentValue == null)
            {
                return "N/A";
            }
            
            if (currentValue == true)
            {
                return "Yes";
            }

            return "No";
        }

        public bool? ConvertSelection(string selection)
        {
            if (selection == "N/A")
            {
                return null;
            }

            if (selection == "Yes")
            {
                return true;
            }

            return false;
        }

        private static readonly string[] Choices = new []{"N/A", "Yes", "No"};

        public override void Draw(FilterConfiguration configuration)
        {
            var currentValue = CurrentSelection(configuration);
            
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
            if (ImGui.BeginCombo("##"+Key+"Combo", currentValue))
            {
                foreach (var item in Choices)
                {
                    if (ImGui.Selectable(item, currentValue == item))
                    {
                        UpdateFilterConfiguration(configuration, ConvertSelection(item));
                    }
                }

                ImGui.EndCombo();
            }
            ImGui.SameLine();
            UiHelpers.HelpMarker(HelpText);
        }

        public override void UpdateFilterConfiguration(FilterConfiguration configuration, bool? newValue)
        {
            if (newValue.HasValue)
            {
                configuration.UpdateBooleanFilter(Key, newValue.Value);
            }
            else
            {
                configuration.RemoveBooleanFilter(Key);
            }
        }
    }
}