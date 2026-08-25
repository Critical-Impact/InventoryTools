using System.Collections.Generic;
using CriticalCommonLib.Models;
using Dalamud.Interface.Colors;
using Dalamud.Bindings.ImGui;
using InventoryTools.Logic.Editors;
using InventoryTools.Logic.Settings.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Settings;

public class TooltipCofferLootScopeSetting : Setting<List<InventorySearchScope>?>
{
    private readonly InventoryScopePicker _scopePicker;

    public TooltipCofferLootScopeSetting(ILogger<TooltipCofferLootScopeSetting> logger, ImGuiService imGuiService, InventoryScopePicker scopePicker) : base(logger, imGuiService)
    {
        _scopePicker = scopePicker;
    }

    public override List<InventorySearchScope>? DefaultValue { get; set; } = new ()
    {
        new InventorySearchScope()
        {
            ActiveCharacter = true,
            Categories = [InventoryCategory.CharacterBags, InventoryCategory.CharacterEquipped, InventoryCategory.RetainerBags, InventoryCategory.RetainerEquipped, InventoryCategory.GlamourChest, InventoryCategory.Armoire, InventoryCategory.CharacterSaddleBags, InventoryCategory.CharacterPremiumSaddleBags]
        }
    };

    public override List<InventorySearchScope>? CurrentValue(InventoryToolsConfiguration configuration)
    {
        if (configuration.TooltipCofferLootScope == null || configuration.TooltipCofferLootScope.Count == 0)
        {
            return DefaultValue;
        }
        return configuration.TooltipCofferLootScope;
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
        if (_scopePicker.Draw("##cofferLootScope", currentScopes))
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
        configuration.TooltipCofferLootScope = newValue;
    }

    public override string Key { get; set; } = "TooltipCofferLootScope";
    public override string Name { get; set; } = "Coffer Loot (Search Locations)";
    public override string HelpText { get; set; } = "Which inventories should be searched when determining how many coffer loot items you already own?";
    
    public override string Version => "1.12.0.0";
}
