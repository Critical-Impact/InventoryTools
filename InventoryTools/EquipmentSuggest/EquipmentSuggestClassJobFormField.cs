using System.Collections.Generic;
using System.Linq;
using AllaganLib.GameSheets.Sheets;
using AllaganLib.Interface.FormFields;
using AllaganLib.Shared.Extensions;
using Humanizer;
using InventoryTools.Services;

namespace InventoryTools.EquipmentSuggest;

public class EquipmentSuggestClassJobFormField : ChoiceFormField<uint, EquipmentSuggestConfig>
{
    private readonly Dictionary<uint, string> _choices;

    public EquipmentSuggestClassJobFormField(ImGuiService imGuiService, ClassJobSheet classJobSheet) : base(imGuiService)
    {
        _choices = classJobSheet.Where(c => c.RowId != 0).OrderBy(c => c.Base.Name.ToImGuiString())
            .ToDictionary(c => c.RowId, c => c.Base.Name.ToImGuiString().Humanize());
    }

    public override uint DefaultValue { get; set; } = 0;
    public override string Key { get; set; } = "ClassJob";
    public override string Name { get; set; } = "Job";
    public override string HelpText { get; set; } = "The job to recommend items for";
    public override string Version { get; } = "1.12.0.10";
    public override Dictionary<uint, string> Choices => _choices;
    public override bool Equal(uint item1, uint item2)
    {
        return item1.Equals(item2);
    }
}