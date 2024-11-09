using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using CriticalCommonLib.Services.Mediator;

using ImGuiNET;
using InventoryTools.Extensions;
using InventoryTools.Services;
using InventoryTools.Ui.Widgets;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns.Abstract
{
    public abstract class ColoredTextColumn : Column<(string, Vector4)?>
    {
        public ColoredTextColumn(ILogger logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }

        public override string CsvExport(ColumnConfiguration columnConfiguration, SearchResult searchResult)
        {
            return CurrentValue(columnConfiguration, searchResult)?.Item1 ?? "";
        }

        public override dynamic? JsonExport(ColumnConfiguration columnConfiguration, SearchResult searchResult)
        {
            return CurrentValue(columnConfiguration, searchResult)?.Item1 ?? "";
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

                return currentValue.Value.Item1.ToLower().PassesFilterComparisonText(columnConfiguration.FilterComparisonText);
            });
        }

        public override IEnumerable<SearchResult> Sort(ColumnConfiguration columnConfiguration,
            ImGuiSortDirection direction, IEnumerable<SearchResult> items)
        {
            return direction == ImGuiSortDirection.Ascending ? items.OrderBy(item =>
            {
                var currentValue = CurrentValue(columnConfiguration, item);
                return !currentValue.HasValue ? "" : currentValue.Value.Item1;
            }) : items.OrderByDescending(item =>
            {
                var currentValue = CurrentValue(columnConfiguration, item);
                return !currentValue.HasValue ? "" : currentValue.Value.Item1;
            });
        }

        public override List<MessageBase>? DoDraw(SearchResult searchResults, (string, Vector4)? currentValue, int rowIndex,
            FilterConfiguration filterConfiguration, ColumnConfiguration columnConfiguration)
        {
            ImGui.TableNextColumn();
            if (ImGui.TableGetColumnFlags().HasFlag(ImGuiTableColumnFlags.IsEnabled)) {
                if (currentValue.HasValue)
                {
                    if (filterConfiguration.FilterType == Logic.FilterType.CraftFilter)
                    {
                        ImGuiUtil.VerticalAlignTextColored(currentValue.Value.Item1, currentValue.Value.Item2,
                            filterConfiguration.TableHeight, true);
                    }
                    else
                    {
                        ImGuiUtil.VerticalAlignTextColored(currentValue.Value.Item1, currentValue.Value.Item2,
                            filterConfiguration.TableHeight, false);
                    }
                }
                else
                {
                    ImGui.Text(EmptyText);
                }
            }

            return null;
        }


    }
}