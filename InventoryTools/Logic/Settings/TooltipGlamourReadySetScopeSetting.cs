using System.Collections.Generic;
using CriticalCommonLib.Models;
using Dalamud.Interface.Colors;
using Dalamud.Bindings.ImGui;
using InventoryTools.Logic.Editors;
using InventoryTools.Logic.Settings.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Settings;

public class TooltipGlamourReadySetScopeSetting : Setting<List<InventorySearchScope>?>
{
    private readonly InventoryScopePicker _scopePicker;

    public TooltipGlamourReadySetScopeSetting(ILogger<TooltipGlamourReadySetScopeSetting> logger, ImGuiService imGuiService, InventoryScopePicker scopePicker) : base(logger, imGuiService)
    {
        _scopePicker = scopePicker;
    }

    public override List<InventorySearchScope>? DefaultValue { get; set; } = new()
    {
        new InventorySearchScope()
        {
            ActiveCharacter = true,
            Categories = [InventoryCategory.GlamourChest, InventoryCategory.Armoire]
        }
    };

    public override List<InventorySearchScope>? CurrentValue(InventoryToolsConfiguration configuration)
    {
        if (configuration.TooltipGlamourReadySetScope == null || configuration.TooltipGlamourReadySetScope.Count == 0)
        {
            return DefaultValue;
        }
        return configuration.TooltipGlamourReadySetScope;
    }

    public override void Draw(InventoryToolsConfiguration configuration, string? customName, bool? disableReset, bool? disableColouring)
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
        if (_scopePicker.Draw("##glamourReadySetScope", currentScopes))
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
        configuration.TooltipGlamourReadySetScope = newValue;
    }

    public override string Key { get; set; } = "TooltipGlamourReadySetScope";
    public override string Name { get; set; } = "Outfit Glamour (Search Locations)";
    public override string HelpText { get; set; } = "Which inventories should be searched when determining which outfit glamour set items you already own?";
    
    public override string Version => "15.0.5";
}