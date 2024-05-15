using Autofac;
using CriticalCommonLib;
using CriticalCommonLib.Interfaces;
using CriticalCommonLib.Services;
using CriticalCommonLib.Time;
using DalaMock.Dalamud;
using DalaMock.Mock;
using DalaMock.Shared.Interfaces;
using Dalamud.Plugin.Services;
using InventoryTools;
using InventoryTools.Host;
using InventoryTools.Logic;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace InventoryToolsMock;

public class MockPluginLoader : PluginLoader
{
    private readonly MockProgram _mockProgram;
    private readonly MockPlugin _mockPlugin;
    private readonly MockService _mockService;
    private readonly ILogger _logger;

    public MockPluginLoader(MockProgram mockProgram, MockPlugin mockPlugin, MockService mockService, IPluginInterfaceService pluginInterfaceService, Service service, ILogger logger) : base(pluginInterfaceService, service)
    {
        _mockProgram = mockProgram;
        _mockPlugin = mockPlugin;
        _mockService = mockService;
        _logger = logger;
    }

    public override void PreBuild(HostBuilder hostBuilder)
    {
        //Replace real services with mocked versions as appropriate
        hostBuilder.ConfigureContainer<ContainerBuilder>(container =>
        {
            container.RegisterInstance(_logger).SingleInstance();
            container.RegisterInstance(_mockService).SingleInstance();
            container.RegisterType<MockWindow>().SingleInstance();
            container.RegisterType<IconBrowserWindow>().SingleInstance();
            container.RegisterInstance(_mockProgram.Framework).As<IFramework>().SingleInstance();
            container.RegisterType<MockFont>().As<IFont>().SingleInstance();
            container.RegisterType<MockCharacterMonitor>().As<ICharacterMonitor>().SingleInstance();
            container.RegisterType<MockPluginLog>().As<IPluginLog>().SingleInstance();
            container.RegisterType<MockTeleporterIpc>().As<ITeleporterIpc>().SingleInstance();
            container.RegisterType<MockChatUtilities>().As<IChatUtilities>().SingleInstance();
            container.RegisterType<MockGameInterface>().As<IGameInterface>().SingleInstance();
            container.RegisterType<MockTitleScreenMenu>().As<ITitleScreenMenu>().SingleInstance();
            container.RegisterType<MockSeTime>().As<ISeTime>().SingleInstance();
        });
        
        //Hosted service registrations
        hostBuilder.ConfigureContainer<ContainerBuilder>(container =>
        {
            container.RegisterType<MockWotsitIpc>().As<IWotsitIpc>().SingleInstance().ExternallyOwned();
        });
    }
}