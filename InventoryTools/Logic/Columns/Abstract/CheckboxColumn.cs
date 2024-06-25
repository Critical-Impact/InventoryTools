using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Interfaces;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services.Mediator;
using CriticalCommonLib.Sheets;
using ImGuiNET;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns.Abstract
{
    public abstract class CheckboxColumn : Column<bool?>
    {
        public CheckboxColumn(ILogger logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }
        public override string CsvExport(ColumnConfiguration columnConfiguration, SearchResult searchResult)
        {
            return CurrentValue(columnConfiguration, searchResult) ?? false ? "true" : "false";
        }

        public override IEnumerable<SearchResult> Filter(ColumnConfiguration columnConfiguration, IEnumerable<SearchResult> searchResults)
        {
            bool isChecked;
            if (columnConfiguration.FilterText == "")
            {
                return searchResults;
            }

            if (columnConfiguration.FilterText == "true")
            {
                isChecked = true;
            }
            else
            {
                isChecked = false;
            }
            return columnConfiguration.FilterText == "" ? searchResults : searchResults.Where(c =>
            {
                var currentValue = CurrentValue(columnConfiguration, c);
                if (!currentValue.HasValue)
                {
                    return false;
                }

                if (isChecked)
                {
                    return currentValue.Value;
                }
                return !currentValue.Value;
            });
        }
        
        private int GetSortOrder(ColumnConfiguration columnConfiguration, SearchResult c)
        {
            var currentValue = CurrentValue(columnConfiguration, c);
            return currentValue switch
            {
                null => 0,
                false => 1,
                _ => 2
            };
        }

        public override IEnumerable<SearchResult> Sort(ColumnConfiguration columnConfiguration,
            ImGuiSortDirection direction, IEnumerable<SearchResult> items)
        {
            return direction == ImGuiSortDirection.Ascending ? items.OrderBy(c => GetSortOrder(columnConfiguration, c)) : items.OrderByDescending(c => GetSortOrder(columnConfiguration, c));
        }

        public override List<MessageBase>? Draw(FilterConfiguration configuration, ColumnConfiguration columnConfiguration, SearchResult searchResult,
            int rowIndex, int columnIndex)
        {
            return DoDraw(searchResult, CurrentValue(columnConfiguration, searchResult), rowIndex, configuration, columnConfiguration);
        }

        public override List<MessageBase>? DoDraw(SearchResult searchResult, bool? currentValue, int rowIndex,
            FilterConfiguration filterConfiguration, ColumnConfiguration columnConfiguration)
        {
            ImGui.TableNextColumn();
            if (ImGui.TableGetColumnFlags().HasFlag(ImGuiTableColumnFlags.IsEnabled))
            {
                if (currentValue.HasValue)
                {
                    ImGui.SetCursorPosX(ImGui.GetCursorPosX() + (ImGui.GetContentRegionAvail().X / 2) -
                                        filterConfiguration.TableHeight / 2.0f);
                    ImGuiService.DrawUldIcon(currentValue.Value ? ImGuiService.TickIcon : ImGuiService.CrossIcon,
                        new Vector2(filterConfiguration.TableHeight, filterConfiguration.TableHeight));
                }
            }

            return null;
        }


    }
}