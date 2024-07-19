using Autofac;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Interfaces;
using CriticalCommonLib.Services;
using CriticalCommonLib.Services.Ui;
using CriticalCommonLib.Time;
using DalaMock.Core.Mocks;
using DalaMock.Core.Windows;
using DalaMock.Host.Factories;
using DalaMock.Shared.Interfaces;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using InventoryTools;
using InventoryTools.Logic;
using InventoryTools.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace InventoryToolsMock;

public class InventoryToolsPluginMock : InventoryToolsPlugin
{
    public InventoryToolsPluginMock(IDalamudPluginInterface pluginInterface, IPluginLog pluginLog, IAddonLifecycle addonLifecycle, IChatGui chatGui, IClientState clientState, ICommandManager commandManager, ICondition condition, IDataManager dataManager, IFramework framework, IGameGui gameGui, IGameInteropProvider gameInteropProvider, IKeyState keyState, IGameNetwork gameNetwork, IObjectTable objectTable, ITargetManager targetManager, ITextureProvider textureProvider, IToastGui toastGui, IContextMenu contextMenu, ITitleScreenMenu titleScreenMenu) : base(pluginInterface, pluginLog, addonLifecycle, chatGui, clientState, commandManager, condition, dataManager, framework, gameGui, gameInteropProvider, keyState, gameNetwork, objectTable, targetManager, textureProvider, toastGui, contextMenu, titleScreenMenu)
    {
    }

    public override void PreBuild(IHostBuilder hostBuilder)
    {
        base.PreBuild(hostBuilder);
        
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
            container.RegisterType<MockSeTime>().As<ISeTime>().SingleInstance();
            container.RegisterType<MockWindowSystem>().As<IWindowSystem>().SingleInstance();
        });
        
        //Hosted service registrations
        hostBuilder.ConfigureContainer<ContainerBuilder>(container =>
        {
            container.RegisterType<MockWotsitIpc>().As<IWotsitIpc>().SingleInstance().ExternallyOwned();
            container.RegisterType<MockHostedCraftMonitor>().As<ICraftMonitor>().SingleInstance().ExternallyOwned();
            container.RegisterType<MockStartup>().SingleInstance().ExternallyOwned();
        });

        hostBuilder
            .ConfigureServices(collection =>
            {
                collection.AddHostedService(p => p.GetRequiredService<MockStartup>());
            });
    }
}