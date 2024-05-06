using System;
using System.Threading;
using System.Threading.Tasks;
using CriticalCommonLib.Services.Mediator;
using DalaMock.Shared.Interfaces;
using Dalamud.Interface;
using Dalamud.Interface.Internal;
using Dalamud.Plugin.Services;
using InventoryTools.Mediator;
using InventoryTools.Services.Interfaces;
using InventoryTools.Ui;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Services;

public class LaunchButtonService : DisposableMediatorSubscriberBase, IHostedService
{
    private readonly ITitleScreenMenu _titleScreenMenu;
    private readonly IIconService _iconService;
    private readonly IPluginInterfaceService _pluginInterfaceService;
    private readonly ILogger<LaunchButtonService> _logger;
    private readonly InventoryToolsUi _inventoryToolsUi;
    private readonly InventoryToolsConfiguration _inventoryToolsConfiguration;
    private readonly ConfigurationManagerService _configurationManagerService;
    private IDalamudTextureWrap?  _icon;
    private TitleScreenMenuEntry? _entry;
    private readonly string           _fileName;
    
    public LaunchButtonService(ILogger<LaunchButtonService> logger, MediatorService mediatorService, ITitleScreenMenu titleScreenMenu, IIconService iconService, IPluginInterfaceService pluginInterfaceService, InventoryToolsUi inventoryToolsUi, InventoryToolsConfiguration inventoryToolsConfiguration, ConfigurationManagerService configurationManagerService) : base(logger,mediatorService)
    {
        _titleScreenMenu = titleScreenMenu;
        _iconService = iconService;
        _pluginInterfaceService = pluginInterfaceService;
        _logger = logger;
        _inventoryToolsUi = inventoryToolsUi;
        _inventoryToolsConfiguration = inventoryToolsConfiguration;
        _configurationManagerService = configurationManagerService;
        _fileName       =  "menu-icon";
    }

    private void CreateEntry()
    {
        if (_entry != null)
        {
            _pluginInterfaceService.Draw -= CreateEntry;
            return;
        }
        try
        {
            _icon = _iconService.LoadImage(_fileName);
            if (_icon != null)
            {
                _entry = _titleScreenMenu.AddEntry("Allagan Tools", _icon, OnTriggered);
            }
            else
            {
                _logger.LogError($"Could not load icon to add to title screen menu.");
            }

            _pluginInterfaceService.Draw -= CreateEntry;
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
        _pluginInterfaceService.Draw -= RemoveEntry;
        if (_entry != null)
        {
            _titleScreenMenu.RemoveEntry(_entry);
            _entry = null;
        }
    }

    protected override void Dispose(bool disposing)
    {
        _icon?.Dispose();
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
            _pluginInterfaceService.Draw += CreateEntry;
        }
        else
        {
            _pluginInterfaceService.Draw += RemoveEntry;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _configurationManagerService.ConfigurationChanged -= ConfigurationManagerServiceOnConfigurationChanged;
        return Task.CompletedTask;
    }
}