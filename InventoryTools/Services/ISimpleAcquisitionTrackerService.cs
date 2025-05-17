using System;
using Microsoft.Extensions.Hosting;

namespace InventoryTools.Services;

public interface ISimpleAcquisitionTrackerService : IHostedService, IDisposable
{
    event SimpleAcquisitionTrackerService.ItemAcquiredDelegate? ItemAcquired;
    void CalculateItemCounts(bool notify = true);
}