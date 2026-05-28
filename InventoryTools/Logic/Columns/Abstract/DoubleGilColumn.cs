using System.Collections.Generic;
using CriticalCommonLib.Services.Mediator;
using DalaMock.Host.Mediator;
using Dalamud.Game.Text;
using Dalamud.Bindings.ImGui;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;
using ImGuiUtil = InventoryTools.Ui.Widgets.ImGuiUtil;

namespace InventoryTools.Logic.Columns.Abstract
{
    public abstract class DoubleGilColumn : DoubleIntegerColumn
    {
        public DoubleGilColumn(ILogger logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }
        public override List<MessageBase>? DoDraw(SearchResult searchResult, (int, int)? currentValue, int rowIndex,
            FilterConfiguration filterConfiguration, ColumnConfiguration columnConfiguration)
        {
            ImGui.TableNextColumn();
            if (ImGui.TableGetColumnFlags().HasFlag(ImGuiTableColumnFlags.IsEnabled))
            {
                if (currentValue != null)
                {
                    var text = $"{currentValue.Value.Item1:n0}" + SeIconChar.Gil.ToIconString() + Divider +
                               $"{currentValue.Value.Item2:n0}" + SeIconChar.Gil.ToIconString();
                    var xOffset = ImGui.GetContentRegionAvail().X - ImGui.CalcTextSize(text).X - 22;
                    ImGui.AlignTextToFramePadding();
                    var posX = (ImGui.GetCursorPosX() + ImGui.GetColumnWidth() - ImGui.CalcTextSize(text).X - ImGui.GetScrollX() - 2 * ImGui.GetStyle().ItemSpacing.X);
                    if (posX > ImGui.GetCursorPosX())
                    {
                        ImGui.SetCursorPosX(posX);
                    }
                    ImGui.Text(text);
                }
                else
                {
                    var text = EmptyText;
                    var xOffset = ImGui.GetContentRegionAvail().X - ImGui.CalcTextSize(text).X - 22;
                    ImGui.AlignTextToFramePadding();
                    var posX = (ImGui.GetCursorPosX() + ImGui.GetColumnWidth() - ImGui.CalcTextSize(text).X - ImGui.GetScrollX() - 2 * ImGui.GetStyle().ItemSpacing.X);
                    if (posX > ImGui.GetCursorPosX())
                    {
                        ImGui.SetCursorPosX(posX);
                    }

                    ImGui.Text(text);
                }
            }

            return null;
        }
    }
}