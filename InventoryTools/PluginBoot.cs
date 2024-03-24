using System.Threading;
using System.Threading.Tasks;
using CriticalCommonLib.Services.Mediator;
using Dalamud.Plugin.Services;
using InventoryTools.Mediator;
using InventoryTools.Services;
using InventoryTools.Ui;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace InventoryTools;

public class PluginBoot : DisposableMediatorSubscriberBase, IHostedService
{
    private readonly IConfigurationWizardService _configurationWizardService;
    
    public PluginBoot(IConfigurationWizardService configurationWizardService, ILogger<PluginBoot> logger, MediatorService mediatorService) : base(logger, mediatorService)
    {
        _configurationWizardService = configurationWizardService;
    }


    public Task StartAsync(CancellationToken cancellationToken)
    {
        Logger.LogTrace("Starting service {type} ({this})", GetType().Name, this);
        if (_configurationWizardService.ShouldShowWizard)
        {
            MediatorService.Publish(new OpenGenericWindowMessage(typeof(ConfigurationWizard)));
        }
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }


}