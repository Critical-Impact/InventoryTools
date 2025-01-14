using InventoryTools.Logic.Settings.Abstract;
using InventoryTools.Logic.Settings.Abstract.Generic;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Settings;

public class ContextMenuAddToCuratedListSetting : GenericBooleanSetting
{
    public ContextMenuAddToCuratedListSetting(ILogger<CraftOverlayRememberStateSetting> logger,
        ImGuiService imGuiService) : base("AddToCuratedListContextMenu",
        "Context Menu - Add to Curated List",
        "Add a submenu to add the item to a curated list?",
        false,
        SettingCategory.ContextMenu,
        SettingSubCategory.General,
        "1.7.0.21",
        logger,
        imGuiService)
    {
    }

    public override string WizardName { get; } = "Add to Curated List";

}