using Dalamud.Game.Text;
using ImGuiNET;

namespace InventoryTools.Logic.Columns
{
    public abstract class GilColumn : IntegerColumn
    {
        public override IColumnEvent? DoDraw(int? currentValue, int rowIndex)
        {
            ImGui.TableNextColumn();
            if (currentValue != null)
            {
                
                ImGui.Text(currentValue.Value + SeIconChar.Gil.ToIconString());
            }
            else
            {
                ImGui.Text(EmptyText);
            }
            return null;
        }
    }
}