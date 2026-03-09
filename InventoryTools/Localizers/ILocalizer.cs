namespace InventoryTools.Localizers;

public interface ILocalizer<in T>
{
    public string Format(T instance);
}