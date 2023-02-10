namespace InventoryTools.Logic;

public interface IWotsitIpc
{
    void InitForWotsit();
    void RegisterFilters();
    void WotsitInvoke(string guid);
    void Dispose();
}