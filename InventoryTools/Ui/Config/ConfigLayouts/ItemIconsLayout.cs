using InventoryTools.Logic.Settings;
using InventoryTools.Ui.Config.Layouts;

namespace InventoryTools.Ui.Config.ConfigLayouts;

public class ItemIconsLayout : ConfigLayout
{
    public override PageLayout Build()
    {
        return Page("item-icons", "Item Icons",
            Paragraph("Where an item can be acquired or used in several similar ways, the icons for those can be collapsed into a single grouped icon. Each source and use below can override that default."),
            Section("Sources",
                Scrollable("sourceIconGrouping", 320,
                    Setting<SourceIconGroupingSetting>())),
            Section("Uses",
                Scrollable("useIconGrouping", 200,
                    Setting<UseIconGroupingSetting>()))
        );
    }
}