using DalaMock.Host.Mediator;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using InventoryTools.Compendium.Interfaces;

namespace InventoryTools.Compendium.Models;

public abstract class MenuProvider<T>(MediatorService mediatorService) : IMenuProvider<T>
{
    public MediatorService MediatorService { get; } = mediatorService;

    public abstract void DrawMenu(T item);

    public void Open(T item)
    {
        ImGui.OpenPopup(PopupName);
    }

    public void Draw(T item)
    {
        using (var popup = ImRaii.Popup(PopupName))
        {
            if (popup)
            {
                DrawMenu(item);
            }
        }
    }

    public abstract string PopupName { get; }
}