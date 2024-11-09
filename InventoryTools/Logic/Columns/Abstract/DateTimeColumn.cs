using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AllaganLib.Shared.Extensions;
using CriticalCommonLib.Services.Mediator;

using ImGuiNET;
using InventoryTools.Extensions;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns.Abstract
{
    public abstract class DateTimeColumn : Column<DateTime?>
    {
        public DateTimeColumn(ILogger logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }

        public override string CsvExport(ColumnConfiguration columnConfiguration, SearchResult searchResult)
        {
            return CurrentValue(columnConfiguration, searchResult)?.ToString(CultureInfo.CurrentCulture) ?? "";
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
            ImGuiSortDirection direction, IEnumerable<SearchResult> items)
        {
            return direction == ImGuiSortDirection.Ascending ? items.OrderBy(item => CurrentValue(columnConfiguration, item)) : items.OrderByDescending(item => CurrentValue(columnConfiguration, item));
        }

        public override List<MessageBase>? DoDraw(SearchResult searchResult, DateTime? currentValue, int rowIndex,
            FilterConfiguration filterConfiguration, ColumnConfiguration columnConfiguration)
        {
            ImGui.TableNextColumn();
            if (ImGui.TableGetColumnFlags().HasFlag(ImGuiTableColumnFlags.IsEnabled))
            {
                if (currentValue != null)
                {
                    var formattedValue = currentValue.Value.ToString(CultureInfo.CurrentCulture);
                    var columnWidth = ImGui.GetColumnWidth();
                    var frameHeight = filterConfiguration.TableHeight / 2.0f;
                    var calcText = ImGui.CalcTextSize(formattedValue);
                    var textHeight = calcText.X >= columnWidth ? 0 : calcText.Y / 2.0f;
                    ImGui.SetCursorPosY(ImGui.GetCursorPosY() + frameHeight - textHeight);
                    ImGui.TextUnformatted(formattedValue);
                }
                else
                {
                    ImGui.TextUnformatted(EmptyText);
                }
            }

            return null;
        }

        public override void Setup(FilterConfiguration filterConfiguration, ColumnConfiguration columnConfiguration,
            int columnIndex)
        {
            ImGui.TableSetupColumn(columnConfiguration.Name ?? (RenderName ?? Name) ?? Name, ImGuiTableColumnFlags.WidthFixed, Width, (uint)columnIndex);
        }
    }
}