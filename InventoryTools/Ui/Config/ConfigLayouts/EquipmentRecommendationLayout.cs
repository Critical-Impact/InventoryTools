using InventoryTools.EquipmentSuggest;
using InventoryTools.Ui.Config.Layouts;

namespace InventoryTools.Ui.Config.ConfigLayouts;

public class EquipmentRecommendationLayout : ConfigLayout
{
    public override PageLayout Build()
    {
        return Page("equipment-recommendation", "Equipment Recommendation",
            Paragraph("Compares what you are wearing against what you could be wearing and suggests upgrades. These are the defaults the recommendation screen opens with."),
            Section("Defaults",
                Setting<EquipmentSuggestModeSetting>("Recommend by"),
                Setting<EquipmentSuggestViewModeSetting>("Layout"))
        );
    }
}
