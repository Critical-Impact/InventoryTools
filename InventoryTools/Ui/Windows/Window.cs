using System.Numerics;
using ImGuiNET;
using InventoryTools.Logic;

namespace InventoryTools.Ui
{
    public abstract class Window : IWindow
    {
        private bool _visible;

        public void Close()
        {
            _visible = false;
            Closed?.Invoke(Key);
        }

        public void Open()
        {
            _visible = true;
            Opened?.Invoke(Key);
        }

        public void Toggle()
        {
            _visible = !Visible;
            if (!Visible)
            {
                Closed?.Invoke(Key);
            }
            else
            {
                Opened?.Invoke(Key);
            }
        }

        public abstract void Invalidate();
        
        public abstract FilterConfiguration? SelectedConfiguration { get; }

        public event IWindow.ClosedDelegate? Closed;
        public event IWindow.OpenedDelegate? Opened;

        public abstract string Name { get;  }
        public abstract string Key { get; }

        public bool Visible => _visible;

        public abstract bool DestroyOnClose { get; }
        public virtual ImGuiWindowFlags? WindowFlags { get; } = null;

        public abstract bool SaveState { get; }
        public abstract void Draw();
        
        public abstract Vector2 Size { get; }
        public abstract Vector2 MaxSize { get; }
        public abstract Vector2 MinSize { get; }
    }
}