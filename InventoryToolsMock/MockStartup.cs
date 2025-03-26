using System.Threading;
using System.Threading.Tasks;
using InventoryTools.Services;
using Microsoft.Extensions.Hosting;

namespace InventoryToolsMock;

public class MockStartup : IHostedService
{
    private readonly WindowService windowService;

    public MockStartup(WindowService windowService)
    {
        this.windowService = windowService;
    }
    
    public Task StartAsync(CancellationToken cancellationToken)
    {
        windowService.OpenWindow<MockWindow>();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}