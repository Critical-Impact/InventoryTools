using System;
using System.Collections.Generic;
using System.Linq;
using Lumina.Excel;
using Lumina.Excel.Sheets;

namespace InventoryTools.Logic;

/// <summary>
/// Information provided by https://ffxiv.pf-n.co/chocobo-color/about
/// </summary>
public class ChocoboColourSolver
{
    private readonly ExcelSheet<Stain> _stainSheet;

    public sealed record ChocoboFruit(uint ItemId, string Name, int DR, int DG, int DB);

    public readonly IReadOnlyList<ChocoboFruit> Fruits = new ChocoboFruit[]
    {
        new(8157, "Xelphatol Apple",       +5, -5, -5),
        new(8158, "Doman Plum",            -5, +5, +5),
        new(8159, "Mamook Pear",           -5, +5, -5),
        new(8160, "Valfruit",              +5, -5, +5),
        new(8161, "O'Ghomoro Berries",     -5, -5, +5),
        new(8162, "Cieldalaes Pineapple",  +5, +5, -5),
    };

    public const uint DefaultStainId = 36;

    public ChocoboColourSolver(ExcelSheet<Stain> stainSheet)
    {
        _stainSheet = stainSheet;
    }

    public List<ChocoboFruit> Solve(
        (byte R, byte G, byte B) current,
        (byte R, byte G, byte B) target,
        int lookahead = 3)
    {
        var result = new List<ChocoboFruit>();
        const int maxSteps = 300;

        while (result.Count < maxSteps)
        {
            var best = FindBestFirstFruit(current, target, lookahead);
            if (best == null) { break; }

            current = Apply(current, best);
            result.Add(best);
        }

        return result;
    }

    internal ChocoboFruit? FindBestFirstFruit(
        (byte R, byte G, byte B) current,
        (byte R, byte G, byte B) target,
        int depth)
    {
        const double eps = 1e-9;

        double bestDist = EuclideanDistance(current, target);
        ChocoboFruit? bestFruit = null;
        int bestLen = 0;

        void Recurse((byte R, byte G, byte B) pos, int remaining, ChocoboFruit first, int len)
        {
            double d = EuclideanDistance(pos, target);
            if (d < bestDist - eps || (Math.Abs(d - bestDist) < eps && bestFruit != null && len < bestLen))
            {
                bestDist = d;
                bestFruit = first;
                bestLen = len;
            }
            if (remaining == 0) { return; }
            foreach (var fruit in Fruits)
            {
                Recurse(Apply(pos, fruit), remaining - 1, first, len + 1);
            }
        }

        foreach (var fruit in Fruits)
        {
            Recurse(Apply(current, fruit), depth - 1, fruit, 1);
        }

        return bestFruit;
    }

    public (byte R, byte G, byte B) Apply(
        (byte R, byte G, byte B) color,
        ChocoboFruit fruit)
    {
        return (
            (byte)Math.Clamp(color.R + fruit.DR, 0, 255),
            (byte)Math.Clamp(color.G + fruit.DG, 0, 255),
            (byte)Math.Clamp(color.B + fruit.DB, 0, 255)
        );
    }

    public double EuclideanDistance(
        (byte R, byte G, byte B) a,
        (byte R, byte G, byte B) b)
    {
        int dr = a.R - b.R;
        int dg = a.G - b.G;
        int db = a.B - b.B;
        return Math.Sqrt(dr * dr + dg * dg + db * db);
    }

    public (byte R, byte G, byte B) DecodeStainColor(uint color) =>
        ((byte)((color >> 16) & 0xFF),
         (byte)((color >> 8) & 0xFF),
         (byte)(color & 0xFF));

    public List<StainInfo> GetStains()
    {
        return _stainSheet
            .Where(c => c.RowId != 0 && c.RowId < 86)
            .Select(ToStainInfo)
            .OrderBy(s => s.Shade)
            .ThenBy(s => s.RowId)
            .ToList();
    }

    public StainInfo? GetStain(uint rowId)
    {
        var row = _stainSheet.GetRowOrDefault(rowId);
        return row.HasValue ? ToStainInfo(row.Value) : null;
    }

    public StainInfo? GetDefaultStain()
    {
        return GetStain(DefaultStainId);
    }

    private StainInfo ToStainInfo(Stain stain)
    {
        var (r, g, b) = DecodeStainColor(stain.Color);
        return new StainInfo(stain.RowId, stain.Name.ExtractText(), r, g, b, stain.Shade);
    }
}
