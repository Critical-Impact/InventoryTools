using CriticalCommonLib.Models;
using CriticalCommonLib.Services;

namespace InventoryToolsMock;

public class MockOdrScanner : IOdrScanner
{
    public event OdrScanner.SortOrderChangedDelegate? OnSortOrderChanged;
    public InventorySortOrder? GetSortOrder(ulong characterId)
    {
        return null;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}