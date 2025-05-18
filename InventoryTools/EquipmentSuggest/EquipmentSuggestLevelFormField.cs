using System;
using System.Collections.Generic;
using System.Linq;
using AllaganLib.Interface.FormFields;
using InventoryTools.Services;

namespace InventoryTools.EquipmentSuggest;

public class EquipmentSuggestLevelFormField : IntegerFormField<EquipmentSuggestConfig>
{
    public EquipmentSuggestLevelFormField(ImGuiService imGuiService) : base(imGuiService)
    {
        this.MinValue = 1;
        this.MaxValue = 100;
    }

    public override int DefaultValue { get; set; } = 1;
    public override string Key { get; set; } = "Level";
    public override string Name { get; set; } = "Level";
    public override string HelpText { get; set; } = "The start level of items";
    public override string Version { get; } = "1.12.0.10";

    public int GetCenteredValue(EquipmentSuggestConfig config, int index)
    {
        int min = 1;
        int max = 100;
        int rangeSize = 5;
        var start = CurrentValue(config);
        int half = rangeSize / 2;

        int lower = start - half;
        int upper = start + half;

        if (lower < min)
        {
            upper += (min - lower);
            lower = min;
        }
        if (upper > max)
        {
            lower -= (upper - max);
            upper = max;
        }

        lower = Math.Max(lower, min);
        upper = Math.Min(upper, max);

        int clampedIndex = Math.Clamp(index, 0, upper - lower);
        return lower + clampedIndex;
    }

    public IEnumerable<int> GetCenteredRange(EquipmentSuggestConfig config)
    {
        int min = 1;
        int max = 100;
        int rangeSize = 5;
        var start = CurrentValue(config);
        int half = rangeSize / 2;

        int lower = start - half;
        int upper = start + half;

        if (lower < min)
        {
            upper += (min - lower);
            lower = min;
        }
        if (upper > max)
        {
            lower -= (upper - max);
            upper = max;
        }

        lower = Math.Max(lower, min);
        upper = Math.Min(upper, max);

        return Enumerable.Range(lower, upper - lower + 1);
    }
}