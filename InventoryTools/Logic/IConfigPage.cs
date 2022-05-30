namespace InventoryTools.Logic
{
    public interface IConfigPage
    {
        public string Name { get; }
        public void Draw();
    }
}