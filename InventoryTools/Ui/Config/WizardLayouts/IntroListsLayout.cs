using InventoryTools.Ui.Config.Layouts;

namespace InventoryTools.Ui.Config.WizardLayouts;

public class IntroListsLayout : ContentLayout
{
    public override PageLayout Build()
    {
        return Page("intro/lists", "Lists",
            Paragraph(
                "Almost everything in the plugin hangs off a list. A list is a saved search over your items, and once you have one you can highlight its results in game, sort them, or track them."),
            Section("Three kinds",
                Bullet("Search list: find items across your inventories."),
                Bullet("Sort list: find items and work out where they should be moved to."),
                Bullet("Game item list: search every item in the game, not just the ones you own.")),
            Section("Have a look",
                Paragraph("The items window is where lists live. There are sample lists you can install as well."),
                OpenWindow<FiltersWindow>("Open the items window"))
        );
    }
}
