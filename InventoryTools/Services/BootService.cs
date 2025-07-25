using System.Threading;
using System.Threading.Tasks;
using CriticalCommonLib;
using CriticalCommonLib.Services;
using CriticalCommonLib.Services.Mediator;
using DalaMock.Host.Mediator;
using InventoryTools.Mediator;
using InventoryTools.Ui;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Services;

public class BootService : DisposableMediatorSubscriberBase, IHostedService
{
    private readonly IConfigurationWizardService _configurationWizardService;

    public BootService(IConfigurationWizardService configurationWizardService, ILogger<BootService> logger, MediatorService mediatorService) : base(logger, mediatorService)
    {
        _configurationWizardService = configurationWizardService;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        Logger.LogTrace("Starting service {type} ({this})", GetType().Name, this);
        MediatorService.Subscribe<PluginLoadedMessage>(this, PluginLoaded);
        return Task.CompletedTask;
    }

    private void PluginLoaded(PluginLoadedMessage obj)
    {
        if (_configurationWizardService.ShouldShowWizard)
        {
            MediatorService.Publish(new OpenGenericWindowMessage(typeof(ConfigurationWizard)));
        }
        MediatorService.Publish(new OpenSavedWindowsMessage());
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        Logger.LogTrace("Stopping service {type} ({this})", GetType().Name, this);
        return Task.CompletedTask;
    }


}