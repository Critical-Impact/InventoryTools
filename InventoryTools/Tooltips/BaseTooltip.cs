using AllaganLib.GameSheets.Sheets;
using AllaganLib.GameSheets.Sheets.Rows;
using CriticalCommonLib.Services;

using Dalamud.Plugin.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Tooltips;

public abstract class BaseTooltip : TooltipService.TooltipTweak
{
    public InventoryToolsConfiguration Configuration { get; }
    public IGameGui GameGui { get; }
    public ILogger Logger { get; }
    public ItemSheet ItemSheet { get; }
    public abstract uint Order { get; }

    public BaseTooltip(ILogger logger, ItemSheet itemSheet, InventoryToolsConfiguration configuration, IGameGui gameGui)
    {
        Configuration = configuration;
        GameGui = gameGui;
        Logger = logger;
        ItemSheet = itemSheet;
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

    public ItemRow? HoverItem => ItemSheet.GetRowOrDefault(HoverItemId);

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

            var item = ItemSheet.GetRowOrDefault((uint) itemId);
            if (item != null)
            {
                if (Configuration.TooltipWhitelistCategories.Count == 0)
                {
                    return true;
                }
                if (Configuration.TooltipWhitelistBlacklist)
                {
                    if (Configuration.TooltipWhitelistCategories.Contains(item.Base.ItemUICategory.RowId))
                    {
                        return false;
                    }

                    return true;
                }

                if (Configuration.TooltipWhitelistCategories.Contains(item.Base.ItemUICategory.RowId))
                {
                    return true;
                }
            }
        }

        return false;
    }
}