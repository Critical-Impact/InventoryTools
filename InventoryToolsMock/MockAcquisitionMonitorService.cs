using System.Threading;
using System.Threading.Tasks;
using AllaganLib.Monitors.Interfaces;
using InventoryTools.Services;

namespace InventoryToolsMock;

public class MockAcquisitionMonitorService : IAcquisitionMonitorService
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

    public event IAcquisitionMonitorService.ItemAcquiredDelegate? ItemAcquired;

    public IAcquisitionMonitorConfiguration Configuration { get; set; }

    public void CalculateItemCounts(bool notify = true)
    {
    }
}