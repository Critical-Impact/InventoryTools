using System.Threading;
using System.Threading.Tasks;
using CriticalCommonLib.Services;
using CriticalCommonLib.Services.Mediator;

using DalaMock.Shared.Interfaces;
using InventoryTools.Mediator;
using InventoryTools.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Ui
{
    using Dalamud.Plugin;

    public partial class InventoryToolsUi : DisposableMediatorSubscriberBase, IHostedService
    {
        private readonly IDalamudPluginInterface _pluginInterfaceService;
        private readonly ICharacterMonitor _characterMonitor;
        private readonly WindowService _windowService;
        private readonly IFileDialogManager _fileDialogManager;
        private readonly InventoryToolsConfiguration _configuration;
        private bool _disposing = false;

        public InventoryToolsUi(IDalamudPluginInterface pluginInterfaceService, ILogger<InventoryToolsUi> logger, MediatorService mediatorService, ICharacterMonitor characterMonitor, WindowService windowService, IFileDialogManager fileDialogManager, InventoryToolsConfiguration configuration) : base(logger, mediatorService)
        {
            _pluginInterfaceService = pluginInterfaceService;
            _characterMonitor = characterMonitor;
            _windowService = windowService;
            _fileDialogManager = fileDialogManager;
            _configuration = configuration;
        }

        private void InterfaceOnOpenMainUi()
        {
            BypassLoginStatus = true;
            MediatorService.Publish(new OpenGenericWindowMessage(typeof(FiltersWindow)));
        }

        private void UiBuilderOnOpenConfigUi()
        {
            BypassLoginStatus = true;
            MediatorService.Publish(new OpenGenericWindowMessage(typeof(ConfigurationWindow)));
        }

        public bool BypassLoginStatus { get; set; }

        public bool IsVisible
        {
            get => _configuration.IsVisible;
            set => _configuration.IsVisible = value;
        }

        public void Draw()
        {
            if (!_characterMonitor.IsLoggedIn && !BypassLoginStatus || _disposing)
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
            Logger.LogTrace("Starting service {type} ({this})", GetType().Name, this);
            _pluginInterfaceService.UiBuilder.Draw += Draw;
            _pluginInterfaceService.UiBuilder.OpenConfigUi += UiBuilderOnOpenConfigUi;
            _pluginInterfaceService.UiBuilder.OpenMainUi += InterfaceOnOpenMainUi;
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Logger.LogTrace("Stopping service {type} ({this})", GetType().Name, this);
            _pluginInterfaceService.UiBuilder.Draw -= Draw;
            _pluginInterfaceService.UiBuilder.OpenConfigUi -= UiBuilderOnOpenConfigUi;
            _pluginInterfaceService.UiBuilder.OpenMainUi -= InterfaceOnOpenMainUi;
            return Task.CompletedTask;
        }
    }
}