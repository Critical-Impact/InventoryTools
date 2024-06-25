using System;
using System.Collections.Generic;
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
using NaturalSort.Extension;

namespace InventoryTools.Logic.Columns.Abstract
{
    public abstract class TextColumn : Column<string?>
    {
        public TextColumn(ILogger logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }
        public override string CsvExport(ColumnConfiguration columnConfiguration, SearchResult searchResult)
        {
            return CurrentValue(columnConfiguration, searchResult) ?? "";
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

        public override IEnumerable<SearchResult> Filter(ColumnConfiguration columnConfiguration, IEnumerable<SearchResult> searchResults)
        {
            return columnConfiguration.FilterText == "" ? searchResults : searchResults.Where(c =>
            {
                var currentValue = CurrentValue(columnConfiguration, c);
                if (currentValue == null)
                {
                    return false;
                }

                if (FilterType == ColumnFilterType.Choice)
                {
                    return currentValue == columnConfiguration.FilterText;
                }
                return currentValue.ToLower().PassesFilter(columnConfiguration.FilterText.ToLower());
            });
        }

        public override IEnumerable<SearchResult> Sort(ColumnConfiguration columnConfiguration,
            ImGuiSortDirection direction, IEnumerable<SearchResult> searchResults)
        {
            return direction == ImGuiSortDirection.Ascending ? searchResults.OrderBy(item => CurrentValue(columnConfiguration, item), StringComparison.OrdinalIgnoreCase.WithNaturalSort()) : searchResults.OrderByDescending(item => CurrentValue(columnConfiguration, item), StringComparison.OrdinalIgnoreCase.WithNaturalSort());
        }

        public override List<MessageBase>? DoDraw(SearchResult searchResult, string? currentValue, int rowIndex,
            FilterConfiguration filterConfiguration, ColumnConfiguration columnConfiguration)
        {
            ImGui.TableNextColumn();
            if (ImGui.TableGetColumnFlags().HasFlag(ImGuiTableColumnFlags.IsEnabled))
            {
                if (currentValue != null)
                {
                    ImGui.AlignTextToFramePadding();
                    ImGui.TextUnformatted(currentValue);
                }
                else
                {
                    ImGui.TextUnformatted(EmptyText);
                }
            }

            return null;
        }


    }
}