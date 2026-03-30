using InventoryTools.Services;
using Lumina.Excel.Sheets;

namespace InventoryToolsMock;

public class MockUIStateService : IUIStateService
{
    public bool IsInstanceContentCompleted(InstanceContent row)
    {
        return row.RowId % 2 == 0;
    }

    public bool IsPublicContentCompleted(PublicContent row)
    {
        return row.RowId % 2 == 0;
    }
}