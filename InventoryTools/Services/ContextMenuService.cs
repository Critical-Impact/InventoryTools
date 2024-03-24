using System;
using CriticalCommonLib;
using CriticalCommonLib.Services.Mediator;
using Dalamud.ContextMenu;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using InventoryTools.Logic;
using InventoryTools.Mediator;
using InventoryTools.Services.Interfaces;
using InventoryTools.Ui;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Services;

public class ContextMenuService : DisposableMediatorSubscriberBase
{
    private readonly IGuiService _guiService;
    private readonly InventoryToolsConfiguration _configuration;
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
    private readonly DalamudContextMenu _contextMenu;
    

    public ContextMenuService(ILogger<ContextMenuService> logger, MediatorService mediatorService, DalamudPluginInterface dalamudPluginInterface, IGuiService guiService, InventoryToolsConfiguration configuration) : base(logger, mediatorService)
    {
        _guiService = guiService;
        _configuration = configuration;
        _contextMenu = new DalamudContextMenu(dalamudPluginInterface);
        _contextMenu.OnOpenGameObjectContextMenu += AddGameObjectItem;
        _contextMenu.OnOpenInventoryContextMenu  += AddInventoryItem;
    }

    private void AddInventoryItem(InventoryContextMenuOpenArgs args)
    {
        if (_configuration.AddMoreInformationContextMenu)
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
        => GetObjectItemId(_guiService.FindAgentInterface(name), offset);

    private unsafe uint? HandleSatisfactionSupply()
    {
        var agent = _guiService.FindAgentInterface("SatisfactionSupply");
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
        var agent = _guiService.FindAgentInterface("HWDSupply");
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
        if (item == null)
        {
            var guiHoveredItem = Service.GameGui.HoveredItem;
            if (guiHoveredItem >= 2000000 || guiHoveredItem == 0) return null;
            item = (uint)guiHoveredItem % 500_000;
        }

        return item;
    }

    private void AddGameObjectItem(GameObjectContextMenuOpenArgs args)
    {
        var gameObjectItemId = GetGameObjectItemId(args);
        if (gameObjectItemId != null)
        {
            if (_configuration.AddMoreInformationContextMenu)
            {
                GameObjectContextMenuItem moreInformation =
                    new GameObjectContextMenuItem(new SeString(new TextPayload("(AT) More Information")),
                        selectedArgs => MoreInformationAction(gameObjectItemId.Value));
                args.AddCustomItem(moreInformation);
            }
        }
    }
    
    private unsafe IntPtr AgentById(AgentId id)
    {
        var uiModule = (UIModule*)_guiService.GetUIModule();
        var agents   = uiModule->GetAgentModule();
        var agent    = agents->GetAgentByInternalId(id);
        return (IntPtr)agent;
    }
        
    private void MoreInformationAction(uint itemId)
    {
        if (itemId >= 2000000 || itemId == 0) return ;
        itemId %= 500000;
        MediatorService.Publish(new OpenUintWindowMessage(typeof(ItemWindow), itemId));
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
            Logger.LogError("There is a disposable object which hasn't been disposed before the finalizer call: " + (this.GetType ().Name));
        }
#endif
        Dispose (true);
    }
}