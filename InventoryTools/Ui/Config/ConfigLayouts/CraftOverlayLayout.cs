using InventoryTools.Logic.Settings;
using InventoryTools.Ui.Config.Layouts;

namespace InventoryTools.Ui.Config.ConfigLayouts;

public class CraftOverlayLayout : ConfigLayout
{
    public override PageLayout Build()
    {
        return Page("craft-overlay", "Craft Overlay",
            Paragraph("A compact panel that follows your active craft list while you are in the game, so you can see what is still needed without opening a window."),
            Section("Display",
                Setting<CraftOverlayMaxExpandedItemsSetting>("Items shown when expanded"),
                Setting<CraftOverlayHideSetting>("Hide during duties and cutscenes")),
            Section("Behaviour",
                Setting<CraftOverlayRememberStateSetting>("Stay open across reloads"))
        );
    }
}
