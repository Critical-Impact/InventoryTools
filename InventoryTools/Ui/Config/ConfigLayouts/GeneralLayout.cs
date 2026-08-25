using InventoryTools.Logic.Settings;
using InventoryTools.Ui.Config.Layouts;

namespace InventoryTools.Ui.Config.ConfigLayouts;

public class GeneralLayout : ConfigLayout
{
    public override PageLayout Build()
    {
        return Page("general", "General",
            Section("Saving",
                Paragraph("Allagan Tools keeps its own record of your inventories. These control how often that record is written to disk and how much of it survives a restart."),
                Setting<AutoSaveSetting>("Save automatically"),
                EnabledBy<AutoSaveSetting>(
                    Setting<AutoSaveTimeSetting>("How often")),
                Setting<PersistDataSetting>()),
            Section("Inventories", Setting<AllowCrossCharacterSetting>("Show other characters' inventories")),
            Section("Integrations",
                Paragraph("Ways to reach Allagan Tools from outside its own windows."),
                Setting<AddTitleMenuButtonSetting>("Add a button to the title menu"),
                Setting<CompendiumWotsitSetting>("List compendium windows in Wotsit"))
        );
    }
}