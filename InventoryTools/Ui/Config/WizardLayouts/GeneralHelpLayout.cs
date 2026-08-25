using InventoryTools.Ui.Config.Layouts;

namespace InventoryTools.Ui.Config.WizardLayouts;

public class GeneralHelpLayout : ContentLayout
{
    public override PageLayout Build()
    {
        return Page("help/general", "General",
            Paragraph(
                "Allagan Tools is a multi-purpose plugin providing 3 primary features: tracking and displaying your inventory data, helping you plan crafts, and providing information about items."),
            Paragraph("If you've used Teamcraft or Garland Tools, it takes some inspiration from both."),
            Section("Inventory tracking",
                Paragraph(
                    "The plugin will do its best to keep track of your inventories. Some inventories are only cached when they are first accessed. If you aren't seeing your retainer, free company, glamour chest and so on, please be sure to view them first otherwise the plugin cannot cache them."),
                Paragraph(
                    "Once the plugin knows about the items, you can create lists to narrow down searches for specific items, help you sort them, and a myriad of other things.")),
            Section("Craft planning",
                Paragraph(
                    "The plugin has a dedicated crafts window that lets you create lists of items you want to craft. It'll create a plan that breaks each item down into its individual parts and tell you what you're missing, where everything you need is, and where to find or buy anything you don't have."),
                Paragraph("If you've ever used Teamcraft, you should be right at home.")),
            Section("Item information",
                Paragraph(
                    "The plugin has a fairly comprehensive database of information about each item. If you've used Garland Tools, the information provided is very similar. Clicking an item's icon within the plugin will always open the item's information window.")),
            Section("Highlighting",
                Paragraph(
                    "When using either an item list or a craft list, you can toggle highlighting. This will highlight the items in game so that you can see exactly where they are. When the plugin's windows are active, hit the 'Highlight' checkbox to activate highlighting for that list. To trigger this with a macro, see the commands section of help and toggle 'background' highlighting.")),
            Section("More",
                Paragraph("This is a very basic guide. For more information please see the wiki."),
                Link("Open Wiki", "https://github.com/Critical-Impact/InventoryTools/wiki/1.-Overview"))
        );
    }
}
