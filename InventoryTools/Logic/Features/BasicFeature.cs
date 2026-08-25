using System.Collections.Generic;
using InventoryTools.Logic.Settings;
using InventoryTools.Logic.Settings.Abstract;
using InventoryTools.Ui.Config;
using InventoryTools.Ui.Config.Layouts;

namespace InventoryTools.Logic.Features;

public class BasicFeature : Feature
{
    public BasicFeature(IEnumerable<ISetting> settings) : base(settings)
    {
    }

    public override PageLayout Build()
    {
        return Page("feature/basic", "Basics",
            Paragraph("These settings apply to the whole plugin. You can change all of them later in the settings window."),
            Setting<AutoSaveSetting>("Save inventories automatically"),
            Setting<AllowCrossCharacterSetting>("Show the inventories of your other characters"),
            Setting<HistoryEnabledSetting>("Record the changes to your inventory"),
            Setting<AddTitleMenuButtonSetting>("Add a button to the game's title menu")
        );
    }
}