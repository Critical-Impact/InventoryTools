using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CriticalCommonLib.Services;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game;
using Microsoft.Extensions.Hosting;

namespace InventoryTools.Services;

public enum AcquisitionReason
{
    Crafting,
    Gathering,
    Shopping,
    CombatDrop,
    Other
}


public class SimpleAcquisitionTrackerService : IHostedService
{
    private readonly IInventoryScanner _inventoryScanner;
    private readonly IClientState _clientState;
    private readonly ICondition _condition;
    private readonly ShopTrackerService _shopTrackerService;
    private InventoryItem[]? _bags1 = null;
    private InventoryItem[]? _bags2 = null;
    private InventoryItem[]? _bags3 = null;
    private InventoryItem[]? _bags4 = null;
    private InventoryItem[]? _currency = null;
    private InventoryItem[]? _crystals = null;

    public delegate void ItemAcquiredDelegate(InventoryItem item, int qtyIncrease, AcquisitionReason reason);

    public event ItemAcquiredDelegate? ItemAcquired;

    public SimpleAcquisitionTrackerService(IInventoryScanner inventoryScanner, IClientState clientState, ICondition condition, ShopTrackerService shopTrackerService)
    {
        _inventoryScanner = inventoryScanner;
        _clientState = clientState;
        _condition = condition;
        _shopTrackerService = shopTrackerService;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _inventoryScanner.BagsChanged += InventoryScannerOnBagsChanged;
        _clientState.Login += ClientLoggedIn;
        _clientState.Logout += ClientStateOnLogout;
        return Task.CompletedTask;
    }

    private void ClientStateOnLogout(int type, int code)
    {
        _bags1 = null;
        _bags2 = null;
        _bags3 = null;
        _bags4 = null;
        _currency = null;
        _crystals = null;
    }

    private void ClientLoggedIn()
    {

    }

    private void InventoryScannerOnBagsChanged(List<BagChange> changes)
    {
        if (!_clientState.IsLoggedIn)
        {
            return;
        }

        if (!changes.Any(c =>
                c.InventoryType == InventoryType.Inventory1 || c.InventoryType == InventoryType.Inventory2 ||
                c.InventoryType == InventoryType.Inventory3 || c.InventoryType == InventoryType.Inventory4 || c.InventoryType == InventoryType.Crystals || c.InventoryType == InventoryType.Currency))
        {
            return;
        }

        if (_bags1 == null || _bags2 == null || _bags3 == null || _bags4 == null || _currency == null || _crystals == null)
        {
            _bags1 = _inventoryScanner.CharacterBag1.ToArray();
            _bags2 = _inventoryScanner.CharacterBag2.ToArray();
            _bags3 = _inventoryScanner.CharacterBag3.ToArray();
            _bags4 = _inventoryScanner.CharacterBag4.ToArray();
            _currency = _inventoryScanner.CharacterCurrency.ToArray();
            _crystals = _inventoryScanner.CharacterCrystals.ToArray();
            return;
        }
        foreach (var bagChange in changes)
        {
            InventoryItem[]? selectedBag = null;
            if (bagChange.InventoryType is InventoryType.Inventory1)
            {
                selectedBag = _bags1;
            }
            else if (bagChange.InventoryType is InventoryType.Inventory2)
            {
                selectedBag = _bags2;
            }
            else if (bagChange.InventoryType is InventoryType.Inventory3)
            {
                selectedBag = _bags3;
            }
            else if (bagChange.InventoryType is InventoryType.Inventory4)
            {
                selectedBag = _bags4;
            }
            else if (bagChange.InventoryType is InventoryType.Currency)
            {
                selectedBag = _currency;
            }
            else if (bagChange.InventoryType is InventoryType.Crystals)
            {
                selectedBag = _crystals;
            }

            if (selectedBag != null)
            {
                int qtyIncrease = 0;
                var originalItem = selectedBag[bagChange.Item.Slot];
                if (originalItem.ItemId == bagChange.Item.ItemId && originalItem.Flags == bagChange.Item.Flags)
                {
                    if (originalItem.Slot == bagChange.Item.Slot && originalItem.Container == bagChange.Item.Container)
                    {
                        //Item increased
                        if (bagChange.Item.Quantity > originalItem.Quantity)
                        {
                            qtyIncrease = bagChange.Item.Quantity - originalItem.Quantity;
                        }
                    }
                    else
                    {
                        //Item added
                        if (bagChange.Item.Quantity > originalItem.Quantity)
                        {
                            qtyIncrease = originalItem.Quantity - bagChange.Item.Quantity;
                        }
                    }

                }
                else if(originalItem.ItemId == 0)
                {
                    qtyIncrease = bagChange.Item.Quantity;
                }

                var acquisitionReason = AcquisitionReason.Other;

                if (_condition[ConditionFlag.Crafting] || _condition[ConditionFlag.Crafting40])
                {
                    acquisitionReason = AcquisitionReason.Crafting;
                }
                else if (_condition[ConditionFlag.Gathering] || _condition[ConditionFlag.Gathering42])
                {
                    acquisitionReason = AcquisitionReason.Gathering;
                }
                else if (_shopTrackerService.GetCurrentShopType() != null)
                {
                    acquisitionReason = AcquisitionReason.Shopping;
                }
                else if (_condition[ConditionFlag.InCombat])
                {
                    acquisitionReason = AcquisitionReason.CombatDrop;
                }

                if (qtyIncrease != 0)
                {
                    ItemAcquired?.Invoke(bagChange.Item, qtyIncrease, acquisitionReason);
                }
            }
        }
        _bags1 = _inventoryScanner.CharacterBag1.ToArray();
        _bags2 = _inventoryScanner.CharacterBag2.ToArray();
        _bags3 = _inventoryScanner.CharacterBag3.ToArray();
        _bags4 = _inventoryScanner.CharacterBag4.ToArray();
        _currency = _inventoryScanner.CharacterCurrency.ToArray();
        _crystals = _inventoryScanner.CharacterCrystals.ToArray();
    }


    public Task StopAsync(CancellationToken cancellationToken)
    {
        _inventoryScanner.BagsChanged -= InventoryScannerOnBagsChanged;
        _clientState.Login -= ClientLoggedIn;
        _clientState.Logout -= ClientStateOnLogout;
        return Task.CompletedTask;
    }
}