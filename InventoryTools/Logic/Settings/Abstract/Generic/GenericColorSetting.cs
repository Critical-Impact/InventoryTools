using System.Numerics;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Settings.Abstract.Generic;

public abstract class GenericColorSetting : Setting<Vector4?>
{
    public GenericColorSetting(string key, string name, string helpText, Vector4? defaultValue, SettingCategory settingCategory, SettingSubCategory settingSubCategory, string version, ILogger logger, ImGuiService imGuiService) : base(logger, imGuiService)
    {
        Key = key;
        Name = name;
        HelpText = helpText;
        DefaultValue = defaultValue;
        SettingCategory = settingCategory;
        SettingSubCategory = settingSubCategory;
        Version = version;
    }

    public sealed override Vector4? DefaultValue { get; set; }
    public override Vector4? CurrentValue(InventoryToolsConfiguration configuration)
    {
        return configuration.Get(Key, DefaultValue) ?? DefaultValue;
    }

    public override void Draw(InventoryToolsConfiguration configuration, string? customName, bool? disableReset, bool? disableColouring)
    {
        var value = CurrentValue(configuration);

        ImGui.LabelText("##" + Key + "Label", customName ?? Name);

        var enabled = value != null;

        if (ImGui.Checkbox("Enable##"+Key+"Boolean", ref enabled))
        {
            if (value == null)
            {
                value = DefaultValue ?? new Vector4(1, 1, 1, 1);
            }
            else
            {
                value = null;
            }

            UpdateFilterConfiguration(configuration, value);
        }
        using (var disabled = ImRaii.Disabled(value == null))
        {
            var color = value ?? new Vector4(1, 1, 1, 1);
            ImGui.SameLine();
            if (ImGui.ColorEdit4("##" + Key + "Color", ref color,
                    ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.NoLabel))
            {
                UpdateFilterConfiguration(configuration, color);
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

    public override void UpdateFilterConfiguration(InventoryToolsConfiguration configuration, Vector4? newValue)
    {
        configuration.Set(Key, newValue);
    }

    public sealed override string Key { get; set; }
    public sealed override string Name { get; set; }
    public sealed override string HelpText { get; set; }
    public sealed override SettingCategory SettingCategory { get; set; }
    public override SettingSubCategory SettingSubCategory { get; }
    public override string Version { get; }
}