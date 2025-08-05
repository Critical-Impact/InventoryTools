using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AllaganLib.Shared.Extensions;
using CriticalCommonLib.Services.Mediator;
using DalaMock.Host.Mediator;
using Dalamud.Bindings.ImGui;
using InventoryTools.Extensions;
using InventoryTools.Services;
using InventoryTools.Ui.Widgets;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns.Abstract
{
    public abstract class DecimalColumn : Column<decimal?>
    {
        public DecimalColumn(ILogger logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }

        public override string CsvExport(ColumnConfiguration columnConfiguration, SearchResult searchResult)
        {
            return CurrentValue(columnConfiguration, searchResult).ToString() ?? "";
        }

        public virtual string EmptyText
        {
            get
            {
                return "";
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

                return currentValue.Value.PassesFilter(columnConfiguration.FilterText);
            });
        }

        public override IEnumerable<SearchResult> Sort(ColumnConfiguration columnConfiguration,
            ImGuiSortDirection direction, IEnumerable<SearchResult> searchResults)
        {
            return direction == ImGuiSortDirection.Ascending ? searchResults.OrderBy(c => CurrentValue(columnConfiguration, c) ?? Int32.MaxValue) : searchResults.OrderByDescending(c => CurrentValue(columnConfiguration, c) ?? Int32.MinValue);
        }

        public override List<MessageBase>? DoDraw(SearchResult searchResult, decimal? currentValue, int rowIndex,
            FilterConfiguration filterConfiguration, ColumnConfiguration columnConfiguration)
        {
            ImGui.TableNextColumn();
            if (ImGui.TableGetColumnFlags().HasFlag(ImGuiTableColumnFlags.IsEnabled))
            {
                if (currentValue != null)
                {
                    var fmt = currentValue.Value.ToString("N2", CultureInfo.InvariantCulture);
                    ImGuiUtil.VerticalAlignText(fmt, filterConfiguration.TableHeight, false);

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