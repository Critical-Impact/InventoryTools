using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AllaganLib.GameSheets.Sheets;
using DalaMock.Host.Mediator;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Plugin;
using Dalamud.Plugin.Ipc;
using Dalamud.Plugin.Services;
using Dalamud.Bindings.ImGui;
using InventoryTools.Logic;
using InventoryTools.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace InventoryTools.IPC;

public interface IChat2Ipc : IHostedService
{
}

public class Chat2Ipc : IChat2Ipc
{
    private readonly ILogger<Chat2Ipc> _logger;
    private readonly ImGuiMenuService _menuService;
    private readonly MediatorService _mediatorService;
    private readonly ItemSheet _itemSheet;

    private ICallGateSubscriber<string> RegisterCallGate { get; }

    private ICallGateSubscriber<string, object?> UnregisterCallGate { get; }

    private ICallGateSubscriber<object?> AvailableCallGate { get; }

    private ICallGateSubscriber<string, PlayerPayload?, ulong, Payload?, SeString?, SeString?, object?> InvokeCallGate { get; }

    private string? _id;

    public Chat2Ipc(IDalamudPluginInterface pluginInterface, ILogger<Chat2Ipc> logger, ImGuiMenuService menuService, MediatorService mediatorService, ItemSheet itemSheet) {
        _logger = logger;
        _menuService = menuService;
        _mediatorService = mediatorService;
        _itemSheet = itemSheet;
        RegisterCallGate = pluginInterface.GetIpcSubscriber<string>("ChatTwo.Register");
        UnregisterCallGate = pluginInterface.GetIpcSubscriber<string, object?>("ChatTwo.Unregister");
        InvokeCallGate = pluginInterface.GetIpcSubscriber<string, PlayerPayload?, ulong, Payload?, SeString?, SeString?, object?>("ChatTwo.Invoke");
        AvailableCallGate = pluginInterface.GetIpcSubscriber<object?>("ChatTwo.Available");
    }

    private void Register() {
        try
        {
            _id = RegisterCallGate.InvokeFunc();
            _logger.LogTrace("Attempting to register with chat2");
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Something went wrong while trying to register with Chat2's IPC. Ignore this if you don't have it installed.");
        }

    }

    public void Disable() {

        if (_id != null) {
            try
            {
                UnregisterCallGate.InvokeAction(_id);
                _logger.LogTrace("Attempting to unregister with chat2's IPC");
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "Something went wrong while trying to unregister with Chat2's IPC. Ignore this if you don't have it installed.");
            }
            _id = null;
        }
        try
        {
            InvokeCallGate.Unsubscribe(Integration);
            _logger.LogTrace("Attempting to unsubscribe with chat2's IPC");
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Something went wrong while trying to unsubscribe from Chat2's IPC. Ignore this if you don't have it installed.");
        }
    }

    private void Integration(string id, PlayerPayload? sender, ulong contentId, Payload? payload, SeString? senderString, SeString? content) {
        // Make sure the ID is the same as the saved registration ID.
        if (id != _id) {
            return;
        }

        if (payload is ItemPayload itemPayload)
        {
            using(var menu = ImRaii.Menu("Allagan Tools"))
            {
                if (menu)
                {
                    List<MessageBase> messages = [];
                    _menuService.DrawRightClickPopup(new SearchResult(_itemSheet.GetRow(itemPayload.ItemId)), messages);
                    if (messages.Count != 0)
                    {
                        _mediatorService.Publish(messages);
                    }
                }
            }
        }

    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        AvailableCallGate.Subscribe(Register);
        Register();
        InvokeCallGate.Subscribe(Integration);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        Disable();
        return Task.CompletedTask;
    }
}