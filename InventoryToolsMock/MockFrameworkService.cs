using CriticalCommonLib.Services;
using InventoryTools.Services;

namespace InventoryToolsMock;

public class MockFrameworkService : IFrameworkService
{
    public void Dispose()
    {
    }

    public event IFrameworkService.OnUpdateDelegate? Update;
    public Task RunOnFrameworkThread(Action action)
    {
        var runOnFrameworkThread = new Task(action);
        runOnFrameworkThread.RunSynchronously();
        return runOnFrameworkThread;
    }

    public Task RunOnTick(Action action, TimeSpan delay = default(TimeSpan), int delayTicks = 0,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        var runOnTick = new Task(action).WaitAsync(delay, cancellationToken);
        runOnTick.RunSynchronously();
        return runOnTick;
    }
}