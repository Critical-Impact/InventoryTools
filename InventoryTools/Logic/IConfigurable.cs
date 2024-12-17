namespace InventoryTools.Logic;

public interface IConfigurable<T>
{
    public T? Get(string key, T defaultValue);

    public void Set(string key, T? newValue);
}