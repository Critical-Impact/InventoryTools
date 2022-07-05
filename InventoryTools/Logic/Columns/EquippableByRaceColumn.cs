using System.Collections.Generic;
using CriticalCommonLib.Extensions;
using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Columns.Abstract;

namespace InventoryTools.Logic.Columns
{
    public class EquippableByRaceColumn : TextColumn
    {
        public override string? CurrentValue(InventoryItem item)
        {
            return CurrentValue(item.Item);
        }

        public override string? CurrentValue(ItemEx item)
        {
            return item.EquipRace.FormattedName();
        }

        public override string? CurrentValue(SortingResult item)
        {
            return CurrentValue(item.InventoryItem);
        }

        public override string Name { get; set; } = "Equippable By Race";
        public override float Width { get; set; } = 200;
        public override string FilterText { get; set; } = "";
        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Choice;

        public override List<string>? FilterChoices { get; set; } = new List<string>()
        {
            CharacterRace.Hyur.FormattedName(),
            CharacterRace.Elezen.FormattedName(),
            CharacterRace.Lalafell.FormattedName(),
            CharacterRace.Miqote.FormattedName(),
            CharacterRace.Roegadyn.FormattedName(),
            CharacterRace.Viera.FormattedName(),
            CharacterRace.AuRa.FormattedName(),
            CharacterRace.None.FormattedName(),
        };
    }
}