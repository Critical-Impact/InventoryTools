using InventoryTools.Ui.Config.Layouts;

namespace InventoryTools.Ui.Config.WizardLayouts;

public class IntroInventoriesLayout : ContentLayout
{
    public override PageLayout Build()
    {
        return Page("intro/inventories", "Your items",
            Paragraph(
                "The plugin can only see an inventory after the game has shown it to you at least once. That is a limit of how the game sends its data, not a setting you can turn on."),
            Section("If something is missing",
                Paragraph("Open it in game once and it will be remembered from then on. Most commonly:"),
                Bullet("Retainers: talk to a summoning bell and open each retainer's inventory."),
                Bullet("Free company chest: open every tab you care about."),
                Bullet("Glamour chest and armoire."),
                Bullet("Chocobo saddlebag.")),
            Section("After that",
                Paragraph(
                    "The plugin keeps its own copy, so you can search your retainers' contents from anywhere without going back to a bell."))
        );
    }
}
