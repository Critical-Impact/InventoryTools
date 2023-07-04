using System.Numerics;
using ImGuiNET;
using InventoryTools.Logic;
// ReSharper disable VirtualMemberCallInConstructor

namespace InventoryTools.Ui
{
    public abstract class Window : Dalamud.Interface.Windowing.Window, IWindow
    {
        protected Window(string name, ImGuiWindowFlags flags = ImGuiWindowFlags.None, bool forceMainWindow = false) : base(name, flags, forceMainWindow)
        {
            if (MinSize != null && MaxSize != null)
            {
                SizeConstraints = new WindowSizeConstraints()
                {
                    MinimumSize = MinSize.Value,
                    MaximumSize = MaxSize.Value
                };
            }

            SizeCondition = ImGuiCond.FirstUseEver;
            if (DefaultSize != null)
            {
                Size = DefaultSize.Value;
            }
        }

        public override void OnOpen()
        {
            Opened?.Invoke(Key);
        }

        public override void OnClose()
        {
            Closed?.Invoke(Key);
        }

        public void Close()
        {
            IsOpen = false;
        }

        public void Open()
        {
            IsOpen = true;
        }

        public abstract void Invalidate();
        
        public abstract FilterConfiguration? SelectedConfiguration { get; }

        public event IWindow.ClosedDelegate? Closed;
        public event IWindow.OpenedDelegate? Opened;

        public abstract string Key { get; }

        public virtual string GenericKey => Key;

        public abstract bool DestroyOnClose { get; }
        public virtual bool SavePosition { get; }
        public virtual Vector2 CurrentPosition { get; set; }

        public abstract bool SaveState { get; }
        
        public abstract Vector2? DefaultSize { get; }
        public abstract Vector2? MaxSize { get; }
        public abstract Vector2? MinSize { get; }
    }
}