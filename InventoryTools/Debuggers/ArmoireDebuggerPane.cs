using System;
using AllaganLib.Shared.Interfaces;
using Dalamud.Bindings.ImGui;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI;

namespace InventoryTools.Debuggers;

public class ArmoireDebuggerPane : IDebugPane
{
    private readonly IGameGui _gameGui;

    public ArmoireDebuggerPane(IGameGui gameGui)
    {
        _gameGui = gameGui;
    }

    public string Name => "Armoire";
    public unsafe void Draw()
    {
        var uiState = UIState.Instance();
        if (uiState == null)
        {
            ImGui.Text("UIState not found.");
        }
        else
        {
            ImGui.Text(uiState->Cabinet.IsCabinetLoaded() ? "Cabinet Loaded" : "Cabinet Not Loaded");
        }

        var addon = this._gameGui.GetAddonByName("CabinetWithdraw");
        if (addon != IntPtr.Zero)
        {
            var cabinetWithdraw = (AddonCabinetWithdraw*)addon.Address;
            if (cabinetWithdraw != null)
            {
                ImGui.Text($"Artifact Armor Selected: { (cabinetWithdraw->ArtifactArmorRadioButton->IsChecked ? "yes" : "no") }");
                ImGui.Text($"Seasonal Gear 1 Selected: { (cabinetWithdraw->SeasonalGear1RadioButton->IsChecked ? "yes" : "no") }");
                ImGui.Text($"Seasonal Gear 2 Selected: { (cabinetWithdraw->SeasonalGear2RadioButton->IsChecked ? "yes" : "no") }");
                ImGui.Text($"Seasonal Gear 3 Selected: { (cabinetWithdraw->SeasonalGear3RadioButton->IsChecked ? "yes" : "no") }");
                ImGui.Text($"Seasonal Gear 4 Selected: { (cabinetWithdraw->SeasonalGear4RadioButton->IsChecked ? "yes" : "no") }");
                ImGui.Text($"Seasonal Gear 5 Selected: { (cabinetWithdraw->SeasonalGear5RadioButton->IsChecked ? "yes" : "no") }");
                ImGui.Text($"Achievements Selected: { (cabinetWithdraw->AchievementsRadioButton->IsChecked ? "yes" : "no") }");
                ImGui.Text($"Exclusive Extras Selected: { (cabinetWithdraw->ExclusiveExtrasRadioButton->IsChecked ? "yes" : "no") }");
                ImGui.Text($"Search Selected: { (cabinetWithdraw->SearchRadioButton->IsChecked ? "yes" : "no") }");
            }
        }
    }
}