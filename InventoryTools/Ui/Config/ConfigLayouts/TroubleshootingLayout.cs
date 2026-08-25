using InventoryTools.Logic.Settings;
using InventoryTools.Ui.Config.Layouts;

namespace InventoryTools.Ui.Config.ConfigLayouts;

public class TroubleshootingLayout : ConfigLayout
{
    public override PageLayout Build()
    {
        return Page("troubleshooting", "Troubleshooting",
            Paragraph("Timings that only need changing if you are seeing a specific symptom. The defaults suit almost everyone."),
            Section("Acquisition tracker",
                Paragraph("The acquisition tracker watches what you craft, gather and loot so craft lists can tick themselves off. Both values are in seconds, and raising them trades responsiveness for reliability on a slow connection or a busy machine."),
                Setting<AcquisitionTrackerLoginDelaySetting>("Wait this long after login before scanning"),
                Setting<AcquisitionTrackerPersistStateSetting>("Keep tracking this long after you stop"))
        );
    }
}
