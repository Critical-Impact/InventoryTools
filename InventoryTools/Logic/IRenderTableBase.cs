using System.Collections.Generic;
using System.Numerics;
using CriticalCommonLib.Services.Mediator;


namespace InventoryTools.Logic
{
    public interface IRenderTableBase
    {
        public void RefreshColumns();
        public List<ColumnConfiguration> Columns { get; set; }

        public List<MessageBase> Draw(Vector2 size, bool shouldDraw = true);

        bool ShowFilterRow { get; set; }
        
    }
}