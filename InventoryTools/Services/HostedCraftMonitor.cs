using AllaganLib.GameSheets.Sheets;
using CriticalCommonLib.Services;
using Dalamud.Plugin.Services;
using Lumina.Excel;
using Lumina.Excel.Sheets;

namespace InventoryTools.Services;

using System.Threading;
using System.Threading.Tasks;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Services.Ui;
using Microsoft.Extensions.Hosting;

public class HostedCraftMonitor : CraftMonitor, IHostedService
{
    public HostedCraftMonitor(IGameUiManager gameUiManager, RecipeSheet recipeSheet, IClientState clientState, IFramework framework, IPluginLog pluginLog, ClassJobService classJobService, ExcelSheet<GathererCrafterLvAdjustTable> adjustSheet, RecipeLevelTableSheet recipeLevelTableSheet) : base(gameUiManager, recipeSheet, clientState, framework, pluginLog, classJobService, adjustSheet, recipeLevelTableSheet)
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