using AllaganLib.GameSheets.Sheets;

namespace InventoryTools.Services;

using System.Threading;
using System.Threading.Tasks;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Services.Ui;
using Microsoft.Extensions.Hosting;

public class HostedCraftMonitor : CraftMonitor, IHostedService
{
    public HostedCraftMonitor(IGameUiManager gameUiManager, RecipeSheet recipeSheet) : base(gameUiManager, recipeSheet)
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