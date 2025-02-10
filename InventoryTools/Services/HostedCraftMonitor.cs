using AllaganLib.GameSheets.Sheets;
using Dalamud.Plugin.Services;

namespace InventoryTools.Services;

using System.Threading;
using System.Threading.Tasks;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Services.Ui;
using Microsoft.Extensions.Hosting;

public class HostedCraftMonitor : CraftMonitor, IHostedService
{
    public HostedCraftMonitor(IGameUiManager gameUiManager, RecipeSheet recipeSheet, IClientState clientState, IFramework framework, IPluginLog pluginLog) : base(gameUiManager, recipeSheet, clientState, framework, pluginLog)
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