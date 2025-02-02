using System.Collections.Generic;
using CriticalCommonLib.Services.Mediator;
using InventoryTools.Ui.Pages;


namespace InventoryTools.Logic
{
    public interface IConfigPage
    {
        public void Initialize();
        public string Name { get; }
        public List<MessageBase>? Draw();
        public bool IsMenuItem { get; }

        public IEnumerable<Page>? ChildPages { get; set; }

        public bool DrawBorder { get; }
    }
}