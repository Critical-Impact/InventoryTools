using InventoryTools.Ui.Config.Layouts;

namespace InventoryTools.Ui.Config.WizardLayouts;

public class ListBasicsHelpLayout : ContentLayout
{
    public override PageLayout Build()
    {
        return Page("help/list-basics", "List Basics",
            Paragraph("Lists are the core way the plugin lets you view the items you are looking for, or are attempting to sort. There are currently 3 types of list that can be created."),
            Section("Search List",
                Paragraph("Allows you to search for specific items across all your inventories. If you just need to find an item but don't want help sorting it, this is the list type you want."),
                Paragraph("Example uses:"),
                Bullet("Finding materials for a craft."),
                Bullet("Finding a housing item you put somewhere."),
                Bullet("Seeing how much an item you just picked up is worth."),
                Bullet("Seeing if a specific item is already in your glamour chest or armoire."),
                Bullet("Checking your retainers' equipment without going to a retainer bell."),
                Bullet("Checking if any items you have can go into the armoire.")),
            Section("Sort List",
                Paragraph("Builds on the search list, but also lets you pick where you want the items to be sorted. It'll attempt to show you the most optimised plan for storing the items in the destinations you pick."),
                Paragraph("Example uses:"),
                Bullet("Putting away materials after a craft without having them double up."),
                Bullet("Storing items above a certain item level in your chocobo saddlebag for later."),
                Bullet("Finding items that are unique to your free company chest and putting them there.")),
            Section("Game Item List",
                Paragraph("Lets you search across all the items that exist within the game's catalogue."),
                Paragraph("Example uses:"),
                Bullet("Searching for glamours."),
                Bullet("Seeing what mounts and minions you haven't obtained."),
                Bullet("Tracking the prices of all the items within the game."))
        );
    }
}
