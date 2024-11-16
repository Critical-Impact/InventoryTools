using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CriticalCommonLib.Services.Mediator;
using Dalamud.Game.Gui.ContextMenu;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using InventoryTools.Logic;
using InventoryTools.Mediator;
using InventoryTools.Services.Interfaces;
using InventoryTools.Ui;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Services;

public class ContextMenuService : DisposableMediatorSubscriberBase, IHostedService
{
    public IContextMenu ContextMenu { get; }
    private readonly IListService _listService;
    private readonly IGameGui _gameGui;
    private readonly InventoryToolsConfiguration _configuration;
    public const int SatisfactionSupplyItemIdx       = 84;
    public const int SatisfactionSupplyItem1Id       = 128 + 1 * 60;
    public const int SatisfactionSupplyItem2Id       = 128 + 2 * 60;
    public const int ContentsInfoDetailContextItemId = 6092;
    public const int RecipeNoteContextItemId         = 920;
    public const int AgentItemContextItemId          = 40;
    public const int GatheringNoteContextItemId      = 160;
    public const int ItemSearchContextItemId         = 6096;
    public const int ChatLogContextMenuType          = ChatLogContextItemId + 8;
    public const int ChatLogContextItemId            = 2392;

    public const int SubmarinePartsMenuContextItemId            = 84;
    public const int ShopExchangeItemContextItemId            = 84;
    public const int ShopContextMenuItemId            = 84;
    public const int ShopExchangeCurrencyContextItemId            = 84;
    public const int HWDSupplyContextItemId            = 908;
    public const int GrandCompanySupplyListContextItemId            = 84;
    public const int GrandCompanyExchangeContextItemId            = 84;

    public ContextMenuService(ILogger<ContextMenuService> logger, IListService listService, IContextMenu contextMenu, IGameGui gameGui, MediatorService mediatorService, InventoryToolsConfiguration configuration) : base(logger, mediatorService)
    {
        ContextMenu = contextMenu;
        _listService = listService;
        _gameGui = gameGui;
        _configuration = configuration;
    }

    private void MenuOpened(IMenuOpenedArgs args)
    {
        uint? itemId;
        Logger.LogDebug($"{args.AddonName}");
        Logger.LogDebug($"{(ulong)args.AgentPtr:X}");
        itemId = GetGameObjectItemId(args);
        itemId %= 500000;
        Logger.LogDebug($"{itemId}");

        if (itemId != null)
        {
            if (_configuration.AddMoreInformationContextMenu)
            {
                var menuItem = new MenuItem();
                menuItem.Name = "More Information";
                menuItem.PrefixChar = 'A';
                menuItem.OnClicked += clickedArgs => MoreInformationClicked(clickedArgs, itemId);
                args.AddMenuItem(menuItem);
            }

            if (_configuration.ItemSearchContextMenu)
            {
                var menuItem = new MenuItem();
                menuItem.Name = "Search";
                menuItem.PrefixChar = 'A';
                menuItem.OnClicked += clickedArgs => ItemSearchClicked(clickedArgs, itemId);
                args.AddMenuItem(menuItem);
            }

            if (_configuration.AddToActiveCraftListContextMenu)
            {
                var activeList = _listService.GetActiveCraftList();
                if (activeList != null)
                {
                    var menuItem = new MenuItem();
                    menuItem.Name = "Add to Active Craft List";
                    menuItem.PrefixChar = 'A';
                    menuItem.OnClicked += clickedArgs => AddToCraftList(activeList, clickedArgs, itemId);
                    args.AddMenuItem(menuItem);
                }
            }

            if (_configuration.AddToCraftListContextMenu)
            {
                var menuItem = new MenuItem();
                menuItem.Name = "Add to Craft List";
                menuItem.PrefixChar = 'A';
                menuItem.IsSubmenu = true;
                menuItem.OnClicked += clickedArgs => OpenAddCraftListSubmenu(clickedArgs, itemId);
                args.AddMenuItem(menuItem);
            }


        }
    }

    private uint? GetGameObjectItemId(IMenuOpenedArgs args)
    {
        var item = args.AddonName switch
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
            "RecipeTree"         => GetObjectItemId(AgentById(AgentId.RecipeItemContext), AgentItemContextItemId),
            "RecipeMaterialList" => GetObjectItemId(AgentById(AgentId.RecipeItemContext), AgentItemContextItemId),
            "RecipeProductList" => GetObjectItemId(AgentById(AgentId.RecipeItemContext), AgentItemContextItemId),
            "GatheringNote"      => GetObjectItemId("GatheringNote",         GatheringNoteContextItemId),
            "ItemSearch"         => GetObjectItemId(args.AgentPtr,              ItemSearchContextItemId),
            "ChatLog"            => GetObjectItemId("ChatLog",               ChatLogContextItemId),
            _                    => null,
        };

        if (args.AddonName == "ChatLog" &&
            (item >= 1500000 || GetObjectItemId("ChatLog", ChatLogContextMenuType) != 3)) {
            return null;
        }

        if (item == null)
        {
            var guiHoveredItem = _gameGui.HoveredItem;
            if (guiHoveredItem >= 2000000 || guiHoveredItem == 0) return null;
            item = (uint)guiHoveredItem % 500_000;
        }

        return item;
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
        => GetObjectItemId(_gameGui.FindAgentInterface(name), offset);

