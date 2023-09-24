using CriticalCommonLib;
using InventoryTools.Logic;
using OtterGui.Classes;

namespace InventoryTools.Hotkeys;

public class MoreInfoWindowHotkey : Hotkey
{
    public override ModifiableHotkey? ModifiableHotkey => ConfigurationManager.Config.MoreInformationHotKey;

    public override bool OnHotKey()
    {
        var id = Service.GameGui.HoveredItem;
        if (id >= 2000000 || id == 0) return false;
        id %= 500000;
        var item = Service.ExcelCache.GetItemExSheet().GetRow((uint) id);
        if (item == null) return false;
        PluginService.WindowService.OpenItemWindow(item.RowId);
        return true;
    }
}