namespace InventoryTools.Compendium.Interfaces;

public interface IMenuProvider<in T>
{
    public void DrawMenu(T item);
    public void Open(T item);
    public void Draw(T item);
    public string PopupName { get; }
}