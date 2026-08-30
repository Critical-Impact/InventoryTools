using InventoryTools.Logic.Settings;
using InventoryTools.Ui.Config.Layouts;

namespace InventoryTools.Ui.Config.ConfigLayouts;

public class WindowsAndListsLayout : ConfigLayout
{
    public override PageLayout Build()
    {
        return Page("windows-lists", "Windows & Lists",
            Section("Layout",
                Paragraph("Control how various windows are laid out."),
                Setting<CraftWindowLayoutSetting>("Craft window"),
                Setting<FiltersWindowLayoutSetting>("Items window"),
                Setting<ShowFiltersTabSetting>(),
                Setting<CompendiumRowHeightSetting>("Compendium row height")),
            Section("Auto-Switch",
                Paragraph("When switching between lists in the UI, if highlighting is on should we automatically switch highlighting to that list?"),
                Setting<SwitchFiltersAutomaticallySetting>(),
                Setting<SwitchCraftListsAutomaticallySetting>()),
            Section("Ignore escape",
                Paragraph("Windows that should stay open when you press escape."),
                Setting<CraftWindowIgnoreEscapeSetting>("Craft window"),
                Setting<FiltersWindowIgnoreEscapeSetting>("Items window"),
                Setting<ItemWindowIgnoreEscapeSetting>("Item window"),
                Setting<FilterWindowIgnoreEscapeSetting>("List window"))
        );
    }
}