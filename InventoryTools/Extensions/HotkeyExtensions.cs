using Dalamud.Game.ClientState.Keys;
using OtterGui.Classes;

namespace InventoryTools.Extensions
{
    public static class HotkeyExtensions
    {
        public static string FormattedName(this ModifiableHotkey hotkey)
        {
            var fancyName = hotkey.Hotkey.GetFancyName();
            var modifierName = hotkey.Modifier1.Modifier.GetFancyName();
            var modifierName2 = hotkey.Modifier2.Modifier.GetFancyName();
            if (hotkey.Modifier1.Modifier != VirtualKey.NO_KEY && hotkey.Modifier2.Modifier != VirtualKey.NO_KEY)
            {
                return modifierName + " + " + modifierName2 + " + " + fancyName;
            }
            if (hotkey.Modifier1.Modifier != VirtualKey.NO_KEY)
            {
                return modifierName + " + " + fancyName;
            }
            return fancyName;
        }
        public static VirtualKey[] VirtualKeys(this ModifiableHotkey hotkey)
        {
            if (hotkey.Modifier2.Modifier != VirtualKey.NO_KEY)
            {
                return new[] { hotkey.Hotkey, hotkey.Modifier1.Modifier, hotkey.Modifier2.Modifier };
            }
            if (hotkey.Modifier1.Modifier != VirtualKey.NO_KEY)
            {
                return new[] { hotkey.Hotkey, hotkey.Modifier1.Modifier };
            }
            return new[] { hotkey.Hotkey };
        }
    }
}