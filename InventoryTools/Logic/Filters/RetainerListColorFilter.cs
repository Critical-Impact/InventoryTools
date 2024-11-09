using System.Numerics;
using AllaganLib.GameSheets.Sheets.Rows;
using CriticalCommonLib.Models;

using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters
{
    public class RetainerListColorFilter : ColorFilter
    {
        public override string Key { get; set; } = "RetainerColor";
        public override string Name { get; set; } = "Retainer List Color";

        public override string HelpText { get; set; } =
            "The color to set the retainers in the retainer list to for this specific filter.";

        public override FilterCategory FilterCategory { get; set; } = FilterCategory.Display;

        public override bool? FilterItem(FilterConfiguration configuration, InventoryItem item)
        {
            return null;
        }

        public override bool? FilterItem(FilterConfiguration configuration, ItemRow item)
        {
            return null;
        }
        
        
        public override Vector4? CurrentValue(FilterConfiguration configuration)
        {
            return configuration.RetainerListColor;
        }

        public override bool HasValueSet(FilterConfiguration configuration)
        {
            return configuration.RetainerListColor != null;
        }

        public override void UpdateFilterConfiguration(FilterConfiguration configuration, Vector4? newValue)
        {
            configuration.RetainerListColor = newValue;
        }

        public RetainerListColorFilter(ILogger<RetainerListColorFilter> logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }
    }
}