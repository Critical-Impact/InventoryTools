using CriticalCommonLib.Services.Mediator;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Mediator;

public abstract class MediatorBackgroundService : BackgroundService, IMediatorSubscriber
{
    protected ILogger Logger { get; }
    public MediatorService MediatorService { get; }
    
    protected MediatorBackgroundService(ILogger logger, MediatorService mediatorService)
    {
        Logger = logger;

        Logger.LogTrace("Creating {type} ({this})", GetType().Name, this);
        MediatorService = mediatorService;
    }
    
    protected void UnsubscribeAll()
    {
        Logger.LogTrace("Unsubscribing from all for {type} ({this})", GetType().Name, this);
        MediatorService.UnsubscribeAll(this);
    }
}