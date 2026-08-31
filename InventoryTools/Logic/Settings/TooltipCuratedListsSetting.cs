using InventoryTools.Logic.Settings.Abstract;
using InventoryTools.Logic.Settings.Abstract.Generic;
using InventoryTools.Services;
using InventoryTools.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Settings;

public class TooltipDisplayCuratedListsSetting : GenericBooleanSetting
{
    public TooltipDisplayCuratedListsSetting(ILogger<TooltipDisplayCuratedListsSetting> logger,
        ImGuiService imGuiService) : base(
        "TooltipDisplayCuratedLists",
        "Curated List Info",
        "When hovering an item, show which of your curated lists already contain it.",
        false,
        "15.0.12",
        logger,
        imGuiService)
    {
    }
}

public class TooltipCuratedListsSetting : MultipleListChoiceSetting
{
    public TooltipCuratedListsSetting(ILogger<TooltipCuratedListsSetting> logger, ImGuiService imGuiService,
        IListService listService) : base(logger, imGuiService, listService)
    {
    }

    public override FilterType? ListFilterType => FilterType.CuratedList;

    public override string Key { get; set; } = "TooltipCuratedLists";
    public override string Name { get; set; } = "Curated Lists to Check";

    public override string HelpText { get; set; } =
        "Which curated lists should be checked? If no lists are picked, every curated list is checked.";

    public override string Version => "15.0.12";
}

public class TooltipCuratedListsMatchQualitySetting : GenericBooleanSetting
{
    public TooltipCuratedListsMatchQualitySetting(ILogger<TooltipCuratedListsMatchQualitySetting> logger,
        ImGuiService imGuiService) : base(
        "TooltipCuratedListsMatchQuality",
        "Match Item Quality",
        "Should the quality of the hovered item match the quality stored in the curated list? When off, a NQ item will match a HQ entry and vice versa.",
        false,
        "15.0.12",
        logger,
        imGuiService)
    {
    }
}