    private unsafe uint? HandleSatisfactionSupply()
    {
        var agent = _gameGui.FindAgentInterface("SatisfactionSupply");
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
        var agent = _gameGui.FindAgentInterface("HWDSupply");
        if (agent == IntPtr.Zero)
            return null;

        return GetObjectItemId(*(uint*)(agent + HWDSupplyContextItemId));
    }

    private uint? HandleNulls()
    {
        var itemId = HandleSatisfactionSupply() ?? HandleHWDSupply();
        return itemId;
    }

    private void OpenAddCraftListSubmenu(IMenuItemClickedArgs obj, uint? itemId = null)
    {
        var craftLists = _listService.Lists.Where(c => !c.CraftListDefault && c.FilterType == FilterType.CraftFilter).ToList();
        var menuItems = new List<MenuItem>();
        foreach (var craftList in craftLists)
        {
            var menuItem = new MenuItem();
            if (craftList.IsEphemeralCraftList)
            {
                menuItem.PrefixChar = 'E';
            }
            menuItem.Name = craftList.Name;
            menuItem.OnClicked += args =>
            {
                AddToCraftList(craftList, args, itemId);
            };
            menuItems.Add(menuItem);
        }

        var newButton = new MenuItem();
        newButton.Name = "Add to New Craft List";
        newButton.OnClicked += args => AddToNewCraftList(args, itemId);
        menuItems.Add(newButton);

        newButton = new MenuItem();
        newButton.Name = "Add to New Ephemeral Craft List";
        newButton.OnClicked += args => AddToNewEphemeralCraftList(args, itemId);
        menuItems.Add(newButton);
        obj.OpenSubmenu(menuItems);
    }

    private unsafe IntPtr AgentById(AgentId id)
    {
        var uiModule = (UIModule*)_gameGui.GetUIModule();
        var agents   = uiModule->GetAgentModule();
        var agent    = agents->GetAgentByInternalId(id);
        return (IntPtr)agent;
    }

    private void AddToNewCraftList(IMenuItemClickedArgs obj, uint? itemId = null)
    {
        if (obj.Target is MenuTargetInventory inventory)
        {
            if (inventory.TargetItem != null)
            {
                itemId ??= inventory.TargetItem.Value.ItemId;
                MediatorService.Publish(new AddToNewCraftListMessage(itemId.Value, 1, inventory.TargetItem.Value.IsHq ? InventoryItem.ItemFlags.HighQuality : InventoryItem.ItemFlags.None, false));
            }
        }
        else if(itemId != null)
        {
            MediatorService.Publish(new AddToNewCraftListMessage(itemId.Value, 1, InventoryItem.ItemFlags.None, false));
        }
    }

    private void AddToNewEphemeralCraftList(IMenuItemClickedArgs obj, uint? itemId = null)
    {
        if (obj.Target is MenuTargetInventory inventory)
        {
            if (inventory.TargetItem != null)
            {
                itemId ??= inventory.TargetItem.Value.ItemId;
                MediatorService.Publish(new AddToNewCraftListMessage(itemId.Value, 1, inventory.TargetItem.Value.IsHq ? InventoryItem.ItemFlags.HighQuality : InventoryItem.ItemFlags.None, true));
            }
        }
        else if(itemId != null)
        {
            MediatorService.Publish(new AddToNewCraftListMessage(itemId.Value, 1, InventoryItem.ItemFlags.None, true));
        }
    }

    private void AddToCraftList(FilterConfiguration craftList, IMenuItemClickedArgs obj, uint? itemId = null)
    {
        if (obj.Target is MenuTargetInventory inventory)
        {
            if (inventory.TargetItem != null)
            {
                itemId ??= inventory.TargetItem.Value.ItemId;
                MediatorService.Publish(new AddToCraftListMessage(craftList.Key, itemId.Value, 1, inventory.TargetItem.Value.IsHq ? InventoryItem.ItemFlags.HighQuality : InventoryItem.ItemFlags.None));
            }
        }
        else if(itemId != null)
        {
            MediatorService.Publish(new AddToCraftListMessage(craftList.Key, itemId.Value, 1, InventoryItem.ItemFlags.None));
        }
    }

    private void MoreInformationClicked(IMenuItemClickedArgs obj, uint? itemId = null)
    {
        if (obj.Target is MenuTargetInventory inventory)
        {
            if (inventory.TargetItem != null)
            {
                itemId ??= inventory.TargetItem.Value.ItemId;
            }
        }

        if (itemId != null)
        {
            MediatorService.Publish(new OpenUintWindowMessage(typeof(ItemWindow), itemId.Value));
        }
    }

    private void ItemSearchClicked(IMenuItemClickedArgs obj, uint? itemId = null)
    {
        if (obj.Target is MenuTargetInventory inventory)
        {
            if (inventory.TargetItem != null)
            {
                itemId ??= inventory.TargetItem.Value.ItemId;
                MediatorService.Publish(new ItemSearchRequestedMessage(itemId.Value, inventory.TargetItem.Value.IsHq ? InventoryItem.ItemFlags.HighQuality : InventoryItem.ItemFlags.None));
            }
        }
        else if(itemId != null)
        {
            MediatorService.Publish(new ItemSearchRequestedMessage(itemId.Value, InventoryItem.ItemFlags.None));
        }
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        Logger.LogTrace("Started service {type} ({this})", GetType().Name, this);
        ContextMenu.OnMenuOpened += MenuOpened;
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        Logger.LogTrace("Stopped service {type} ({this})", GetType().Name, this);
        ContextMenu.OnMenuOpened -= MenuOpened;
        return Task.CompletedTask;
    }
}