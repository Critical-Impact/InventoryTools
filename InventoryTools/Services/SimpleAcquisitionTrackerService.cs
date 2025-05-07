using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CriticalCommonLib.Services.Ui;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.Inventory;
using Dalamud.Game.Inventory.InventoryEventArgTypes;
using Dalamud.Plugin.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Services;

public enum AcquisitionReason
{
    Crafting,
    Gathering,
    Shopping,
    CombatDrop,
    Marketboard,
    Other
}

public enum ItemType
{
    Normal,
    Hq,
    Collectable
}

public class SimpleAcquisitionTrackerService : IHostedService
{
    private readonly IGameInventory _gameInventory;
    private readonly IClientState _clientState;
    private readonly ICondition _condition;
    private readonly ShopTrackerService _shopTrackerService;
    private readonly IGameUiManager _gameUiManager;
    private readonly IFramework _framework;
    private readonly ILogger<SimpleAcquisitionTrackerService> _logger;
    private Dictionary<(uint, ItemType), long> _itemCounts = new();
    private AcquisitionReason _currentState = AcquisitionReason.Other;
    private DateTime? _stateChangeTime;
    private bool _initialCheckPerformed;
    private const int PersistStateSeconds = 2;
    private const int MaxEvents = 20;

    public delegate void ItemAcquiredDelegate(uint itemId, ItemType itemType, int qtyIncrease, AcquisitionReason reason);

    public event ItemAcquiredDelegate? ItemAcquired;

    public SimpleAcquisitionTrackerService(IGameInventory gameInventory, IClientState clientState, ICondition condition, ShopTrackerService shopTrackerService, IGameUiManager gameUiManager, IFramework framework, ILogger<SimpleAcquisitionTrackerService> logger)
    {
        _gameInventory = gameInventory;
        _clientState = clientState;
        _condition = condition;
        _shopTrackerService = shopTrackerService;
        _gameUiManager = gameUiManager;
        _framework = framework;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _gameInventory.InventoryChanged += InventoryChanged;
        _clientState.Login += ClientLoggedIn;
        _clientState.Logout += ClientStateOnLogout;
        _framework.Update += FrameworkOnUpdate;
        return Task.CompletedTask;
    }

    private void InventoryChanged(IReadOnlyCollection<InventoryEventArgs> events)
    {
        if (events.Count < MaxEvents)
        {
            CalculateItemCounts();
        }
        else
        {
            _logger.LogTrace("Detected {TotalEvents} events, limit is {MaxEvents}", events.Count, MaxEvents);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _gameInventory.InventoryChanged -= InventoryChanged;
        _clientState.Login -= ClientLoggedIn;
        _clientState.Logout -= ClientStateOnLogout;
        _framework.Update -= FrameworkOnUpdate;
        return Task.CompletedTask;
    }

    public void CalculateItemCounts(bool notify = true)
    {
        _logger.LogTrace("Calculating item counts, set to notify: {Notify}", notify ? "true" : "false");
        GameInventoryType[] inventories =
        [
            GameInventoryType.Inventory1, GameInventoryType.Inventory2, GameInventoryType.Inventory3,
            GameInventoryType.Inventory4, GameInventoryType.Currency, GameInventoryType.Crystals
        ];

        var newItemCounts = new Dictionary<(uint, ItemType), long>();
        foreach (var inventory in inventories)
        {
            foreach (var item in _gameInventory.GetInventoryItems(inventory))
            {
                var itemType = ItemType.Normal;
                if (item.IsHq)
                {
                    itemType = ItemType.Hq;
                }
                else if (item.IsCollectable)
                {
                    itemType = ItemType.Collectable;
                }

                newItemCounts.TryAdd((item.ItemId, itemType), 0);
                newItemCounts[(item.ItemId, itemType)] += item.Quantity;
            }
        }

        if (notify)
        {
            var changed = new Dictionary<(uint, ItemType), long>();

            foreach (var itemCount in newItemCounts)
            {
                if (_itemCounts.TryGetValue(itemCount.Key, out var oldCount))
                {
                    var newCount = itemCount.Value;
                    if (newCount > oldCount)
                    {
                        changed.Add(itemCount.Key, newCount - oldCount);
                    }
                }
                else
                {
                    changed.Add(itemCount.Key, itemCount.Value);
                }
            }

            foreach (var itemChange in changed)
            {
                _logger.LogTrace("Item change detected, {ItemId} of {ItemType} quality increased by {Quantity}", itemChange.Key.Item1, itemChange.Key.Item2, itemChange.Value);
                ItemAcquired?.Invoke(itemChange.Key.Item1, itemChange.Key.Item2, (int)itemChange.Value, _currentState);
            }
        }

        _itemCounts = newItemCounts;

    }

    private void FrameworkOnUpdate(IFramework framework)
    {
        if (!_clientState.IsLoggedIn)
        {
            return;
        }

        if (!_initialCheckPerformed)
        {
            CalculateItemCounts(false);
            _initialCheckPerformed = true;
        }
        var acquisitionReason = AcquisitionReason.Other;

        if (_condition[ConditionFlag.Crafting] || _condition[ConditionFlag.ExecutingCraftingAction])
        {
            acquisitionReason = AcquisitionReason.Crafting;
        }
        else if (_condition[ConditionFlag.Gathering] || _condition[ConditionFlag.ExecutingGatheringAction])
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
        else if (_gameUiManager.IsWindowVisible(WindowName.ItemSearch))
        {
            acquisitionReason = AcquisitionReason.Marketboard;
        }

        if (_currentState != acquisitionReason)
        {
            if (_stateChangeTime == null)
            {
                if (_currentState == AcquisitionReason.Other)
                {
                    _logger.LogTrace("Currently in {CurrentState}, moving to {NewState}", _currentState, acquisitionReason);
                    _currentState = acquisitionReason;
                }
                else
                {
                    _logger.LogTrace("Currently in {CurrentState}, moving to {NewState} in {Time} seconds", _currentState, acquisitionReason, PersistStateSeconds);
                    _stateChangeTime = DateTime.Now + TimeSpan.FromSeconds(PersistStateSeconds);
                }
            }
            else if (DateTime.Now > _stateChangeTime)
            {
                _stateChangeTime = null;
                _currentState = acquisitionReason;
                _logger.LogTrace("State changed to {NewState}",_currentState);
            }
        }
    }

    private void ClientStateOnLogout(int type, int code)
    {
        _itemCounts.Clear();
    }

    private void ClientLoggedIn()
    {
        CalculateItemCounts(false);
    }
}