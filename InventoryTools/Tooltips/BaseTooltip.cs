using CriticalCommonLib.Services;
using CriticalCommonLib.Sheets;
using Dalamud.Plugin.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Tooltips;

public abstract class BaseTooltip : TooltipService.TooltipTweak
{
    public ExcelCache ExcelCache { get; }
    public InventoryToolsConfiguration Configuration { get; }
    public IGameGui GameGui { get; }
    public ILogger Logger { get; }

    public BaseTooltip(ILogger logger, ExcelCache excelCache, InventoryToolsConfiguration configuration, IGameGui gameGui)
    {
        ExcelCache = excelCache;
        Configuration = configuration;
        GameGui = gameGui;
        Logger = logger;
    }
    public bool HoverItemIsHq
    {
        get
        {
            var itemId = GameGui.HoveredItem;
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
            var itemId = GameGui.HoveredItem;
            if (itemId < 2000000)
            {
                bool isHq = itemId > 1000000;
                itemId %= 500000;
                return (uint) itemId;
            }

            return 0;
        }
    }

    public ItemEx? HoverItem => ExcelCache.GetItemExSheet().GetRow(HoverItemId);

    public bool ShouldShow()
    {
        if (!Configuration.DisplayTooltip)
        {
            return false;
        }
        var itemId = GameGui.HoveredItem;
        if (itemId < 2000000)
        {
            itemId %= 500000;

            var item = ExcelCache.GetItemExSheet().GetRow((uint) itemId);
            if (item != null)
            {
                if (Configuration.TooltipWhitelistCategories.Count == 0)
                {
                    return true;
                }
                if (Configuration.TooltipWhitelistBlacklist)
                {
                    if (Configuration.TooltipWhitelistCategories.Contains(item.ItemUICategory.Row))
                    {
                        return false;
                    }

                    return true;
                }

                if (Configuration.TooltipWhitelistCategories.Contains(item.ItemUICategory.Row))
                {
                    return true;
                }
            }
        }

        return false;
    }
}