using Dalamud.Game.Text;
using ImGuiNET;
using OtterGui;
using ImGuiUtil = InventoryTools.Ui.Widgets.ImGuiUtil;

namespace InventoryTools.Logic.Columns.Abstract
{
    public abstract class DoubleGilColumn : DoubleIntegerColumn
    {
        public override IColumnEvent? DoDraw((int, int)? currentValue, int rowIndex,
            FilterConfiguration filterConfiguration)
        {
            ImGui.TableNextColumn();
            if (currentValue != null)
            {
                var text = $"{currentValue.Value.Item1:n0}" + SeIconChar.Gil.ToIconString() + Divider + $"{currentValue.Value.Item2:n0}" + SeIconChar.Gil.ToIconString();
                var xOffset = ImGui.GetContentRegionAvail().X - ImGui.CalcTextSize(text).X;
                ImGuiUtil.VerticalAlignText(text, filterConfiguration.TableHeight, false, xOffset);
            }
            else
            {
                var text = EmptyText;
                var xOffset = ImGui.GetContentRegionAvail().X - ImGui.CalcTextSize(text).X;
                ImGuiUtil.VerticalAlignText(text, filterConfiguration.TableHeight, false, xOffset);
            }
            return null;
        }
    }
}