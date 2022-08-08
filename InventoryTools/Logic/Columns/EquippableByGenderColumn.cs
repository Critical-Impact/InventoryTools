using System.Collections.Generic;
using CriticalCommonLib.Extensions;
using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Columns.Abstract;

namespace InventoryTools.Logic.Columns
{
    public class EquippableByGenderColumn : TextColumn
    {
        public override string? CurrentValue(InventoryItem item)
        {
            return item.Item.EquippableByGender.FormattedName();
        }

        public override string? CurrentValue(ItemEx item)
        {
            return item.EquippableByGender.FormattedName();
        }

        public override string? CurrentValue(SortingResult item)
        {
            return CurrentValue(item.InventoryItem);
        }

        public override string Name { get; set; } = "Equippable By Gender";
        public override float Width { get; set; } = 200;
        public override string HelpText { get; set; } = "Shows if an item can be equipped by a specific gender.";
        public override string FilterText { get; set; } = "";
        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Choice;

        public override List<string>? FilterChoices { get; set; } = new List<string>()
        {
            CharacterSex.Both.FormattedName(),
            CharacterSex.Male.FormattedName(),
            CharacterSex.Female.FormattedName(),
            CharacterSex.NotApplicable.FormattedName(),
        };
    }
}