using System;
using CriticalCommonLib;
using Dalamud.ContextMenu;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using InventoryTools.Logic;

namespace InventoryTools.Services;

public class ContextMenuService : IDisposable
{
    public const int SatisfactionSupplyItemIdx       = 0x54;
    public const int SatisfactionSupplyItem1Id       = 0x80 + 1 * 0x3C;
    public const int SatisfactionSupplyItem2Id       = 0x80 + 2 * 0x3C;
    public const int ContentsInfoDetailContextItemId = 0x17CC;
    public const int RecipeNoteContextItemId         = 0x398;
    public const int AgentItemContextItemId          = 0x28;
    public const int GatheringNoteContextItemId      = 0xA0;
    public const int ItemSearchContextItemId         = 0x1738;
    public const int ChatLogContextItemId            = 0x948;
    
    public const int SubmarinePartsMenuContextItemId            = 0x54;
    public const int ShopExchangeItemContextItemId            = 0x54;
    public const int ShopContextMenuItemId            = 0x54;
    public const int ShopExchangeCurrencyContextItemId            = 0x54;
    public const int HWDSupplyContextItemId            = 0x38C;
    public const int GrandCompanySupplyListContextItemId            = 0x54;
    public const int GrandCompanyExchangeContextItemId            = 0x54;
    private readonly DalamudContextMenu _contextMenu = new();
    

    public ContextMenuService()
    {
        _contextMenu.OnOpenGameObjectContextMenu += AddGameObjectItem;
        _contextMenu.OnOpenInventoryContextMenu  += AddInventoryItem;
    }

    private void AddInventoryItem(InventoryContextMenuOpenArgs args)
    {
        if (ConfigurationManager.Config.AddMoreInformationContextMenu)
        {
            InventoryContextMenuItem moreInformation =
                new InventoryContextMenuItem(new SeString(new TextPayload("(AT) More Information")), selectedArgs => MoreInformationAction(selectedArgs.ItemId));
            args.AddCustomItem(moreInformation);
        }
    }
    
    private uint GetObjectItemId(uint itemId)
    {
        if (itemId > 500000)
            itemId -= 500000;

        return itemId;
    }

    private unsafe uint? GetObjectItemId(IntPtr agent, int offset)
        => agent != IntPtr.Zero ? GetObjectItemId(*(uint*)(agent + offset)) : null;

    private uint? GetObjectItemId(string name, int offset)
        => GetObjectItemId(PluginService.GuiService.FindAgentInterface(name), offset);

    private unsafe uint? HandleSatisfactionSupply()
    {
        var agent = PluginService.GuiService.FindAgentInterface("SatisfactionSupply");
        if (agent == IntPtr.Zero)
            return null;

        var itemIdx = *(byte*)(agent + SatisfactionSupplyItemIdx);
        return itemIdx switch
        {
            1 => GetObjectItemId(*(uint*)(agent + SatisfactionSupplyItem1Id)),
            2 => GetObjectItemId(*(uint*)(agent + SatisfactionSupplyItem2Id)),
            _ => null,
        };
    }
    private unsafe uint? HandleHWDSupply()
    {
        var agent = PluginService.GuiService.FindAgentInterface("HWDSupply");
        if (agent == IntPtr.Zero)
            return null;

        return GetObjectItemId(*(uint*)(agent + HWDSupplyContextItemId));
    }

    private uint? HandleNulls()
    {
        var itemId = HandleSatisfactionSupply() ?? HandleHWDSupply();
        return itemId;
    }

    private uint? GetGameObjectItemId(GameObjectContextMenuOpenArgs args)
    {
        var item = args.ParentAddonName switch
        {
            null                 => HandleNulls(),
            "Shop" => GetObjectItemId("Shop", ShopContextMenuItemId),
            "GrandCompanySupplyList" => GetObjectItemId("GrandCompanySupplyList", GrandCompanySupplyListContextItemId),
            "GrandCompanyExchange" => GetObjectItemId("GrandCompanyExchange", GrandCompanyExchangeContextItemId),
            "ShopExchangeCurrency" => GetObjectItemId("ShopExchangeCurrency", ShopExchangeCurrencyContextItemId),
            "SubmarinePartsMenu" => GetObjectItemId("SubmarinePartsMenu", SubmarinePartsMenuContextItemId),
            "ShopExchangeItem" => GetObjectItemId("ShopExchangeItem", ShopExchangeItemContextItemId),
            "ContentsInfoDetail" => GetObjectItemId("ContentsInfo",          ContentsInfoDetailContextItemId),
            "RecipeNote"         => GetObjectItemId("RecipeNote",            RecipeNoteContextItemId),
            "RecipeTree"         => GetObjectItemId(AgentById((AgentId)259), AgentItemContextItemId),
            "RecipeMaterialList" => GetObjectItemId(AgentById((AgentId)259), AgentItemContextItemId),
            "GatheringNote"      => GetObjectItemId("GatheringNote",         GatheringNoteContextItemId),
            "ItemSearch"         => GetObjectItemId(args.Agent,              ItemSearchContextItemId),
            "ChatLog"            => GetObjectItemId("ChatLog",               ChatLogContextItemId),
            _                    => null,
        };
        return item;
    }

    private void AddGameObjectItem(GameObjectContextMenuOpenArgs args)
    {
        var gameObjectItemId = GetGameObjectItemId(args);
        if (gameObjectItemId != null)
        {
            if (ConfigurationManager.Config.AddMoreInformationContextMenu)
            {
                GameObjectContextMenuItem moreInformation =
                    new GameObjectContextMenuItem(new SeString(new TextPayload("(AT) More Information")),
                        selectedArgs => MoreInformationAction(gameObjectItemId.Value));
                args.AddCustomItem(moreInformation);
            }
        }
    }
    
    private static unsafe IntPtr AgentById(AgentId id)
    {
        var uiModule = (UIModule*)PluginService.GuiService.GetUIModule();
        var agents   = uiModule->GetAgentModule();
        var agent    = agents->GetAgentByInternalId(id);
        return (IntPtr)agent;
    }
        
    private void MoreInformationAction(uint itemId)
    {
        if (itemId != 0)
        {
            PluginService.WindowService.OpenItemWindow(itemId);
        }
    }
            
    private bool _disposed;
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
        
    protected virtual void Dispose(bool disposing)
    {
        if(!_disposed && disposing)
        {
            _contextMenu.OnOpenGameObjectContextMenu -= AddGameObjectItem;
            _contextMenu.OnOpenInventoryContextMenu  -= AddInventoryItem;
            _contextMenu.Dispose();
        }
        _disposed = true;         
    }
    
    ~ContextMenuService()
    {
#if DEBUG
        // In debug-builds, make sure that a warning is displayed when the Disposable object hasn't been
        // disposed by the programmer.

        if( _disposed == false )
        {
            PluginLog.Error("There is a disposable object which hasn't been disposed before the finalizer call: " + (this.GetType ().Name));
        }
#endif
        Dispose (true);
    }
}