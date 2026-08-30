using InventoryTools.Ui.Config.Layouts;

namespace InventoryTools.Ui.Config.WizardLayouts;

public class IntroDefaultsLayout : ContentLayout
{
    public override PageLayout Build()
    {
        return Page("intro/defaults", "Defaults",
            Paragraph("By default, the plugin is configured with a default set of features enabled."),
            Paragraph("The next screens will show you the settings for the most commonly used features."),
            Paragraph("Hover the ? icons to get further information about what each setting does.")
        );
    }
}