using System;
using System.Collections.Generic;
using CriticalCommonLib.Services.Mediator;
using DalaMock.Host.Mediator;
using InventoryTools.Ui.Config;
using InventoryTools.Ui.Pages;


namespace InventoryTools.Logic
{
    public interface IConfigPage
    {
        public void Initialize();

        public string Key { get; }
        public string Name { get; }
        public List<MessageBase>? Draw();
        public bool IsMenuItem { get; }

        public IEnumerable<IConfigPage>? ChildPages { get; set; }

        public bool DrawBorder { get; }

        public IEnumerable<ConfigSearchEntry> GetSearchEntries()
        {
            return [];
        }
    }
}