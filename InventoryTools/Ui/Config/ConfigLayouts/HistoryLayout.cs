using InventoryTools.Logic.Settings;
using InventoryTools.Ui.Config.Layouts;

namespace InventoryTools.Ui.Config.ConfigLayouts;

public class HistoryLayout : ConfigLayout
{
    public override PageLayout Build()
    {
        return Page("history", "History",
            Paragraph("Records items moving into, out of and around your inventories so you can look back at what changed. History lists then read from that record."),
            Section("Tracking",
                Setting<HistoryEnabledSetting>("Track inventory changes"),
                EnabledBy<HistoryEnabledSetting>(
                    Setting<HistoryTrackEventsSetting>("Events worth recording")))
        );
    }
}
