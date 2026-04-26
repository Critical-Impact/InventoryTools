using System.Collections.Generic;
using System.Linq;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using LuminaSupplemental.Excel.Model;

namespace InventoryTools.Compendium.Types.Extra;

public class RelicToolGroup
{
    private readonly RowRef<ClassJob> _classJob;
    private RelicToolCategory _relicToolCategory;
    private uint _rowId;
    private List<RelicTool> _relicTools;

    public RelicToolGroup(RowRef<ClassJob> classJob, RelicToolCategory relicToolCategory, uint rowId, List<RelicTool> relicTools)
    {
        _classJob = classJob;
        _relicToolCategory = relicToolCategory;
        _rowId = rowId;
        _relicTools = relicTools;
    }

    public RowRef<ClassJob> ClassJob
    {
        get => _classJob;
    }

    public RelicToolCategory ToolCategory
    {
        get => _relicToolCategory;
    }

    public uint RowId
    {
        get => _rowId;
    }

    public List<RelicTool> RelicTools
    {
        get => _relicTools;
    }

    public List<RowRef<Quest>> Quests => RelicTools.Select(c => c.Quest).DistinctBy(c => c.RowId).ToList();
}