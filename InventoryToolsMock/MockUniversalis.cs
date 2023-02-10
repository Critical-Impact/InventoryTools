using CriticalCommonLib.MarketBoard;
using CriticalCommonLib.Models;

namespace InventoryToolsMock;

public class MockUniversalis : IUniversalis
{
    public void Dispose()
    {
    }

    public event Universalis.ItemPriceRetrievedDelegate? ItemPriceRetrieved;

    public int QueuedCount
    {
        get
        {
            return 0;
        }
    }

    public int SaleHistoryLimit
    {
        get
        {
            return 10;
        }
    }

    public PricingResponse? RetrieveMarketBoardPrice(InventoryItem item)
    {
        return null;
    }

    public PricingResponse? RetrieveMarketBoardPrice(uint itemId)
    {
        return null;
    }

    public void SetSaleHistoryLimit(int limit)
    {
    }

    public void Initalise()
    {
    }

    public void QueuePriceCheck(uint itemId)
    {
    }

    public void RetrieveMarketBoardPrices(IEnumerable<uint> itemIds)
    {
    }
}