using System.Threading;
using Autofac;
using CriticalCommonLib.Services.Mediator;
using Dalamud.Plugin.Services;
using InventoryToolsTesting.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace InventoryToolsTesting.Tests.Abstract
{
    public class BaseTest : IMediatorSubscriber
    {
        public BaseTest()
        {
            Host = new TestBoot().CreateHost();
            MediatorService = Host.Services.GetRequiredService<MediatorService>();
            MediatorService.StartAsync(new CancellationToken());
            PluginLog = Host.Services.GetRequiredService<IPluginLog>();
        }

        public IPluginLog PluginLog { get; set; }
        public IHost Host { get; set; }
        public MediatorService MediatorService { get; }
    }
}