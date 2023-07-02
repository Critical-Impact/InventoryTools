using Dalamud.Game.Text;
using ImGuiNET;
using OtterGui;

namespace InventoryTools.Logic.Columns.Abstract
{
    public abstract class GilColumn : IntegerColumn
    {
        public override IColumnEvent? DoDraw(int? currentValue, int rowIndex, FilterConfiguration filterConfiguration)
        {
            ImGui.TableNextColumn();
            if (currentValue != null)
            {
                ImGuiUtil.RightAlign($"{currentValue.Value:n0}" + SeIconChar.Gil.ToIconString());
            }
            else
            {
                ImGui.Text(EmptyText);
            }
            return null;
        }
    }
}