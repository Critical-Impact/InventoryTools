using System.Collections.Generic;
using CriticalCommonLib.Enums;
using CriticalCommonLib.Extensions;
using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns
{
    public class EquippableByGenderColumn : TextColumn
    {
        public EquippableByGenderColumn(ILogger<EquippableByGenderColumn> logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }
        public override ColumnCategory ColumnCategory => ColumnCategory.Basic;

        public override string? CurrentValue(ColumnConfiguration columnConfiguration, SearchResult searchResult)
        {
            return searchResult.Item.EquippableByGender.FormattedName();
        }

        public override string Name { get; set; } = "Equipped By (Gender)";
        public override float Width { get; set; } = 200;
        public override string HelpText { get; set; } = "Shows if an item can be equipped by a specific gender.";
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