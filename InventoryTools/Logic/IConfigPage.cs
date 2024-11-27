using System.Collections.Generic;
using CriticalCommonLib.Services.Mediator;


namespace InventoryTools.Logic
{
    public interface IConfigPage
    {
        public void Initialize();
        public string Name { get; }
        public List<MessageBase>? Draw();
        public bool IsMenuItem { get; }
        
        public bool DrawBorder { get; }
    }
}