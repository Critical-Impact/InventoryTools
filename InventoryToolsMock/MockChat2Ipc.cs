using System.Threading;
using System.Threading.Tasks;
using InventoryTools.IPC;

namespace InventoryToolsMock;

public class MockChat2Ipc : IChat2Ipc
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