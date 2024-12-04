using AllaganLib.Shared.Time;
using Autofac;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Interfaces;
using CriticalCommonLib.Services;
using CriticalCommonLib.Services.Ui;

using DalaMock.Core.Mocks;
using DalaMock.Core.Windows;
using DalaMock.Host.Factories;
using DalaMock.Shared.Interfaces;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using InventoryTools;
using InventoryTools.IPC;
using InventoryTools.Logic;
using InventoryTools.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace InventoryToolsMock;

public class InventoryToolsPluginMock : InventoryToolsPlugin
{
    private Logger seriLog;

    public InventoryToolsPluginMock(IDalamudPluginInterface pluginInterface, IPluginLog pluginLog, IAddonLifecycle addonLifecycle, IChatGui chatGui, IClientState clientState, ICommandManager commandManager, ICondition condition, IDataManager dataManager, IFramework framework, IGameGui gameGui, IGameInteropProvider gameInteropProvider, IKeyState keyState, IGameNetwork gameNetwork, IObjectTable objectTable, ITargetManager targetManager, ITextureProvider textureProvider, IToastGui toastGui, IContextMenu contextMenu, ITitleScreenMenu titleScreenMenu) : base(pluginInterface, pluginLog, addonLifecycle, chatGui, clientState, commandManager, condition, dataManager, framework, gameGui, gameInteropProvider, keyState, gameNetwork, objectTable, targetManager, textureProvider, toastGui, contextMenu, titleScreenMenu)
    {
    }

    public override void ReplaceHostedServices(Dictionary<Type, Type> replacements)
    {
        replacements.Add(typeof(WotsitIpc),typeof(MockWotsitIpc));
        replacements.Add(typeof(CraftMonitor),typeof(MockHostedCraftMonitor));
        replacements.Add(typeof(OdrScanner),typeof(MockOdrScanner));
    }

    public override void PreBuild(IHostBuilder hostBuilder)
    {
        base.PreBuild(hostBuilder);

        this.seriLog = new LoggerConfiguration()
            .WriteTo.Console(standardErrorFromLevel: LogEventLevel.Verbose)
            .CreateLogger();

        hostBuilder.ConfigureContainer<ContainerBuilder>(container =>
        {
            container.RegisterType<MockWindow>().SingleInstance();
            container.RegisterType<MockGameItemsWindow>().SingleInstance();
            container.RegisterType<IconBrowserWindow>().SingleInstance();
            container.RegisterType<MockFont>().As<IFont>().SingleInstance();
            container.RegisterType<MockCharacterMonitor>().As<ICharacterMonitor>().SingleInstance();
            container.RegisterType<MockTeleporterIpc>().As<ITeleporterIpc>().SingleInstance();
            container.RegisterType<MockChatUtilities>().As<IChatUtilities>().SingleInstance();
            container.RegisterType<MockGameInterface>().As<IGameInterface>().SingleInstance();
            container.RegisterType<MockGameUiManager>().As<IGameUiManager>().SingleInstance();
            container.RegisterType<MockClipboardService>().As<IClipboardService>().SingleInstance();
            container.RegisterType<MockSeTime>().As<ISeTime>().SingleInstance();
            container.RegisterType<MockWindowSystem>().As<IWindowSystem>().SingleInstance();
            container.RegisterType<MockGameInteropService>().As<IGameInteropService>().SingleInstance();
            container.RegisterType<MockUnlockTrackerService>().As<IUnlockTrackerService>().SingleInstance();
            container.RegisterType<MockQuestManagerService>().AsImplementedInterfaces().SingleInstance();
            container.RegisterType<MockStartup>().AsImplementedInterfaces().SingleInstance();
            container.RegisterType<MockFileDialogManager>().AsImplementedInterfaces().SingleInstance();
            container.RegisterInstance(seriLog).As<ILogger>().SingleInstance();
        });
    }
}