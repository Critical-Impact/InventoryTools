using System.Collections.Generic;
using System.Linq;
using AllaganLib.Shared.Extensions;
using CriticalCommonLib.Services.Mediator;

using ImGuiNET;
using InventoryTools.Extensions;
using InventoryTools.Services;
using InventoryTools.Ui.Widgets;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns.Abstract
{
    public abstract class DoubleIntegerColumn : Column<(int,int)?>
    {
        public DoubleIntegerColumn(ILogger logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }

        public override string CsvExport(ColumnConfiguration columnConfiguration, SearchResult searchResult)
        {
            return (CurrentValue(columnConfiguration, searchResult)?.Item1.ToString()  ?? "") + "/" + (CurrentValue(columnConfiguration, searchResult)?.Item2.ToString() ?? "");
        }

        public virtual string Divider => "/";

        public virtual string EmptyText => "";

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

                return currentValue.Value.Item1.PassesFilter(columnConfiguration.FilterText) || currentValue.Value.Item2.PassesFilter(columnConfiguration.FilterText);
            });
        }


        public override IEnumerable<SearchResult> Sort(ColumnConfiguration columnConfiguration,
            ImGuiSortDirection direction, IEnumerable<SearchResult> items)
        {
            return direction == ImGuiSortDirection.Ascending ? items.OrderBy(item =>
            {
                var currentValue = CurrentValue(columnConfiguration, item);
                if (currentValue == null)
                {
                    return 0;
                }

                return currentValue.Value.Item1;
            }) : items.OrderByDescending(item =>
            {
                var currentValue = CurrentValue(columnConfiguration, item);
                if (currentValue == null)
                {
                    return 0;
                }

                return currentValue.Value.Item1;
            });
        }

        public override List<MessageBase>? DoDraw(SearchResult searchResult, (int, int)? currentValue, int rowIndex,
            FilterConfiguration filterConfiguration, ColumnConfiguration columnConfiguration)
        {
            ImGui.TableNextColumn();
            if (ImGui.TableGetColumnFlags().HasFlag(ImGuiTableColumnFlags.IsEnabled))
            {
                if (currentValue != null)
                {
                    var text = $"{currentValue.Value.Item1:n0}" + Divider + $"{currentValue.Value.Item2:n0}";
                    ImGuiUtil.VerticalAlignText(text, filterConfiguration.TableHeight, false);
                }
                else
                {
                    ImGuiUtil.VerticalAlignText(EmptyText, filterConfiguration.TableHeight, false);
                }
            }

            return null;
        }


    }
}