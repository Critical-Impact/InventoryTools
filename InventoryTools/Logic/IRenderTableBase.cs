using System.Collections.Generic;
using System.Numerics;
using InventoryTools.Logic.Columns;

namespace InventoryTools.Logic
{
    public interface IRenderTableBase
    {
        public void RefreshColumns();
        public List<IColumn> Columns { get; set; }

        public bool Draw(Vector2 size);

        public void Refresh(InventoryToolsConfiguration configuration);
        
        bool ShowFilterRow { get; set; }
        
        event RenderTableBase.PreFilterSortedItemsDelegate? PreFilterSortedItems;
        event RenderTableBase.PreFilterItemsDelegate? PreFilterItems;
        event RenderTableBase.ChangedDelegate? Refreshed;
    }
}