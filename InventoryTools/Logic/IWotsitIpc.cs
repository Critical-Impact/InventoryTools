using Microsoft.Extensions.Hosting;

namespace InventoryTools.Logic;

public interface IWotsitIpc : IHostedService
{
    void InitForWotsit();
    void RegisterFilters();
    void WotsitInvoke(string guid);
    void Dispose();
}