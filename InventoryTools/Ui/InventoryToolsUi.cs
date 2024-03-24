using System;
using System.Threading;
using System.Threading.Tasks;
using CriticalCommonLib;
using CriticalCommonLib.Services;
using CriticalCommonLib.Services.Mediator;
using DalaMock.Shared.Interfaces;
using Dalamud.Interface.ImGuiFileDialog;
using Dalamud.Plugin.Services;
using InventoryTools.Logic;
using InventoryTools.Mediator;
using InventoryTools.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Ui
{
    public partial class InventoryToolsUi : DisposableMediatorSubscriberBase, IHostedService
    {
        private readonly IPluginInterfaceService _pluginInterfaceService;
        private readonly ICharacterMonitor _characterMonitor;
        private readonly WindowService _windowService;
        private readonly FileDialogManager _fileDialogManager;
        private readonly InventoryToolsConfiguration _configuration;
        private bool _disposing = false;
        
        public InventoryToolsUi(IPluginInterfaceService pluginInterfaceService, ILogger<InventoryToolsUi> logger, MediatorService mediatorService, ICharacterMonitor characterMonitor, WindowService windowService, FileDialogManager fileDialogManager, InventoryToolsConfiguration configuration) : base(logger, mediatorService)
        {
            _pluginInterfaceService = pluginInterfaceService;
            _characterMonitor = characterMonitor;
            _windowService = windowService;
            _fileDialogManager = fileDialogManager;
            _configuration = configuration;
        }

        private void InterfaceOnOpenMainUi()
        {
            MediatorService.Publish(new ToggleGenericWindowMessage(typeof(FiltersWindow)));
        }

        private void UiBuilderOnOpenConfigUi()
        {
            MediatorService.Publish(new ToggleGenericWindowMessage(typeof(ConfigurationWindow)));
        }

        public bool IsVisible
        {
            get => _configuration.IsVisible;
            set => _configuration.IsVisible = value;
        }

        public void Draw()
        {
            if (!_characterMonitor.IsLoggedIn || _disposing)
                return;
            _windowService.WindowSystem.Draw();

            _fileDialogManager.Draw();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {

            }

            base.Dispose(disposing);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _pluginInterfaceService.Draw += Draw;
            _pluginInterfaceService.OpenConfigUi += UiBuilderOnOpenConfigUi;
            _pluginInterfaceService.OpenMainUi += InterfaceOnOpenMainUi;
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _pluginInterfaceService.Draw -= Draw;
            _pluginInterfaceService.OpenConfigUi -= UiBuilderOnOpenConfigUi;
            _pluginInterfaceService.OpenMainUi -= InterfaceOnOpenMainUi;
            return Task.CompletedTask;
        }
    }
}