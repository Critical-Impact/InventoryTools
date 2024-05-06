using System;
using Microsoft.Extensions.Hosting;

namespace InventoryTools.Logic;

public interface IWotsitIpc : IHostedService, IDisposable
{
    void InitForWotsit();
    void RegisterFilters();
    void WotsitInvoke(string guid);
}