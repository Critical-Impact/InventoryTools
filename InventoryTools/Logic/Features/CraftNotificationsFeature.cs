using System.Collections.Generic;
using System.Linq;
using InventoryTools.Logic.Filters;
using InventoryTools.Logic.Settings.Abstract;
using InventoryTools.Logic.WizardSettings;
using InventoryTools.Services.Interfaces;
using InventoryTools.Ui.Config;
using InventoryTools.Ui.Config.Layouts;

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
        InventoryToolsConfiguration configuration) : base(settings)
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

    public override PageLayout Build()
    {
        return Page("feature/craft-notifications", "Craft Notifications",
            Paragraph("The plugin can tell you about your progress as you collect the items for a craft list. These notifications occur only while a craft list is active."),
            Setting<CraftNotificationsReportToChatSetting>("Report progress to the chat window"),
            Setting<CraftNotificationsPlaySoundSetting>("Play a sound when an item is complete"),
            Setting<CraftNotificationsCompletionOnlySetting>("Report only completed items, not each item that you collect"),
            Paragraph("When you complete the wizard, the plugin applies these settings to your craft lists and to the default craft list. Each list then keeps its own copy, which you can change in the settings for that list.")
        );
    }

    public override void OnFinish()
    {
        var reportToChat = _reportToChatSetting.CurrentValue(_configuration);
        var playSound = _playSoundSetting.CurrentValue(_configuration);
        var completionOnly = _completionOnlySetting.CurrentValue(_configuration);

        var lists = _listService.Lists
            .Where(c => c.FilterType == FilterType.CraftFilter)
            .ToList();
        var defaultList = _listService.GetDefaultCraftList();
        if (!lists.Contains(defaultList))
        {
            lists.Add(defaultList);
        }

        foreach (var list in lists)
        {
            _progressFilter.UpdateFilterConfiguration(list, reportToChat);
            _playSoundFilter.UpdateFilterConfiguration(list, playSound);
            _completionOnlyFilter.UpdateFilterConfiguration(list, completionOnly);
        }
    }
}