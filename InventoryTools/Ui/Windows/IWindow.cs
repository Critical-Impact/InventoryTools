using System.Numerics;
using ImGuiNET;
using InventoryTools.Logic;

namespace InventoryTools.Ui
{
    public interface IWindow
    {
        public string Name { get; }
        public string Key { get;  }
        public Vector2 Size { get; }
        public Vector2 MaxSize { get; }
        public Vector2 MinSize { get; }
        public bool Visible { get;  }
        public bool DestroyOnClose { get;}
        
        public ImGuiWindowFlags? WindowFlags { get;}
        public void Draw();
        public void Close();
        public void Open();
        public void Toggle();
        public void Invalidate();
        
        public FilterConfiguration? SelectedConfiguration { get;  }

        public delegate void ClosedDelegate(string windowKey);
        public delegate void OpenedDelegate(string windowKey);

        public event ClosedDelegate Closed;
        public event OpenedDelegate Opened;
    }
}