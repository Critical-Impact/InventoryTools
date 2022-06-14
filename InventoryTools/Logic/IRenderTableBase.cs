using System.Collections.Generic;
using InventoryTools.Logic.Columns;

namespace InventoryTools.Logic
{
    public interface IRenderTableBase
    {
        public void RefreshColumns();
        public List<IColumn> Columns { get; set; }

        public void Draw();

        public void Refresh(InventoryToolsConfiguration configuration);
        
        bool ShowFilterRow { get; set; }
        
        event RenderTableBase.PreFilterSortedItemsDelegate? PreFilterSortedItems;
        event RenderTableBase.PreFilterItemsDelegate? PreFilterItems;
        event RenderTableBase.ChangedDelegate? Refreshed;
    }
}