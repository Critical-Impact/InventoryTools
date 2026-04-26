using System.Threading;
using System.Threading.Tasks;
using DalaMock.Host.Mediator;
using Dalamud.Plugin.Services;
using InventoryTools.Compendium.Windows;
using InventoryTools.Mediator;
using Microsoft.Extensions.Hosting;

namespace InventoryTools.Services;

public class POIService : IHostedService, IMediatorSubscriber
{
    private readonly IClientState _clientState;

    public POIService(MediatorService mediatorService, IClientState clientState)
    {
        _clientState = clientState;
        MediatorService = mediatorService;
    }
    public Task StartAsync(CancellationToken cancellationToken)
    {
        MediatorService.Subscribe<OpenCurrentPOIMessage>(this, OpenCurrentPOI);
        return Task.CompletedTask;
    }

    private void OpenCurrentPOI(OpenCurrentPOIMessage obj)
    {
        if (_clientState.IsLoggedIn && _clientState.TerritoryType != 0)
        {
            MediatorService.Publish(new OpenUintWindowMessage(typeof(CompendiumMapFeaturesWindow), _clientState.TerritoryType));
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        MediatorService.UnsubscribeAll(this);
        return Task.CompletedTask;
    }

    public MediatorService MediatorService { get; }
}