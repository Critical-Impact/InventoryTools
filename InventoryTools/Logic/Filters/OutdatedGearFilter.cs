using System.Collections.Generic;
using System.Linq;
using AllaganLib.GameSheets.Sheets.Rows;
using CriticalCommonLib.Models;
using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;

using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters;

public class OutdatedGearFilter : BooleanFilter
{
    private readonly IGameInteropService _gameInteropService;
    private string _key;
    private string _name;
    private string _helpText;
    private FilterCategory _filterCategory;
    private FilterType _availableIn;
    private Dictionary<uint, short>? _jobClassLevels = null;

    public OutdatedGearFilter(ILogger<OutdatedGearFilter> logger, ImGuiService imGuiService, IGameInteropService gameInteropService) : base(logger, imGuiService)
    {
        _gameInteropService = gameInteropService;
    }


    public override string Key { get; set; } = "OutdatedGearFilter";
    public override string Name { get; set; } = "Outdated Gear?";
    public override string HelpText { get; set; } = "Will show any gear considered to be outdated. This will compare the item level of each item with the level of your classes. It will use the lowest level you have applicable to the weapon to determine if it's outdated. Any classes you do not have are not taken into consideration.";
    public override FilterCategory FilterCategory { get; set; } = FilterCategory.Basic;

    public override FilterType AvailableIn { get; set; } = FilterType.SearchFilter | FilterType.SortingFilter | FilterType.GameItemFilter | FilterType.HistoryFilter;

    public override bool? FilterItem(FilterConfiguration configuration, InventoryItem item)
    {
        return FilterItem(configuration, item.Item);
    }

    public override bool? FilterItem(FilterConfiguration configuration, ItemRow item)
    {
        var currentValue = this.CurrentValue(configuration);
        if (currentValue == null)
        {
            return null;
        }

        var isOutdated = false;
        var jobClassLevels = GetClassJobLevels();

        if (item.ClassJobCategory != null)
        {
            int? lowestJobLevel = null;

            foreach (var job in item.ClassJobCategory.ClassJobs)
            {
                if (jobClassLevels.TryGetValue(job.RowId, out var jobLevel))
                {
                    if (lowestJobLevel == null || lowestJobLevel > jobLevel)
                    {
                        lowestJobLevel = jobLevel;
                    }
                }
            }

            if (lowestJobLevel != null && lowestJobLevel > item.Base.LevelEquip)
            {
                isOutdated = true;
            }
        }
        else
        {
            return false;
        }

        switch (currentValue)
        {
            case true when isOutdated:
            case false when !isOutdated:
                return true;
            default:
                return false;
        }
    }


    private Dictionary<uint, short> GetClassJobLevels()
    {
        if (_jobClassLevels == null)
        {
            _jobClassLevels = _gameInteropService.GetClassJobLevels()?.ToDictionary(c => c.Key.RowId, c=> c.Value) ?? new();
        }

        return _jobClassLevels;
    }

    public override void InvalidateSearchCache()
    {
        _jobClassLevels = null;
    }
}