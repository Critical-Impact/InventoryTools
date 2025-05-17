using System.Threading;
using System.Threading.Tasks;
using InventoryTools.Services;

namespace InventoryToolsMock;

public class MockAcquisitionTrackerService : ISimpleAcquisitionTrackerService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
       return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public void Dispose()
    {
    }

    public event SimpleAcquisitionTrackerService.ItemAcquiredDelegate? ItemAcquired;
    public void CalculateItemCounts(bool notify = true)
    {
    }
}