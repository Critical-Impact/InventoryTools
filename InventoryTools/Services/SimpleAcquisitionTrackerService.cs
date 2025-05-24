using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CriticalCommonLib.Services.Ui;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.Inventory;
using Dalamud.Game.Inventory.InventoryEventArgTypes;
using Dalamud.Hooking;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI;
using InventoryTools.Logic.Settings;
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

public class SimpleAcquisitionTrackerService : ISimpleAcquisitionTrackerService
{
    private readonly IGameInventory _gameInventory;
    private readonly IClientState _clientState;
    private readonly ICondition _condition;
    private readonly ShopTrackerService _shopTrackerService;
    private readonly IGameUiManager _gameUiManager;
    private readonly IFramework _framework;
    private readonly ILogger<SimpleAcquisitionTrackerService> _logger;
    private readonly IGameInteropProvider _gameInteropProvider;
    private readonly AcquisitionTrackerPersistStateSetting _persistStateSetting;
    private readonly AcquisitionTrackerLoginDelaySetting _loginDelaySetting;
    private readonly InventoryToolsConfiguration _configuration;
    private Dictionary<(uint, InventoryItem.ItemFlags), long> _itemCounts = new();
    private AcquisitionReason _currentState = AcquisitionReason.Other;
    private DateTime? _stateChangeTime;
    private bool _initialCheckPerformed;
    private bool _pluginBootCheckPerformed;
    private DateTime? _lastLoginTime;
    private Hook<RaptureAtkModuleUpdateDelegate>? _raptureAtkModuleUpdateHook;

    public delegate void ItemAcquiredDelegate(uint itemId, InventoryItem.ItemFlags itemFlags, int qtyIncrease, AcquisitionReason reason);
    private unsafe delegate void RaptureAtkModuleUpdateDelegate(RaptureAtkModule* ram, float f1);

    public event ItemAcquiredDelegate? ItemAcquired;

    public SimpleAcquisitionTrackerService(IGameInventory gameInventory,
        IClientState clientState,
        ICondition condition,
        ShopTrackerService shopTrackerService,
        IGameUiManager gameUiManager,
        IFramework framework,
        ILogger<SimpleAcquisitionTrackerService> logger,
        IGameInteropProvider gameInteropProvider,
        AcquisitionTrackerPersistStateSetting persistStateSetting,
        AcquisitionTrackerLoginDelaySetting loginDelaySetting,
        InventoryToolsConfiguration configuration)
    {
        _gameInventory = gameInventory;
        _clientState = clientState;
        _condition = condition;
        _shopTrackerService = shopTrackerService;
        _gameUiManager = gameUiManager;
        _framework = framework;
        _logger = logger;
        _gameInteropProvider = gameInteropProvider;
        _persistStateSetting = persistStateSetting;
        _loginDelaySetting = loginDelaySetting;
        _configuration = configuration;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _clientState.Login += ClientLoggedIn;
        _clientState.Logout += ClientStateOnLogout;
        _framework.Update += FrameworkOnUpdate;
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        this._raptureAtkModuleUpdateHook?.Disable();
        _clientState.Login -= ClientLoggedIn;
        _clientState.Logout -= ClientStateOnLogout;
        _framework.Update -= FrameworkOnUpdate;
        return Task.CompletedTask;
    }


