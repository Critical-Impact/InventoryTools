using System.Threading;
using CriticalCommonLib.Services.Mediator;
using InventoryToolsTesting.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace InventoryToolsTesting.Tests.Abstract
{
    public class BaseTest : IMediatorSubscriber
    {
        public BaseTest()
        {
            TestHost = new TestBoot().CreateHost();
            MediatorService = TestHost.Services.GetRequiredService<MediatorService>();
            MediatorService.StartAsync(new CancellationToken());
        }

        public IHost TestHost { get; set; }
        public MediatorService MediatorService { get; }
    }
}