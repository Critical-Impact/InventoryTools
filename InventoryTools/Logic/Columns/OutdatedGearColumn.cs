using System.Collections.Generic;
using System.Linq;
using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns;

public class OutdatedGearColumn : CheckboxColumn
{
    private readonly IGameInteropService _gameInteropService;
    private Dictionary<uint, short>? _jobClassLevels = null;

    public OutdatedGearColumn(ILogger<OutdatedGearColumn> logger, ImGuiService imGuiService, IGameInteropService gameInteropService) : base(logger, imGuiService)
    {
        _gameInteropService = gameInteropService;
    }

    public override string Name { get; set; } = "Outdated Gear?";
    public override float Width { get; set; } = 50;
    public override string HelpText { get; set; } = "Will show any gear considered to be outdated. This will compare the item level of each item with the level of your classes. It will use the lowest level you have applicable to the weapon to determine if it's outdated. Any classes you do not have are not taken into consideration.";
    public override ColumnCategory ColumnCategory { get; } = ColumnCategory.Tools;
    public override bool HasFilter { get; set; } = true;
    public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Boolean;
    
    private Dictionary<uint, short> GetClassJobLevels()
    {
        if (_jobClassLevels == null)
        {
            _jobClassLevels = _gameInteropService.GetClassJobLevels()?.ToDictionary(c => c.Key.RowId, c=> c.Value) ?? new();
        }

        return _jobClassLevels;
    }


    public override bool? CurrentValue(ColumnConfiguration columnConfiguration, InventoryItem item)
    {
        return CurrentValue(columnConfiguration, item.Item);
    }

    public override bool? CurrentValue(ColumnConfiguration columnConfiguration, ItemEx item)
    {
        var isOutdated = false;
        var jobClassLevels = GetClassJobLevels();
        
        if (item.ClassJobCategoryEx.Row != 0 && item.ClassJobCategoryEx.Value != null)
        {
            int? lowestJobLevel = null;
            
            foreach (var job in item.ClassJobCategoryEx.Value.ApplicableClasses)
            {
                if (jobClassLevels.TryGetValue(job.Value.Row, out var jobLevel))
                {
                    if (lowestJobLevel == null || lowestJobLevel > jobLevel)
                    {
                        lowestJobLevel = jobLevel;
                    }
                }
            }

            if (lowestJobLevel != null && lowestJobLevel > item.LevelEquip)
            {
                isOutdated = true;
            }
        }
        else
        {
            return false;
        }

        return isOutdated;
    }

    public override bool? CurrentValue(ColumnConfiguration columnConfiguration, SortingResult item)
    {
        return CurrentValue(columnConfiguration, item.Item);
    }

    public override void InvalidateSearchCache()
    {
        this._jobClassLevels = null;
    }
}