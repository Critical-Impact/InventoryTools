using System.Collections.Generic;
using CriticalCommonLib.Interfaces;
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
        public override List<MessageBase>? DoDraw(IItem item, int? currentValue, int rowIndex,
            FilterConfiguration filterConfiguration, ColumnConfiguration columnConfiguration)
        {
            if (ImGui.TableNextColumn())
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