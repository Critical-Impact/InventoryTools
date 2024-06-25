using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Interfaces;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services.Mediator;
using CriticalCommonLib.Sheets;
using ImGuiNET;
using InventoryTools.Extensions;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns.Abstract
{
    using CriticalCommonLib.Time;

    public abstract class TimeIntervalColumn : Column<TimeInterval?>
    {
        public TimeIntervalColumn(ILogger logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }

        public override string CsvExport(ColumnConfiguration columnConfiguration, SearchResult searchResult)
        {
            var currentValue = CurrentValue(columnConfiguration, searchResult);
            if (currentValue != null)
            {
                return TimeInterval.DurationString(currentValue.Value.End, TimeStamp.UtcNow, true);
            }
            return "";
        }

        public virtual string EmptyText
        {
            get
            {
                return "N/A";
            }
        }
        public override List<MessageBase>? Draw(FilterConfiguration configuration,
            ColumnConfiguration columnConfiguration,
            SearchResult searchResult, int rowIndex, int columnIndex)
        {
            return DoDraw(searchResult, CurrentValue(columnConfiguration, searchResult), rowIndex, configuration, columnConfiguration);
        }

        public override IEnumerable<SearchResult> Filter(ColumnConfiguration columnConfiguration, IEnumerable<SearchResult> items)
        {
            return columnConfiguration.FilterText == "" ? items : items.Where(c =>
            {
                var currentValue = CurrentValue(columnConfiguration, c);
                if (currentValue == null)
                {
                    return false;
                }
                return currentValue.Value.PassesFilter(columnConfiguration.FilterText.ToLower());
            });
        }

        public override IEnumerable<SearchResult> Sort(ColumnConfiguration columnConfiguration,
            ImGuiSortDirection direction, IEnumerable<SearchResult> searchResults)
        {
            return direction == ImGuiSortDirection.Ascending ? searchResults.OrderBy(item => CurrentValue(columnConfiguration, item)) : searchResults.OrderByDescending(item => CurrentValue(columnConfiguration, item));
        }

        public override void Setup(FilterConfiguration filterConfiguration, ColumnConfiguration columnConfiguration,
            int columnIndex)
        {
            ImGui.TableSetupColumn(columnConfiguration.Name ?? (RenderName ?? Name) ?? Name, ImGuiTableColumnFlags.WidthFixed, Width, (uint)columnIndex);
        }
    }
}