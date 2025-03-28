using System;
using System.Threading;
using System.Threading.Tasks;
using CriticalCommonLib.Services.Mediator;

using Dalamud.Interface;
using Dalamud.Plugin.Services;
using InventoryTools.Mediator;
using InventoryTools.Ui;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Services;

using System.IO;
using Dalamud.Interface.Textures.TextureWraps;
using Dalamud.Plugin;

public class LaunchButtonService : DisposableMediatorSubscriberBase, IHostedService
{
    private readonly ITitleScreenMenu _titleScreenMenu;
    private readonly IDalamudPluginInterface _pluginInterfaceService;
    private readonly ILogger<LaunchButtonService> _logger;
    private readonly ITextureProvider _textureProvider;
    private readonly InventoryToolsUi _inventoryToolsUi;
    private readonly InventoryToolsConfiguration _inventoryToolsConfiguration;
    private readonly ConfigurationManagerService _configurationManagerService;
    private IReadOnlyTitleScreenMenuEntry? _entry;
    private readonly string           _fileName;

    public LaunchButtonService(ILogger<LaunchButtonService> logger, ITextureProvider textureProvider, MediatorService mediatorService, ITitleScreenMenu titleScreenMenu, IDalamudPluginInterface pluginInterfaceService, InventoryToolsUi inventoryToolsUi, InventoryToolsConfiguration inventoryToolsConfiguration, ConfigurationManagerService configurationManagerService) : base(logger,mediatorService)
    {
        _titleScreenMenu = titleScreenMenu;
        _pluginInterfaceService = pluginInterfaceService;
        _logger = logger;
        _textureProvider = textureProvider;
        _inventoryToolsUi = inventoryToolsUi;
        _inventoryToolsConfiguration = inventoryToolsConfiguration;
        _configurationManagerService = configurationManagerService;
        var assemblyLocation = pluginInterfaceService.AssemblyLocation.DirectoryName!;
        _fileName = Path.Combine(assemblyLocation, Path.Combine("Images", "menu-icon.png"));
    }

    private void CreateEntry()
    {
        if (_entry != null)
        {
            _pluginInterfaceService.UiBuilder.Draw -= CreateEntry;
            return;
        }
        try
        {
            _entry = _titleScreenMenu.AddEntry("Allagan Tools", _textureProvider.GetFromFile(_fileName), OnTriggered);

            _pluginInterfaceService.UiBuilder.Draw -= CreateEntry;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Could not register title screen menu entry:\n{ex}");
        }
    }

    private void OnTriggered()
    {
        _inventoryToolsUi.BypassLoginStatus = !_inventoryToolsUi.BypassLoginStatus;
        if (_inventoryToolsUi.BypassLoginStatus)
        {
            MediatorService.Publish(new OpenGenericWindowMessage(typeof(FiltersWindow)));
        }
    }

    private void RemoveEntry()
    {
        _pluginInterfaceService.UiBuilder.Draw -= RemoveEntry;
        if (_entry != null)
        {
            _titleScreenMenu.RemoveEntry(_entry);
            _entry = null;
        }
    }

    protected override void Dispose(bool disposing)
    {
        RemoveEntry();
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _configurationManagerService.ConfigurationChanged += ConfigurationManagerServiceOnConfigurationChanged;
        ConfigurationManagerServiceOnConfigurationChanged();
        return Task.CompletedTask;
    }

    private void ConfigurationManagerServiceOnConfigurationChanged()
    {
        if (_inventoryToolsConfiguration.AddTitleMenuButton)
        {
            _pluginInterfaceService.UiBuilder.Draw += CreateEntry;
        }
        else
        {
            _pluginInterfaceService.UiBuilder.Draw += RemoveEntry;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _configurationManagerService.ConfigurationChanged -= ConfigurationManagerServiceOnConfigurationChanged;
        return Task.CompletedTask;
    }
}