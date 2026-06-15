using System.Collections.Generic;
using AllaganLib.GameSheets.Sheets.Rows;
using CriticalCommonLib.Models;
using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters;

public enum CraftReportPrefix
{
    Blank = 0,
    PluginName = 1,
    CraftListName = 2
}

public class CraftReportPrefixFilter : ChoiceFilter<CraftReportPrefix>
{
    public override CraftReportPrefix CurrentValue(FilterConfiguration configuration)
    {
        return (CraftReportPrefix)(configuration.GetUintFilter(Key) ?? (uint)DefaultValue);
    }
    
    public override void ResetFilter(FilterConfiguration configuration)
    {
        configuration.UpdateUintFilter(Key, (uint)DefaultValue);
    }
    
    public override void UpdateFilterConfiguration(FilterConfiguration configuration, CraftReportPrefix newValue)
    {
        configuration.UpdateUintFilter(Key, (uint)newValue);
    }
    
    public override string Key { get; set; } = "CraftReportPrefix";
    public override string Name { get; set; } = "Notification Prefix";
    
    public override string HelpText { get; set; } =
        "What to prefix each acquisition progress message with. 'Plugin Name' uses [AT], 'Craft List Name' uses the name of this craft list.";
    
    public override FilterCategory FilterCategory { get; set; } = FilterCategory.Notifications;
    public override CraftReportPrefix DefaultValue { get; set; } = CraftReportPrefix.Blank;
    public override FilterType AvailableIn { get; set; } = FilterType.CraftFilter;
    
    public override bool? FilterItem(FilterConfiguration configuration, InventoryItem item)
    {
        return null;
    }
    
    public override bool? FilterItem(FilterConfiguration configuration, ItemRow item)
    {
        return null;
    }
    
    public override List<CraftReportPrefix> GetChoices(FilterConfiguration configuration)
    {
        return
        [
            CraftReportPrefix.Blank,
            CraftReportPrefix.PluginName,
            CraftReportPrefix.CraftListName
        ];
    }
    
    public override string GetFormattedChoice(FilterConfiguration filterConfiguration, CraftReportPrefix choice)
    {
        return choice switch
        {
            CraftReportPrefix.Blank => "None",
            CraftReportPrefix.PluginName => "Plugin Name",
            CraftReportPrefix.CraftListName => "Craft List Name",
            _ => choice.ToString()
        };
    }
    
    public CraftReportPrefixFilter(ILogger<CraftReportPrefixFilter> logger, ImGuiService imGuiService) : base(logger,
        imGuiService)
    {
    }
}