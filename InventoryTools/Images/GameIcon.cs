using System.Numerics;

namespace InventoryTools.Images
{
    public struct GameIcon
    {
        public string Name;
        public Vector2 Size;
        public Vector2? Uv0;
        public Vector2? Uv1;

        public static readonly GameIcon TickIcon = new GameIcon()
            {Name = "readycheck", Size = new Vector2(32, 32), Uv0 = new Vector2(0f, 0f), Uv1 = new Vector2(0.5f, 1f)};

        public static readonly GameIcon CrossIcon = new GameIcon()
            {Name = "readycheck", Size = new Vector2(32, 32), Uv0 = new Vector2(0.5f, 0f), Uv1 = new Vector2(1f, 1f)};

        public static readonly GameIcon CheckboxChecked = new GameIcon()
        {
            Name = "CheckBoxA_hr1", Size = new Vector2(16, 16), Uv0 = new Vector2(0.5f, 0f), Uv1 = new Vector2(1f, 1f)
        };

        public static readonly GameIcon CheckboxUnChecked = new GameIcon()
        {
            Name = "CheckBoxA_hr1", Size = new Vector2(16, 16), Uv0 = new Vector2(0f, 0f), Uv1 = new Vector2(0.5f, 1f)
        };
    }
}