using System;
using AllaganLib.GameSheets.Sheets;
using AllaganLib.GameSheets.Sheets.Rows;
using CriticalCommonLib.Services;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Tooltips;

public abstract class BaseTooltip : TooltipService.TooltipTweak, IDisposable
{
    public InventoryToolsConfiguration Configuration { get; }
    public IGameGui GameGui { get; }

    public IDalamudPluginInterface PluginInterface { get; }
    public ILogger Logger { get; }
    public ItemSheet ItemSheet { get; }
    public abstract uint Order { get; }

    public BaseTooltip(uint tooltipIdentifier, ILogger logger, ItemSheet itemSheet, InventoryToolsConfiguration configuration, IGameGui gameGui, IDalamudPluginInterface pluginInterface)
    {
        TooltipIdentifier = tooltipIdentifier;
        Configuration = configuration;
        GameGui = gameGui;
        PluginInterface = pluginInterface;
        Logger = logger;
        ItemSheet = itemSheet;
    }

    public DalamudLinkPayload GetLinkPayload()
    {
        return this.IdentifierPayload ??= PluginInterface.AddChatLinkHandler(TooltipIdentifier, (_, _) => { });
    }

    public void ClearLinkPayload()
    {
        if (this.IdentifierPayload != null)
        {
            PluginInterface.RemoveChatLinkHandler(TooltipIdentifier);
        }
    }

    public virtual uint TooltipIdentifier { get; set; }

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

    public void Dispose()
    {
        ClearLinkPayload();
    }
}