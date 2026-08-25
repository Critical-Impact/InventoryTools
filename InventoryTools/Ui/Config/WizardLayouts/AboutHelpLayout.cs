using InventoryTools.Ui.Config.Layouts;

namespace InventoryTools.Ui.Config.WizardLayouts;

public class AboutHelpLayout : ContentLayout
{
    public override PageLayout Build()
    {
        return Page("help/about", "About",
            Paragraph(
                "This plugin is written in some of the free time that I have. It's a labour of love and I will hopefully be actively releasing updates for a while."),
            Paragraph("If you run into any issues please submit feedback via the plugin installer feedback button."),
            Section("Links",
                Link("Open the wiki", "https://github.com/Critical-Impact/InventoryTools/wiki/1.-Overview"),
                Link("Report a bug", "https://github.com/Critical-Impact/InventoryTools/issues"))
        );
    }
}
