using System.Numerics;
using Dalamud.Plugin.Services;
using InventoryTools.Logic;

namespace InventoryTools.Ui
{
    public interface IWindow
    {
        public string Key { get;  }
        public string GenericKey { get;  }
        public bool DestroyOnClose { get;}
        public bool SavePosition { get;}
        public Vector2 CurrentPosition { get; set; }
        
        public void Draw();
        public void Close();
        public void Open();
        public void Toggle();
        public void Invalidate();
        
        public FilterConfiguration? SelectedConfiguration { get;  }
        
        public IPluginLog PluginLog { get; }

        public delegate void ClosedDelegate(string windowKey);
        public delegate void OpenedDelegate(string windowKey);

        public event ClosedDelegate Closed;
        public event OpenedDelegate Opened;
    }
}