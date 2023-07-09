using CriticalCommonLib;
using CriticalCommonLib.Services;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic;

namespace InventoryTools.Tooltips;

public abstract class BaseTooltip : TooltipService.TooltipTweak
{
    public bool HoverItemIsHq
    {
        get
        {
            var itemId = Service.Gui.HoveredItem;
            if (itemId < 2000000)
            {
                return  itemId > 1000000;
            }

            return false;
        }
    }
    public uint HoverItemId
    {
        get
        {
            var itemId = Service.Gui.HoveredItem;
            if (itemId < 2000000)
            {
                bool isHq = itemId > 1000000;
                itemId %= 500000;
                return (uint) itemId;
            }

            return 0;
        }
    }

    public ItemEx? HoverItem => Service.ExcelCache.GetItemExSheet().GetRow(HoverItemId);

    public bool ShouldShow()
    {
        if (!ConfigurationManager.Config.DisplayTooltip)
        {
            return false;
        }
        var itemId = Service.Gui.HoveredItem;
        if (itemId < 2000000)
        {
            itemId %= 500000;

            var item = Service.ExcelCache.GetItemExSheet().GetRow((uint) itemId);
            if (item != null)
            {
                if (ConfigurationManager.Config.TooltipWhitelistCategories.Count == 0)
                {
                    return true;
                }
                if (ConfigurationManager.Config.TooltipWhitelistBlacklist)
                {
                    if (ConfigurationManager.Config.TooltipWhitelistCategories.Contains(item.ItemUICategory.Row))
                    {
                        return false;
                    }

                    return true;
                }

                if (ConfigurationManager.Config.TooltipWhitelistCategories.Contains(item.ItemUICategory.Row))
                {
                    return true;
                }
            }
        }

        return false;
    }
}