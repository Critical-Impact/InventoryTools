using CriticalCommonLib.Models;
using Lumina.Excel.GeneratedSheets;

namespace InventoryTools.Logic.Columns
{
    public class SourceColumn : TextColumn
    {
        public override string? CurrentValue(InventoryItem item)
        {
            return PluginService.CharacterMonitor.Characters.ContainsKey(item.RetainerId) ?  PluginService.CharacterMonitor.Characters[item.RetainerId].Name : "Unknown (" + item.RetainerId + ")";
        }

        public override string? CurrentValue(Item item)
        {
            return null;
        }

        public override string? CurrentValue(SortingResult item)
        {
            return CurrentValue(item.InventoryItem);
        }

        public override string Name { get; set; } = "Source";
        public override float Width { get; set; } = 100.0f;
        public override string FilterText { get; set; } = "";
        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
    }
}