using System.Collections.Generic;
using Dalamud.Interface.Colors;
using ImGuiNET;
using InventoryTools.Logic.Editors;
using InventoryTools.Logic.Settings.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Settings;



public class ContextMenuItemSearchScopeSetting : Setting<List<InventorySearchScope>?>
{
    private readonly InventoryScopePicker _scopePicker;

    public ContextMenuItemSearchScopeSetting(ILogger<ContextMenuItemSearchScopeSetting> logger, ImGuiService imGuiService, InventoryScopePicker scopePicker) : base(logger, imGuiService)
    {
        _scopePicker = scopePicker;
    }
    public override List<InventorySearchScope>? DefaultValue { get; set; } = null;
    public override List<InventorySearchScope>? CurrentValue(InventoryToolsConfiguration configuration)
    {
        if (configuration.ItemSearchScope == null || configuration.ItemSearchScope.Count == 0)
        {
            return DefaultValue;
        }
        return configuration.ItemSearchScope;
    }

    public override void Draw(InventoryToolsConfiguration configuration, string? customName, bool? disableReset,
        bool? disableColouring)
    {
        var currentScopes = CurrentValue(configuration) ?? new List<InventorySearchScope>();
        if (disableColouring != true && HasValueSet(configuration))
        {
            ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.HealerGreen);
            ImGui.LabelText("##" + Key + "Label", Name);
            ImGui.PopStyleColor();
        }
        else
        {
            ImGui.LabelText("##" + Key + "Label", Name);
        }

        ImGui.SetNextItemWidth(InputSize - 26);
        if (_scopePicker.Draw("##tooltipScope", currentScopes))
        {
            UpdateFilterConfiguration(configuration, currentScopes);
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

    public override void UpdateFilterConfiguration(InventoryToolsConfiguration configuration, List<InventorySearchScope>? newValue)
    {
        configuration.ItemSearchScope = newValue;
    }

    public override string Key { get; set; } = "ItemSearchScope";
    public override string Name { get; set; } = "Context Menu - Search Scope";
    public override string HelpText { get; set; } = "When searching for an item across the inventories AT knows about, which inventories should be searched?";

    public override string WizardName { get; } = "Search Scope";
    public override SettingCategory SettingCategory { get; set; } = SettingCategory.ContextMenu;
    public override SettingSubCategory SettingSubCategory { get; } = SettingSubCategory.General;
    public override string Version => "1.7.0.13";
}