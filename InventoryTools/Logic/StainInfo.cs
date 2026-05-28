using System.Numerics;

namespace InventoryTools.Logic;

public readonly record struct StainInfo(uint RowId, string Name, byte R, byte G, byte B, byte Shade)
{
    public Vector4 AsVec4 => new(R / 255f, G / 255f, B / 255f, 1f);
}
