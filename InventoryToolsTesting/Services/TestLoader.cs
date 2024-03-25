using Autofac;
using CriticalCommonLib;
using CriticalCommonLib.Interfaces;
using CriticalCommonLib.Services;
using DalaMock.Dalamud;
using DalaMock.Shared.Interfaces;
using Dalamud.Plugin.Services;
using InventoryTools;
using InventoryTools.Logic;
using InventoryToolsMock;
using Microsoft.Extensions.Hosting;
using Serilog;
using MockFont = InventoryToolsMock.MockFont;

namespace InventoryToolsTesting.Services
{
    public class TestPluginLoader : PluginLoader
    {
        private readonly IFramework _framework;
        private readonly ILogger _logger;

        public TestPluginLoader(IPluginInterfaceService pluginInterfaceService, Service service, IFramework framework, ILogger logger) : base(pluginInterfaceService, service)
        {
            _framework = framework;
            _logger = logger;
        }

        public override void PreBuild(HostBuilder hostBuilder)
        {
            base.PreBuild(hostBuilder);
            hostBuilder.ConfigureContainer<ContainerBuilder>(container =>
            {
                container.RegisterInstance(_logger).SingleInstance();
                container.RegisterType<MockWindow>().SingleInstance();
                container.RegisterInstance(_framework).As<IFramework>().SingleInstance();
                container.RegisterType<MockFont>().As<IFont>().SingleInstance();
                container.RegisterType<TestKeyState>().As<IKeyState>().SingleInstance();
                container.RegisterType<MockCharacterMonitor>().As<ICharacterMonitor>().SingleInstance();
                container.RegisterType<MockPluginLog>().As<IPluginLog>().SingleInstance();
                container.RegisterType<MockTeleporterIpc>().As<ITeleporterIpc>().SingleInstance();
                container.RegisterType<MockChatUtilities>().As<IChatUtilities>().SingleInstance();
                container.RegisterType<MockGameInterface>().As<IGameInterface>().SingleInstance();
                container.RegisterType<MockWotsitIpc>().As<IWotsitIpc>().SingleInstance();
                container.RegisterType<TestInventoryMonitor>().AsSelf().As<IInventoryMonitor>().SingleInstance();
            });

        }
    }
}