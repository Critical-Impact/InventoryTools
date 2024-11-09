using System.Collections.Generic;
using System.Linq;
using AllaganLib.GameSheets.Sheets.Rows;
using CriticalCommonLib.Models;
using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters
{
    public class SourceWorldFilter : MultipleChoiceFilter<uint>
    {
        private readonly ExcelSheet<World> _worldSheet;

        public SourceWorldFilter(ILogger<SourceWorldFilter> logger, ImGuiService imGuiService, ExcelSheet<World> worldSheet) : base(logger, imGuiService)
        {
            _worldSheet = worldSheet;
        }

        public override List<uint> CurrentValue(FilterConfiguration configuration)
        {
            return configuration.SourceWorlds?.ToList() ?? new List<uint>();
        }

        public override void UpdateFilterConfiguration(FilterConfiguration configuration, List<uint> newValue)
        {
            configuration.SourceWorlds = newValue.Count == 0 ? null : newValue.Distinct().ToHashSet();
        }

        public override void ResetFilter(FilterConfiguration configuration)
        {
            UpdateFilterConfiguration(configuration, new List<uint>());
        }

        public override int LabelSize { get; set; } = 240;
        public override string Key { get; set; } = "SourceWorlds";
        public override string Name { get; set; } = "Source - Worlds";
        public override string HelpText { get; set; } =
            "This is a list of sources worlds to search in. It will attempt to search for items in any bag of any character/retainer on that world.";

        public override FilterCategory FilterCategory { get; set; } = FilterCategory.Inventories;
        public override List<uint> DefaultValue { get; set; } = new();
        public override FilterType AvailableIn { get; set; } = FilterType.SearchFilter | FilterType.SortingFilter | FilterType.CraftFilter | FilterType.HistoryFilter;

        public override bool? FilterItem(FilterConfiguration configuration, InventoryItem item)
        {
            return null;
        }

        public override bool? FilterItem(FilterConfiguration configuration, ItemRow item)
        {
            return null;
        }


        private Dictionary<uint, string>? _choices;
        public override Dictionary<uint, string> GetChoices(FilterConfiguration configuration)
        {
            if (_choices == null)
            {
                _choices = _worldSheet.Where(c => c.IsPublic).OrderBy(c => c.Name.ExtractText()).ToDictionary(c => c.RowId, c => c.Name.ExtractText());
            }

            return _choices;
        }

        public override bool HideAlreadyPicked { get; set; } = true;
    }
}