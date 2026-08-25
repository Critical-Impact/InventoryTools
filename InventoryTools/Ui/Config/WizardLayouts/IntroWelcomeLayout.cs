using InventoryTools.Ui.Config.Layouts;

namespace InventoryTools.Ui.Config.WizardLayouts;

public class IntroWelcomeLayout : ContentLayout
{
    public override PageLayout Build()
    {
        return Page("intro/welcome", "Overview",
            Paragraph(
                "Allagan Tools does three main things. The next few pages cover each briefly. It should take a minute, and all of it is in the Help window later if you want more detail."),
            Section("Track your inventories",
                Paragraph(
                    "Every item across your characters, retainers, free company, saddlebag and glamour chest, searchable in one place.")),
            Section("Plan your crafts",
                Paragraph(
                    "Break a craft down into every sub-item, see what you already own, and find where the rest can be bought or gathered.")),
            Section("Look things up",
                Paragraph(
                    "A database covering items, monsters, duties, NPCs, airships and more telling you where things come from and what they are used for."))
        );
    }
}
