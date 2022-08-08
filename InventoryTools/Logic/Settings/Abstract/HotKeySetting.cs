using System.Collections.Generic;
using Dalamud.Game.ClientState.Keys;
using OtterGui.Classes;
using OtterGui.Widgets;

namespace InventoryTools.Logic.Settings.Abstract
{
    public abstract class HotKeySetting : Setting<ModifiableHotkey>
    {
        private readonly List<VirtualKey> _virtualKeys = new() { VirtualKey.A, VirtualKey.B, VirtualKey.C, VirtualKey.D, VirtualKey.E, VirtualKey.F, VirtualKey.G, VirtualKey.H, VirtualKey.I, VirtualKey.J, VirtualKey.K, VirtualKey.L, VirtualKey.M, VirtualKey.N, VirtualKey.O, VirtualKey.P, VirtualKey.Q, VirtualKey.R, VirtualKey.S, VirtualKey.T, VirtualKey.U, VirtualKey.V, VirtualKey.W, VirtualKey.X, VirtualKey.Y, VirtualKey.Z, VirtualKey.NO_KEY };


        public override void Draw(InventoryToolsConfiguration configuration)
        {
            
            Widget.ModifiableKeySelector(Name, HelpText, LabelSize, CurrentValue(configuration),
                delegate(ModifiableHotkey hotkey)
                {
                    UpdateFilterConfiguration(configuration, hotkey);
                }, _virtualKeys);
        }
        
    }
}