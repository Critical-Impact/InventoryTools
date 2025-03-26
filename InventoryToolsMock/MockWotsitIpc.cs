using System.Threading;
using System.Threading.Tasks;
using InventoryTools.Logic;

namespace InventoryToolsMock;

public class MockWotsitIpc : IWotsitIpc
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public void InitForWotsit()
    {
    }

    public void RegisterFilters()
    {
    }

    public void WotsitInvoke(string guid)
    {
    }

    public void Dispose()
    {
    }
}