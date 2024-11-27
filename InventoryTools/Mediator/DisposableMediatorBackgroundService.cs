using System;
using CriticalCommonLib.Services.Mediator;

using Microsoft.Extensions.Logging;

namespace InventoryTools.Mediator;

public abstract class DisposableMediatorBackgroundService : MediatorBackgroundService
{
    protected DisposableMediatorBackgroundService(ILogger logger, MediatorService mediatorService) : base(logger, mediatorService)
    {
        logger.LogTrace("Starting background service {type} ({this})", GetType().Name, this);
    }
    
    public override void Dispose()
    {
        base.Dispose();
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        Logger.LogTrace("Disposing {type} ({this})", GetType().Name, this);
        UnsubscribeAll();
    }
}