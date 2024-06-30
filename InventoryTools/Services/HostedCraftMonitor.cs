namespace InventoryTools.Services;

using System.Threading;
using System.Threading.Tasks;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Services.Ui;
using Dalamud.Plugin.Services;
using Microsoft.Extensions.Hosting;

public class HostedCraftMonitor : CraftMonitor, IHostedService
{
    public HostedCraftMonitor(IGameUiManager gameUiManager) : base(gameUiManager)
    {
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}