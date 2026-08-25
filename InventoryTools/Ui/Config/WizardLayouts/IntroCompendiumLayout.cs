using InventoryTools.Compendium.Windows;
using InventoryTools.Ui.Config.Layouts;

namespace InventoryTools.Ui.Config.WizardLayouts;

public class IntroCompendiumLayout : ContentLayout
{
    public override PageLayout Build()
    {
        return Page("intro/compendium", "Compendium",
            Paragraph("The compendium is a browsable reference for the game's content, built from the same data the plugin uses everywhere else. If you want to know what something is, where it comes from or what drops it, this is where to look."),
            Section("What's in it",
                Bullet("Items, quests and achievements."),
                Bullet("Mounts, minions and glamour sets."),
                Bullet("Duties, leves, beast tribes and custom deliveries."),
                Bullet("Relic weapons and tools, master recipe books, folklore tomes and soul crystals."),
                Bullet("NPCs, territories, and airship and submarine routes."),
                Bullet("Classes, gearsets, chocobo items and more besides.")),
            Section("Have a look",
                Paragraph("Each entry links through to everything related to it, so you can follow a chain from an item to the duty that drops it to the NPC that sells the rest."),
                OpenWindow<CompendiumTypesWindow>("Open the compendium"),
                Paragraph("It also lives under the Compendium menu in the items and crafts windows, or the /compendium command."))
        );
    }
}
