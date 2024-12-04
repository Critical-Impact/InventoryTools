using System.Collections.Generic;
using Dalamud.Game.ClientState.Keys;
using ImGuiNET;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;
using OtterGui.Classes;
using OtterGui.Widgets;

namespace InventoryTools.Logic.Settings.Abstract
{
    public abstract class HotKeySetting : Setting<ModifiableHotkey>
    {
        public HotKeySetting(ILogger logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }
        private readonly List<VirtualKey> _virtualKeys = new() { VirtualKey.A, VirtualKey.B, VirtualKey.C, VirtualKey.D, VirtualKey.E, VirtualKey.F, VirtualKey.G, VirtualKey.H, VirtualKey.I, VirtualKey.J, VirtualKey.K, VirtualKey.L, VirtualKey.M, VirtualKey.N, VirtualKey.O, VirtualKey.P, VirtualKey.Q, VirtualKey.R, VirtualKey.S, VirtualKey.T, VirtualKey.U, VirtualKey.V, VirtualKey.W, VirtualKey.X, VirtualKey.Y, VirtualKey.Z, VirtualKey.NO_KEY };


        public override ModifiableHotkey CurrentValue(InventoryToolsConfiguration configuration)
        {
            return configuration.Hotkeys.ContainsKey(Key) ? configuration.Hotkeys[Key] : new ModifiableHotkey();
        }

        public override void UpdateFilterConfiguration(InventoryToolsConfiguration configuration, ModifiableHotkey newValue)
        {
            if (newValue.Hotkey == VirtualKey.NO_KEY && newValue.Modifier1 == VirtualKey.NO_KEY &&
                newValue.Modifier2 == VirtualKey.NO_KEY)
            {
                configuration.Hotkeys.Remove(Key, out _);
            }
            else
            {
                configuration.Hotkeys[Key] = newValue;
            }
        }
        public override void Draw(InventoryToolsConfiguration configuration, string? customName, bool? disableReset,
            bool? disableColouring)
        {
            Widget.ModifiableKeySelector(customName ?? Name, HelpText, InputSize, CurrentValue(configuration),
                delegate(ModifiableHotkey hotkey)
                {
                    UpdateFilterConfiguration(configuration, hotkey);
                }, _virtualKeys);

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

    }
}