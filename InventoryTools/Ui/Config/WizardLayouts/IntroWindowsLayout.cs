using InventoryTools.EquipmentSuggest;
using InventoryTools.Ui.Config.Layouts;

namespace InventoryTools.Ui.Config.WizardLayouts;

public class IntroWindowsLayout : ContentLayout
{
    public override PageLayout Build()
    {
        return Page("intro/windows", "Other tools",
            Paragraph("A couple of calculators sit outside lists, crafts and the compendium. Both are in the menu button at the bottom of any Allagan Tools window."),
            Section("Chocobo colour calculator",
                Paragraph("Pick the colour you want your chocobo to be and it works out which fruit to feed, in which order. Lock the sequence in and it tracks your progress, ticking off each fruit as you go."),
                OpenWindow<ChocoboColourWindow>("Open the chocobo colour calculator")),
            Section("Equipment recommendations",
                Paragraph("Pick a class and it looks through everything you own on any character and in any retainer for gear that beats what you are wearing, weighing item level against the stats that class actually cares about."),
                OpenWindow<EquipmentSuggestWindow>("Open equipment recommendations")),
            Section("Two things worth knowing",
                Paragraph("Right-clicking an item or a table row almost always offers more than you would expect, and clicking an item's icon anywhere in the plugin opens its full information page."))
        );
    }
}
