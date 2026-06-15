using System.Collections.Generic;
using System.Linq;
using InventoryTools.Logic.Filters;
using InventoryTools.Logic.Settings.Abstract;
using InventoryTools.Logic.WizardSettings;
using InventoryTools.Services.Interfaces;

namespace InventoryTools.Logic.Features;

public class CraftNotificationsFeature : Feature
{
    private readonly CraftReportCompletionOnlyFilter _completionOnlyFilter;
    private readonly CraftNotificationsCompletionOnlySetting _completionOnlySetting;
    private readonly InventoryToolsConfiguration _configuration;
    private readonly IListService _listService;
    private readonly CraftReportPlaySoundFilter _playSoundFilter;
    private readonly CraftNotificationsPlaySoundSetting _playSoundSetting;
    private readonly CraftReportProgressFilter _progressFilter;
    private readonly CraftNotificationsReportToChatSetting _reportToChatSetting;

    public CraftNotificationsFeature(IEnumerable<ISetting> settings,
        CraftNotificationsReportToChatSetting reportToChatSetting,
        CraftNotificationsPlaySoundSetting playSoundSetting,
        CraftNotificationsCompletionOnlySetting completionOnlySetting,
        CraftReportProgressFilter progressFilter,
        CraftReportPlaySoundFilter playSoundFilter,
        CraftReportCompletionOnlyFilter completionOnlyFilter,
        IListService listService,
        InventoryToolsConfiguration configuration) : base([
            typeof(CraftNotificationsReportToChatSetting),
            typeof(CraftNotificationsPlaySoundSetting),
            typeof(CraftNotificationsCompletionOnlySetting)
        ],
        settings)
    {
        _reportToChatSetting = reportToChatSetting;
        _playSoundSetting = playSoundSetting;
        _completionOnlySetting = completionOnlySetting;
        _progressFilter = progressFilter;
        _playSoundFilter = playSoundFilter;
        _completionOnlyFilter = completionOnlyFilter;
        _listService = listService;
        _configuration = configuration;
    }

    public override string Name => "Craft Notifications";

    public override string Description =>
        "Configure how Allagan Tools notifies you as you acquire items for your craft lists. The default craft list and any of your existing craft lists will receive these notification settings. Notifications only occur while a craft list is active. This feature can be further configured in craft lists by editing the settings of your individual lists or the default craft list.";

    public override void OnFinish()
    {
        var reportToChat = _reportToChatSetting.CurrentValue(_configuration);
        var playSound = _playSoundSetting.CurrentValue(_configuration);
        var completionOnly = _completionOnlySetting.CurrentValue(_configuration);

        var lists = _listService.Lists
            .Where(c => c.FilterType == FilterType.CraftFilter)
            .ToList();
        var defaultList = _listService.GetDefaultCraftList();
        if (!lists.Contains(defaultList)) lists.Add(defaultList);

        foreach (var list in lists)
        {
            _progressFilter.UpdateFilterConfiguration(list, reportToChat);
            _playSoundFilter.UpdateFilterConfiguration(list, playSound);
            _completionOnlyFilter.UpdateFilterConfiguration(list, completionOnly);
        }
    }
}