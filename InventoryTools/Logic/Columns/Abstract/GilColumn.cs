using System.Collections.Generic;
using CriticalCommonLib.Services.Mediator;

using Dalamud.Game.Text;
using ImGuiNET;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;
using OtterGui;

namespace InventoryTools.Logic.Columns.Abstract
{
    public abstract class GilColumn : IntegerColumn
    {
        public GilColumn(ILogger logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }
        public override List<MessageBase>? DoDraw(SearchResult searchResult, int? currentValue, int rowIndex,
            FilterConfiguration filterConfiguration, ColumnConfiguration columnConfiguration)
        {
            ImGui.TableNextColumn();
            if (ImGui.TableGetColumnFlags().HasFlag(ImGuiTableColumnFlags.IsEnabled))
            {
                if (currentValue != null)
                {
                    ImGuiUtil.RightAlign($"{currentValue.Value:n0}" + SeIconChar.Gil.ToIconString());
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