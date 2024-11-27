using System.Threading;
using System.Threading.Tasks;
using CriticalCommonLib.Interfaces;
using CriticalCommonLib.Services.Mediator;

using InventoryTools.Mediator;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Services;

public class TeleporterService : DisposableMediatorSubscriberBase, IHostedService
{
    private readonly ITeleporterIpc _teleporterIpc;

    public TeleporterService(ILogger<TeleporterService> logger, MediatorService mediatorService, ITeleporterIpc teleporterIpc) : base(logger, mediatorService)
    {
        _teleporterIpc = teleporterIpc;
    }

    private void TeleportRequested(RequestTeleportMessage obj)
    {
        _teleporterIpc.Teleport(obj.aetheryteId);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        Logger.LogTrace("Starting service {type} ({this})", GetType().Name, this);
        MediatorService.Subscribe<RequestTeleportMessage>(this, TeleportRequested);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        Logger.LogTrace("Stopping service {type} ({this})", GetType().Name, this);
        return Task.CompletedTask;
    }
}