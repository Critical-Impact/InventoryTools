using System.Collections.Generic;
using System.Numerics;
using CriticalCommonLib.Services.Mediator;

using ImGuiNET;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns.Abstract
{
    public abstract class GameIconColumn : Column<(ushort,bool)?>
    {
        public GameIconColumn(ILogger logger, ImGuiService imGuiService) : base(logger, imGuiService)
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

        public override List<MessageBase>? DoDraw(SearchResult searchResult, (ushort, bool)? currentValue, int rowIndex,
            FilterConfiguration filterConfiguration, ColumnConfiguration columnConfiguration)
        {
            ImGui.TableNextColumn();
            if (ImGui.TableGetColumnFlags().HasFlag(ImGuiTableColumnFlags.IsEnabled))
            {
                if (currentValue != null)
                {
                    ImGuiService.DrawIcon(currentValue.Value.Item1,
                        new Vector2(filterConfiguration.TableHeight, filterConfiguration.TableHeight) *
                        ImGui.GetIO().FontGlobalScale, currentValue.Value.Item2);
                }
            }

            return null;
        }


    }
}