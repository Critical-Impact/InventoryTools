using System.Collections.Generic;
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
    public abstract class TextIconColumn : Column<(string,ushort,bool)?>
    {
        public TextIconColumn(ILogger logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }
        
        public override string CsvExport(ColumnConfiguration columnConfiguration, SearchResult searchResult)
        {
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
        
        public override IEnumerable<SearchResult> Filter(ColumnConfiguration columnConfiguration, IEnumerable<SearchResult> searchResults)
        {
            return searchResults;
        }

        public override IEnumerable<SearchResult> Sort(ColumnConfiguration columnConfiguration,
            ImGuiSortDirection direction, IEnumerable<SearchResult> searchResults)
        {
            return searchResults;
        }

        public override List<MessageBase>? DoDraw(SearchResult searchResult, (string, ushort, bool)? currentValue, int rowIndex,
            FilterConfiguration filterConfiguration, ColumnConfiguration columnConfiguration)
        {
            ImGui.TableNextColumn();
            if (ImGui.TableGetColumnFlags().HasFlag(ImGuiTableColumnFlags.IsEnabled))
            {
                if (currentValue != null)
                {
                    ImGuiService.DrawIcon(currentValue.Value.Item2,
                        new Vector2(filterConfiguration.TableHeight, filterConfiguration.TableHeight) *
                        ImGui.GetIO().FontGlobalScale, currentValue.Value.Item3);
                    ImGui.SameLine();
                    if (filterConfiguration.FilterType == Logic.FilterType.CraftFilter)
                    {
                        ImGui.TextWrapped(currentValue.Value.Item1);
                    }
                    else
                    {
                        ImGui.Text(currentValue.Value.Item1);
                    }
                }
            }

            return null;
        }
    }
}