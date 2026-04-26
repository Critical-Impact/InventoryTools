using System.Collections.Generic;
using System.Linq;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using LuminaSupplemental.Excel.Model;

namespace InventoryTools.Compendium.Types.Extra;

public class RelicWeaponGroup
{
    private readonly RowRef<ClassJob> _classJob;
    private RelicWeaponCategory _relicWeaponCategory;
    private uint _rowId;
    private List<RelicWeapon> _relicWeapons;

    public RelicWeaponGroup(RowRef<ClassJob> classJob, RelicWeaponCategory relicWeaponCategory, uint rowId, List<RelicWeapon> relicWeapons)
    {
        _classJob = classJob;
        _relicWeaponCategory = relicWeaponCategory;
        _rowId = rowId;
        _relicWeapons = relicWeapons;
    }

    public RowRef<ClassJob> ClassJob
    {
        get => _classJob;
    }

    public RelicWeaponCategory WeaponCategory
    {
        get => _relicWeaponCategory;
    }

    public uint RowId
    {
        get => _rowId;
    }

    public List<RelicWeapon> RelicWeapons
    {
        get => _relicWeapons;
    }

    public List<RowRef<Quest>> Quests => RelicWeapons.Select(c => c.Quest).DistinctBy(c => c.RowId).ToList();
}