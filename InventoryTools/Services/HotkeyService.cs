using System.Collections.Generic;
using Dalamud.Game.ClientState.Keys;
using InventoryTools.Hotkeys;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CriticalCommonLib.Services.Mediator;
using DalaMock.Host.Mediator;
using Dalamud.Plugin.Services;
using InventoryTools.Services.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Services;

public class HotkeyService : DisposableMediatorSubscriberBase, IHotkeyService, IHostedService
{
    private IFramework _frameworkService;
    private IKeyState _keyStateService;
    private readonly ILogger _logger;
    private readonly MediatorService _mediatorService;
    private List<IHotkey> _hotKeys;
    public HotkeyService(ILogger<HotkeyService> logger, MediatorService mediatorService, IFramework framework, IKeyState keyState, IEnumerable<IHotkey> hotkeys) : base(logger, mediatorService)
    {
        _hotKeys = new List<IHotkey>();
        _frameworkService = framework;
        _keyStateService = keyState;
        _logger = logger;
        _mediatorService = mediatorService;
        _hotKeys = hotkeys.ToList();
    }

    public void AddHotkey<T>() where T : IHotkey, new()
    {
        var hotKeys = _hotKeys.ToList();
        var hotKey = new T();
        hotKeys.Add(hotKey);
        _hotKeys = hotKeys;
    }

    public void AddHotkey(IHotkey instance)
    {
        var hotKeys = _hotKeys.ToList();
        hotKeys.Add(instance);
        _hotKeys = hotKeys;
    }

    public void AddHotkey<T>(T instance) where T : IHotkey
    {
        var hotKeys = _hotKeys.ToList();
        hotKeys.Add(instance);
        _hotKeys = hotKeys;
    }

    private void FrameworkServiceOnUpdate(IFramework framework)
    {
        foreach (var hotkey in _hotKeys)
        {
            var modifiableHotkey = hotkey.ModifiableHotkey;
            if (modifiableHotkey != null)
            {
                var hotkeyVirtualKeys = hotkey.VirtualKeys;
                if (hotkeyVirtualKeys != null && HotkeyPressed(hotkeyVirtualKeys))
                {
                    if (hotkey.OnHotKey() && !hotkey.PassToGame)
                    {
                        foreach (var k in hotkeyVirtualKeys)
                        {
                            _keyStateService[(int)k] = false;
                        }
                    }
                }
            }
        }
    }

    private bool HotkeyPressed(VirtualKey[] keys) {
        if (keys.Length == 1 && keys[0] == VirtualKey.NO_KEY)
        {
            return false;
        }

        bool hotKeyPressed = keys.Length != 0;
        foreach (var vk in keys) {
            if (!_keyStateService[vk])
            {
                hotKeyPressed = false;
            }
        }
        return hotKeyPressed;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _frameworkService.Update += FrameworkServiceOnUpdate;
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _frameworkService.Update -= FrameworkServiceOnUpdate;
        return Task.CompletedTask;
    }
}