using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Plugin.Services;
using Lumina.Excel.Sheets;

namespace InventoryTools.Localizers;

public class ENpcBaseLocalizer : ILocalizer<ENpcBase>
{
    private readonly ISeStringEvaluator _seStringEvaluator;

    public ENpcBaseLocalizer(ISeStringEvaluator seStringEvaluator)
    {
        _seStringEvaluator = seStringEvaluator;
    }
    public string Format(ENpcBase instance)
    {
        return _seStringEvaluator.EvaluateObjStr(ObjectKind.EventNpc, instance.RowId);
    }
}