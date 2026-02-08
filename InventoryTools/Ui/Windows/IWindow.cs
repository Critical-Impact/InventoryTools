using System;
using System.Numerics;
using InventoryTools.Logic;

namespace InventoryTools.Ui
{
    public interface IWindow : IDisposable
    {
        public string Key { get;  }
        public string GenericKey { get;  }
        public string GenericName { get;  }
        public bool DestroyOnClose { get;}
        public bool SavePosition { get;}
        public bool SaveState { get;}
        public Vector2 CurrentPosition { get; set; }

        public bool RespectCloseHotkey { get; set; }
        public bool IsOpen { get; set; }

        public void DrawWindow();
        public void Close();
        public void Open();
        public void Toggle();
        public void Invalidate();

        public void SetPosition(Vector2 newPosition, bool isAppearing);

        public FilterConfiguration? SelectedConfiguration { get;  }

        public delegate void ClosedDelegate(IWindow window);
        public delegate void OpenedDelegate(IWindow window);

        public event ClosedDelegate Closed;
        public event OpenedDelegate Opened;
    }
}