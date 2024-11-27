using System;
using CriticalCommonLib.Services.Mediator;

using Dalamud.Game.ClientState.Keys;
using InventoryTools.Extensions;
using Microsoft.Extensions.Logging;
using OtterGui.Classes;

namespace InventoryTools.Hotkeys;

public abstract class Hotkey : IHotkey, IMediatorSubscriber, IDisposable
{
    public ILogger Logger { get; }
    public MediatorService MediatorService { get; set; }
    public InventoryToolsConfiguration Configuration { get; }

    private VirtualKey[]? _keys;
    private ModifiableHotkey? _currentHotkey;
    
    public VirtualKey[]? VirtualKeys
    {
        get
        {
            if (ModifiableHotkey != _currentHotkey)
            {
                _currentHotkey = ModifiableHotkey;
                _keys = null;
            }
            if (_keys != null)
            {
                return _keys;
            }
            if (_keys == null && ModifiableHotkey.HasValue)
            {
                _keys = ModifiableHotkey.Value.VirtualKeys();
                if (_keys.Length == 0)
                {
                    _keys = null;
                }
            }
            return _keys;
        }
    }

    public Hotkey(ILogger logger, MediatorService mediatorService, InventoryToolsConfiguration configuration)
    {
        Logger = logger;
        MediatorService = mediatorService;
        Configuration = configuration;
        Logger.LogDebug("Creating {type}", GetType());
    }

    public abstract ModifiableHotkey? ModifiableHotkey { get; }
    public abstract bool OnHotKey();
    public bool PassToGame { get; }
    
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
    
    protected virtual void Dispose(bool disposing)
    {
        Logger.LogDebug("Disposing {type}", GetType());

        MediatorService.UnsubscribeAll(this);
    }
}