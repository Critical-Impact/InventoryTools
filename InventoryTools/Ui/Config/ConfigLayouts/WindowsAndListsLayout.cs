using InventoryTools.Logic.Settings;
using InventoryTools.Ui.Config.Layouts;

namespace InventoryTools.Ui.Config.ConfigLayouts;

public class WindowsAndListsLayout : ConfigLayout
{
    public override PageLayout Build()
    {
        return Page("windows-lists", "Windows & Lists",
            Section("Layout",
                Setting<CraftWindowLayoutSetting>("Craft window"),
                Setting<FiltersWindowLayoutSetting>("Items window"),
                Setting<ShowFiltersTabSetting>(),
                Setting<CompendiumRowHeightSetting>("Compendium row height")),
            Section("Behaviour",
                Paragraph("Which lists are shown when, and how the windows respond to the escape key."),
                Setting<SwitchFiltersAutomaticallySetting>(),
                Setting<SwitchCraftListsAutomaticallySetting>()),
            Section("Ignore escape",
                Paragraph("Windows that should stay open when you press escape."),
                Setting<CraftWindowIgnoreEscapeSetting>("Craft window"),
                Setting<FiltersWindowIgnoreEscapeSetting>("Items window"),
                Setting<ItemWindowIgnoreEscapeSetting>("Item window"),
                Setting<FilterWindowIgnoreEscapeSetting>("List window")),
            Section("Active lists",
                Paragraph("Crafts made anywhere in the game count towards this list."),
                Setting<ActiveCraftListSetting>())
        );
    }
}