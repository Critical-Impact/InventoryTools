namespace InventoryToolsMock;

using Microsoft.Extensions.Hosting;

public class MockHostedCraftMonitor : MockCraftMonitor, IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}