    private unsafe void RaptureAtkModuleUpdateDetour(RaptureAtkModule* ram, float f1)
    {
        var agentUpdateFlag = ram->AgentUpdateFlag;
        if (agentUpdateFlag != 0)
        {
            var actualFlags = Enum.GetValues(agentUpdateFlag.GetType()).Cast<Enum>().Where(agentUpdateFlag.HasFlag).Select(c => c.ToString());
            _logger.LogTrace("Agent update flag is {AgentUpdateFlag}, types are {Types}", (int)agentUpdateFlag, actualFlags);
            try
            {
                if (_initialCheckPerformed && _pluginBootCheckPerformed && agentUpdateFlag.HasFlag(RaptureAtkModule.AgentUpdateFlags.InventoryUpdate))
                {
                    CalculateItemCounts();
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed in RaptureAtkModuleUpdateDetour");
            }

        }

        this._raptureAtkModuleUpdateHook!.Original(ram, f1);
    }

    public void CalculateItemCounts(bool notify = true)
    {
        if (!_clientState.IsLoggedIn)
        {
            return;
        }
        _logger.LogTrace("Calculating item counts, set to notify: {Notify}", notify ? "true" : "false");
        GameInventoryType[] inventories =
        [
            GameInventoryType.Inventory1, GameInventoryType.Inventory2, GameInventoryType.Inventory3,
            GameInventoryType.Inventory4, GameInventoryType.Currency, GameInventoryType.Crystals
        ];

        var newItemCounts = new Dictionary<(uint, InventoryItem.ItemFlags), long>();
        foreach (var inventory in inventories)
        {
            foreach (var item in _gameInventory.GetInventoryItems(inventory))
            {
                var itemType = InventoryItem.ItemFlags.None;
                if (item.IsHq)
                {
                    itemType = InventoryItem.ItemFlags.HighQuality;
                }
                else if (item.IsCollectable)
                {
                    itemType = InventoryItem.ItemFlags.Collectable;
                }

                newItemCounts.TryAdd((item.BaseItemId, itemType), 0);
                newItemCounts[(item.BaseItemId, itemType)] += item.Quantity;
            }
        }

        if (notify)
        {
            var changed = new Dictionary<(uint, InventoryItem.ItemFlags), long>();

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
        if (_raptureAtkModuleUpdateHook == null)
        {
            unsafe
            {
                this._raptureAtkModuleUpdateHook = _gameInteropProvider.HookFromFunctionPointerVariable<RaptureAtkModuleUpdateDelegate>(
                    new(&RaptureAtkModule.StaticVirtualTablePointer->Update),
                    this.RaptureAtkModuleUpdateDetour);
            }

            this._raptureAtkModuleUpdateHook.Enable();
        }

        //When the plugin first loads, check to see if they are already logged in, if so then their inventory is probably already loaded so we can scan once and mark it as checked
        //If they are not logged in we'll need to wait for them to login and then scan
        if (!_pluginBootCheckPerformed)
        {
            _logger.LogTrace("Performing initial tracker service login check");
            _pluginBootCheckPerformed = true;
            if (_clientState.IsLoggedIn)
            {
                _logger.LogTrace("Character already logged in, checking item counts.");
                CalculateItemCounts(false);
                _initialCheckPerformed = true;
            }
            else
            {
                _logger.LogTrace("Character not logged in, waiting until login to start.");
            }
        }

        if (!_clientState.IsLoggedIn)
        {
            return;
        }

        if (!_initialCheckPerformed)
        {
            if (_lastLoginTime == null)
            {
                _logger.LogTrace("Character logged in but no login time detected, setting to now.");
                _lastLoginTime = DateTime.Now;
            }

            var loginDelay = _loginDelaySetting.CurrentValue(_configuration);
            if (_lastLoginTime.Value.AddSeconds(loginDelay) <= DateTime.Now)
            {
                _logger.LogTrace("{LoginDelay} seconds has elapsed since login, generating item counts.", loginDelay);
                _lastLoginTime = null;
                _initialCheckPerformed = true;
                CalculateItemCounts(false);
            }
            else
            {
                return;
            }
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
                    var persistStateSecond = _persistStateSetting.CurrentValue(_configuration);
                    _logger.LogTrace("Currently in {CurrentState}, moving to {NewState} in {Time} seconds", _currentState, acquisitionReason, persistStateSecond);
                    _stateChangeTime = DateTime.Now + TimeSpan.FromSeconds(persistStateSecond);
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
        _logger.LogTrace("Character has logged out, clearing items, setting initial check and removing last login time.");
        _itemCounts.Clear();
        _initialCheckPerformed = false;
        _lastLoginTime = null;
    }

    private void ClientLoggedIn()
    {
        _logger.LogTrace("Character has logged in, setting initial check and setting last login time..");
        _initialCheckPerformed = false;
        _lastLoginTime = DateTime.Now;
    }

    public void Dispose()
    {
        _raptureAtkModuleUpdateHook?.Dispose();
    }
}