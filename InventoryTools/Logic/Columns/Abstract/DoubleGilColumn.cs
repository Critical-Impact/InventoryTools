using Dalamud.Game.Text;
using ImGuiNET;

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
                
                ImGui.Text($"{currentValue.Value.Item1:n0}" + SeIconChar.Gil.ToIconString() + Divider + $"{currentValue.Value.Item2:n0}" + SeIconChar.Gil.ToIconString());
            }
            else
            {
                ImGui.Text(EmptyText);
            }
            return null;
        }
    }
}