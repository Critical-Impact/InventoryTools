using InventoryTools.Ui.Config.Layouts;

namespace InventoryTools.Ui.Config.WizardLayouts;

public class IntroCraftingLayout : ContentLayout
{
    public override PageLayout Build()
    {
        return Page("intro/crafting", "Crafting",
            Paragraph("A craft list takes what you want to make and breaks it into every intermediate item and raw material, then checks that against what you already own."),
            Section("What you get",
                Bullet("A full material tree, not just the immediate ingredients."),
                Bullet("What you already have, and which character or retainer is holding it."),
                Bullet("Where to buy or gather whatever is missing."),
                Bullet("Progress ticking down as you acquire things.")),
            Section("Have a look",
                OpenWindow<CraftsWindow>("Open the crafts window"))
        );
    }
}
