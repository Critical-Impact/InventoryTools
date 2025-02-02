using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Interface.Colors;
using ImGuiNET;
using InventoryTools.Logic.ItemRenderers;
using InventoryTools.Logic.Settings.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;
using OtterGui.Raii;

namespace InventoryTools.Logic.Settings;

public class UseIconGroupingSetting : Setting<Dictionary<Type, bool>?>
{
    private readonly Dictionary<Type, IItemInfoRenderer> _sourceRenderers;
    private readonly Dictionary<SourceIconGrouping,string> _choices;

    public UseIconGroupingSetting(ILogger<UseIconGroupingSetting> logger, IEnumerable<IItemInfoRenderer> itemInfoRenderers, ImGuiService imGuiService) : base(logger, imGuiService)
    {
        _sourceRenderers = itemInfoRenderers.Where(c => c.RendererType == RendererType.Use).ToDictionary(c => c.ItemSourceType, c => c);
        _choices = new Dictionary<SourceIconGrouping, string>()
        {
            {SourceIconGrouping.Default, "Default"},
            {SourceIconGrouping.Grouped, "Grouped"},
            {SourceIconGrouping.Ungrouped, "Ungrouped"}
        };
    }

    public override Dictionary<Type, bool>? DefaultValue { get; set; } = null;
    public override Dictionary<Type, bool>? CurrentValue(InventoryToolsConfiguration configuration)
    {
        return configuration.Get(Key, DefaultValue);
    }

    public override void Draw(InventoryToolsConfiguration configuration, string? customName, bool? disableReset, bool? disableColouring)
    {
        var currentSettings = CurrentValue(configuration) ?? new();

        foreach (var sourceRenderer in _sourceRenderers)
        {
            ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 5);
            using (var id = ImRaii.PushId(sourceRenderer.Key.Name))
            {
                SourceIconGrouping sourceIconGrouping = SourceIconGrouping.Default;
                bool hasValueSet = false;
                if (currentSettings.TryGetValue(sourceRenderer.Key, out var currentValue))
                {
                    sourceIconGrouping = currentValue ? SourceIconGrouping.Grouped : SourceIconGrouping.Ungrouped;
                    hasValueSet = true;
                }

                if (disableColouring != true && hasValueSet)
                {
                    ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.HealerGreen);
                    ImGui.LabelText("##Label", sourceRenderer.Value.SingularName);
                    ImGui.PopStyleColor();
                }
                else
                {
                    ImGui.LabelText("##Label", sourceRenderer.Value.SingularName);
                }



                var defaultGrouping = sourceRenderer.Value.ShouldGroup;

                ImGui.SetNextItemWidth(InputSize);
                var previewValue = sourceIconGrouping == SourceIconGrouping.Default ? $"{_choices[sourceIconGrouping]} ({_choices[defaultGrouping ? SourceIconGrouping.Grouped : SourceIconGrouping.Ungrouped]})" : _choices[sourceIconGrouping];

                using (var combo = ImRaii.Combo("##Combo",  previewValue))
                {
                    if (combo.Success)
                    {
                        foreach (var item in _choices)
                        {
                            var text = item.Value.Replace("\u0002\u001F\u0001\u0003", "-");
                            if (text == "")
                            {
                                continue;
                            }

                            if (ImGui.Selectable(text, _choices[sourceIconGrouping] == text))
                            {
                                if (item.Key == SourceIconGrouping.Grouped)
                                {
                                    currentSettings[sourceRenderer.Key] = true;
                                }
                                else if (item.Key == SourceIconGrouping.Ungrouped)
                                {
                                    currentSettings[sourceRenderer.Key] = false;
                                }
                                else
                                {
                                    currentSettings.Remove(sourceRenderer.Key);
                                }

                                UpdateFilterConfiguration(configuration, currentSettings);
                            }
                        }
                    }
                }

                ImGui.SameLine();
                ImGuiService.HelpMarker(HelpText, Image, ImageSize);
                if (disableReset != true && hasValueSet)
                {
                    ImGui.SameLine();
                    if (ImGui.Button("Reset##Reset"))
                    {
                        currentSettings.Remove(sourceRenderer.Key);
                        UpdateFilterConfiguration(configuration, currentSettings);
                    }
                }
            }
            ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 5);
        }
    }

    public override void UpdateFilterConfiguration(InventoryToolsConfiguration configuration, Dictionary<Type, bool>? newValue)
    {
        configuration.Set(Key, newValue);
    }

    public override string Key { get; set; } = "UseIconGrouping";
    public override string Name { get; set; } = "Use Acquisition Icon Grouping";
    public override string HelpText { get; set; } = "When use acquisition icons are displayed, how should they be grouped?";
    public override SettingCategory SettingCategory { get; set; } = SettingCategory.Items;
    public override SettingSubCategory SettingSubCategory { get; } = SettingSubCategory.UseGrouping;
    public override string Version { get; } = "1.11.0.10";
}