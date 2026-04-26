using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Plugin.Services;
using Lumina.Excel.Sheets;

namespace InventoryTools.Localizers;

public class CompanionLocalizer : ILocalizer<Companion>
{
    private readonly ISeStringEvaluator _seStringEvaluator;

    public CompanionLocalizer(ISeStringEvaluator seStringEvaluator)
    {
        _seStringEvaluator = seStringEvaluator;
    }

    public string Format(Companion instance)
    {
        return _seStringEvaluator.EvaluateObjStr(ObjectKind.Companion, instance.RowId);
    }
}